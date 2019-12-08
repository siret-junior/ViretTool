using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Proxy;
using Castle.Services.Logging.NLogIntegration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Newtonsoft.Json;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Submission;

namespace KisQueryResultExtraction
{
    class Program
    {
        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            // TODO: print usage, parameter check
            string datasetDirectory = args[0];
            string rootInputDirectory = args[1];
            string rootOutputDirectory = args[2];
            Directory.CreateDirectory(rootOutputDirectory);

            // parse grid search arguments
            int[] vValues = new int[] { };
            int[] sValues = new int[] { };
            int[] kValues = new int[] { };
            int[] cValues = new int[] { };
            int[] eValues = new int[] { };
            int dropout = 0;

            foreach (string argument in args)
            {
                if (argument.StartsWith("-v"))
                {
                    vValues = ParseSteps(argument.Substring(2));
                }
                if (argument.StartsWith("-s"))
                {
                    sValues = ParseSteps(argument.Substring(2));
                }

                if (argument.StartsWith("-k"))
                {
                    kValues = ParseSteps(argument.Substring(2));
                }
                if (argument.StartsWith("-c"))
                {
                    cValues = ParseSteps(argument.Substring(2));
                }
                if (argument.StartsWith("-e"))
                {
                    eValues = ParseSteps(argument.Substring(2));
                }

                if (argument.StartsWith("-d"))
                {
                    dropout = int.Parse(argument.Substring(2));
                }
            }



            using (WindsorContainer container = new WindsorContainer(
                    new DefaultKernel(
                        new ArgumentPassingDependencyResolver(),
                        new NotSupportedProxyFactory()),
                    new DefaultComponentInstaller()))
            {
                // init business layer
                Console.WriteLine("Initializing ranking service...");
                container.AddFacility<TypedFactoryFacility>();
                container.AddFacility<LoggingFacility>(x => x.LogUsing<NLogFactory>().WithAppConfig());
                container.Install(FromAssembly.This());
                IDatasetServicesManager datasetServiceManager = container.Resolve<IDatasetServicesManager>();
                datasetServiceManager.OpenDataset(datasetDirectory);

                // init services
                DatasetServices dataset = datasetServiceManager.CurrentDataset;
                IQueryPersistingService queryPersistingService = container.Resolve<IQueryPersistingService>();
                IBiTemporalRankingService rankingService = dataset.RankingService;

                string displaysFile = Path.Combine(rootOutputDirectory, $"Displays-All-{DateTime.Now.Ticks}.csv");
                using (StreamWriter displayWriter = new StreamWriter(displaysFile))
                {
                    displayWriter.WriteLine("Vid;Shot;Key;Color;SemEx;Drop;V8;V16;V32;V64;V128;V256;V512;V1024;S8;S16;S32;S64;S128;S256;S512;S1024");

                    foreach (int vValue in vValues)
                    foreach (int sValue in sValues)
                    foreach (int kValue in kValues)
                    foreach (int cValue in cValues)
                    foreach (int eValue in eValues)
                    {
                        if (vValue < sValue) continue;

                        string configurationOutputDirectory = Path.Combine(rootOutputDirectory, $"Config-v{vValue}-s{sValue}-k{kValue}-c{cValue}-e{eValue}-d{dropout}");
                        Directory.CreateDirectory(configurationOutputDirectory);

                        string displayString = Extract(rootInputDirectory, configurationOutputDirectory, 
                            datasetServiceManager, dataset, queryPersistingService, rankingService,
                            vValue, sValue, kValue, cValue, eValue, dropout);

                        displayWriter.WriteLine(displayString);
                        displayWriter.Flush();
                    }
                }
            }
        }

        private static string Extract(
            string rootInputDirectory, string rootOutputDirectory, 
            IDatasetServicesManager datasetServiceManager, DatasetServices dataset,
            IQueryPersistingService queryPersistingService, IBiTemporalRankingService rankingService,
            int vValue, int sValue, int kValue, int cValue, int eValue, int dropout)
        {
            Random random = new Random(42);
            string displayString = "";

            // global accumulators
            Dictionary<int, int> videoRankAccumulator = new Dictionary<int, int>();
            Dictionary<int, int> shotRankAccumulator = new Dictionary<int, int>();

            // displayed accumulators
            Dictionary<int, int> displayedVideoAccumulator = new Dictionary<int, int>();
            Dictionary<int, int> displayedShotAccumulator = new Dictionary<int, int>();


            // compute result set for each query in all query directories
            string topRankedFilePath = Path.Combine(rootOutputDirectory, "topRankedFrames.txt");
            string displayedFilePath = Path.Combine(rootOutputDirectory, "displayedRankedFrames.txt");
            //using (StreamWriter topRankedWriter = new StreamWriter(topRankedFilePath))
            using (StreamWriter displayedWriter = new StreamWriter(displayedFilePath))
            {
                // print displayed configuration
                displayedWriter.Write($"{vValue};{sValue};{kValue};{cValue};{eValue};{dropout};");
                displayString += $"{vValue};{sValue};{kValue};{cValue};{eValue};{dropout};";

                // iterate users
                string[] userDirectories = Directory.GetDirectories(rootInputDirectory).OrderBy(x => x).ToArray();
                int iDirectory = 1;
                foreach (string userDirectory in userDirectories)
                {
                    int teamNumber = int.Parse(Path.GetFileName(userDirectory));
                    Console.WriteLine();
                    Console.WriteLine($"Processing {teamNumber} ({iDirectory++}/{userDirectories.Length})");
                    string outputUserDirectory = Path.Combine(rootOutputDirectory, Path.GetFileNameWithoutExtension(userDirectory));
                    Directory.CreateDirectory(outputUserDirectory);

                    // get the last task file
                    string tasksDirectory = Path.Combine(userDirectory, "TaskLogs");
                    string taskFile = Directory.GetFiles(tasksDirectory, "*.txt")
                        .Select(file => new DirectoryInfo(file))
                        .OrderByDescending(file => file.LastWriteTime)
                        .Last()
                        .ToString();
                    // load tasks
                    var tasks = JsonConvert.DeserializeObject<string[]>(File.ReadAllLines(taskFile)[0])
                    .Select(line => line.Split(';'))
                    .Select(item => new
                    {
                        taskId = int.Parse(item[2].Split('_')[0]),
                        videoId = int.Parse(item[3]) - 1,
                        startFrame = int.Parse(item[4]),
                        endFrame = int.Parse(item[5]),
                        startTimeStamp = long.Parse(item[6] + "000"),
                        endTimeStamp = long.Parse(item[7] + "000")
                    });

                    // local accumulators
                    //Dictionary<int, int> localVideoAccumulator = new Dictionary<int, int>();
                    //Dictionary<int, int> localShotAccumulator = new Dictionary<int, int>();

                    // compute result set for each query in query directory
                    //string localTopRankedFilePath = Path.Combine(outputUserDirectory, "topRankedFrames.txt");
                    //using (StreamWriter localTopRankedWriter = new StreamWriter(localTopRankedFilePath))
                    {
                        int iQuery = 0;
                        string[] queryFiles = Directory.GetFiles(Path.Combine(userDirectory, "QueriesLog"), "*.json").OrderBy(x => x).ToArray();
                        foreach (string queryFile in queryFiles)
                        {
                            iQuery++;
                            if (random.Next(100) < dropout)
                            {
                                continue;
                            }

                            Console.WriteLine($"Computing ranking for {Path.GetFileName(queryFile)} ({iQuery}/{queryFiles.Length})");
                            long queryTimestamp = long.Parse(Path.GetFileNameWithoutExtension(queryFile).Split('_')[1]);

                            // find correct task based on timestamp
                            dynamic task = tasks
                                .Where(t => queryTimestamp >= (long)t.startTimeStamp && queryTimestamp <= (long)t.endTimeStamp)
                                .First();

                            // compute ranking
                            BiTemporalQuery query = queryPersistingService.LoadQuery(queryFile);

                            // modify queries
                            //SetNoFilter(query);
                            //SetFiltersMiddle(query);
                            SetFilters(query, vValue, sValue, kValue / 100f, cValue / 100f, eValue / 100f);

                            BiTemporalRankedResultSet resultSet = rankingService.ComputeRankedResultSet(query);

                            //QueryResultVisualizer.Visualize(dataset, new Task(task), query, rankingService.BiTemporalRankingModule, 
                            //    Path.Combine(outputUserDirectory, $"visualization-u{teamNumber}-t{task.taskId}-{queryTimestamp}.jpg"));

                            // extract intermediate ranks
                            float[] keywordRanks;
                            float[] colorRanks;
                            float[] semanticExampleRanks;
                            float[] faceSketchRanks;
                            float[] textSketchRanks;
                            float[] fusionRanks;
                            float[] maskFilteringRanks;
                            float[] countFilteringRanks;
                            float[] outputRanks;
                            IBiTemporalRankingModule rankingModule = rankingService.BiTemporalRankingModule;
                            switch (query.PrimaryTemporalQuery)
                            {
                                case BiTemporalQuery.TemporalQueries.Former:
                                    keywordRanks = rankingModule.KeywordIntermediateRanking.FormerRankingBuffer.Ranks;
                                    colorRanks = rankingModule.ColorSketchIntermediateRanking.FormerRankingBuffer.Ranks;
                                    semanticExampleRanks = rankingModule.SemanticExampleIntermediateRanking.FormerRankingBuffer.Ranks;
                                    faceSketchRanks = rankingModule.FaceSketchIntermediateRanking.FormerRankingBuffer.Ranks;
                                    textSketchRanks = rankingModule.TextSketchIntermediateRanking.FormerRankingBuffer.Ranks;
                                    fusionRanks = rankingModule.IntermediateFusionRanking.FormerRankingBuffer.Ranks;
                                    maskFilteringRanks = rankingModule.FormerFilteringModule.MaskIntermediateRanking.Ranks;
                                    countFilteringRanks = rankingModule.OutputRanking.FormerRankingBuffer.Ranks;
                                    break;

                                case BiTemporalQuery.TemporalQueries.Latter:
                                    keywordRanks = rankingModule.KeywordIntermediateRanking.LatterRankingBuffer.Ranks;
                                    colorRanks = rankingModule.ColorSketchIntermediateRanking.LatterRankingBuffer.Ranks;
                                    semanticExampleRanks = rankingModule.SemanticExampleIntermediateRanking.LatterRankingBuffer.Ranks;
                                    faceSketchRanks = rankingModule.FaceSketchIntermediateRanking.LatterRankingBuffer.Ranks;
                                    textSketchRanks = rankingModule.TextSketchIntermediateRanking.LatterRankingBuffer.Ranks;
                                    fusionRanks = rankingModule.IntermediateFusionRanking.LatterRankingBuffer.Ranks;
                                    maskFilteringRanks = rankingModule.LatterFilteringModule.MaskIntermediateRanking.Ranks;
                                    countFilteringRanks = rankingModule.OutputRanking.LatterRankingBuffer.Ranks;
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                            //outputRanks = maskFilteringRanks;
                            outputRanks = countFilteringRanks;


                            // find top video/shot positions in the result set
                            //int[] videoFrameSet = task.videoFrameSet.ToObject<int[]>();
                            //int[] shotFrameSet = task.shotFrameSet.ToObject<int[]>();
                            int[] videoFrameSet = datasetServiceManager.CurrentDataset.DatasetService.GetFrameIdsForVideo(task.videoId);
                            int[] shotFrameSet = videoFrameSet
                                .Where(frameId => datasetServiceManager.CurrentDataset.DatasetService.GetFrameNumberForFrameId(frameId) >= task.startFrame
                                    && datasetServiceManager.CurrentDataset.DatasetService.GetFrameNumberForFrameId(frameId) <= task.endFrame)
                                .ToArray();

                            int topVideoPosition = GetFramesetTopPosition(videoFrameSet, outputRanks);
                            int topShotPosition = GetFramesetTopPosition(shotFrameSet, outputRanks);


                            // count per display size

                            for (int displaySize = 8; displaySize <= 1024; displaySize *= 2)
                            {
                                int videoDisplayedCount = GetDisplayedCount(videoFrameSet, outputRanks, displaySize);
                                int shotDisplayedCount = GetDisplayedCount(shotFrameSet, outputRanks, displaySize);

                                // accumulate displayed counts
                                // accumulate across all queries
                                AccumulateDisplayedCounts(
                                    displayedVideoAccumulator, displayedShotAccumulator, displaySize, 
                                    videoDisplayedCount, shotDisplayedCount);
                            }


                            // find where the video/shot was filtered
                            //string filteredBy = "TODO";

                            // write result
                            //dynamic Result = new ExpandoObject();

                            //Result.taskName = (string)task.name;
                            //Result.taskStartTimeStamp = (string)task.startTimeStamp;
                            //Result.taskEndTimeStamp = (string)task.endTimeStamp;
                            //Result.videoId = (int)task.videoId;
                            //Result.startFrame = (int)task.startFrame;
                            //Result.endFrame = (int)task.endFrame;

                            //Result.queryTimestamp = queryTimestamp;
                            //Result.topVideoPosition = topVideoPosition;
                            //Result.topShotPosition = topShotPosition;
                            //Result.filteredBy = filteredBy;

                            //writer.WriteLine(JsonConvert.SerializeObject(Result));

                            //localTopRankedWriter.WriteLine($"{queryTimestamp};{topVideoPosition};{topShotPosition}");


                            //AccumulateAcrossUserQueries(localVideoAccumulator, localShotAccumulator, topVideoPosition, topShotPosition);

                            AccumulateAcrossAllQueries(videoRankAccumulator, shotRankAccumulator, topVideoPosition, topShotPosition);

                        }
                    }

                    //PrintResultAcrossUserQueries(localVideoAccumulator, localShotAccumulator, localTopRankedFilePath);
                }

                PrintResultAcrossAllQueries(dataset, videoRankAccumulator, shotRankAccumulator, topRankedFilePath);

                // print displayed counts
                for (int displaySize = 8; displaySize <= 1024; displaySize *= 2)
                {
                    displayedWriter.Write($"{displayedVideoAccumulator[displaySize]};");
                    displayString += $"{displayedVideoAccumulator[displaySize]};";
                }
                for (int displaySize = 8; displaySize <= 1024; displaySize *= 2)
                {
                    displayedWriter.Write($"{displayedShotAccumulator[displaySize]};");
                    displayString += $"{displayedShotAccumulator[displaySize]};";
                }
                displayedWriter.WriteLine();
            }
            return displayString;
        }

        private static void AccumulateDisplayedCounts(Dictionary<int, int> displayedVideoAccumulator, Dictionary<int, int> displayedShotAccumulator, int displaySize, int videoDisplayedCount, int shotDisplayedCount)
        {
            if (displayedVideoAccumulator.ContainsKey(displaySize))
            {
                displayedVideoAccumulator[displaySize] += videoDisplayedCount;
            }
            else
            {
                displayedVideoAccumulator[displaySize] = videoDisplayedCount;
            }
            if (displayedShotAccumulator.ContainsKey(displaySize))
            {
                displayedShotAccumulator[displaySize] += shotDisplayedCount;
            }
            else
            {
                displayedShotAccumulator[displaySize] = shotDisplayedCount;
            }
        }

        private static int GetDisplayedCount(int[] frameSet, float[] ranks, int displaySize)
        {
            //return outputRanks.Select((rank, index) => new { Rank = rank, Index = index })
            //    .OrderByDescending(x => x.Rank)
            //    .Take(displaySize)
            //    .Where(x => frameSet.Contains(x.Index))
            //    .Count();

            // prepare indexes
            int[] indexes = Enumerable.Range(0, ranks.Length).ToArray();
            // hide filtered indexes
            for (int i = 0; i < frameSet.Length; i++)
            {
                if (ranks[frameSet[i]] == float.MinValue)
                {
                    indexes[frameSet[i]] = -1;
                }
            }
            // sort indexes by rank descending
            float[] ranksCopy = (float[])ranks.Clone();
            Array.Sort(ranksCopy, indexes);
            Array.Reverse(indexes);

            // find number of items from the frameSet in the display
            int displayedCount = 0;
            for (int iPosition = 0; iPosition < displaySize; iPosition++)
            {
                if (frameSet.Contains(indexes[iPosition]))
                {
                    displayedCount++;
                }
            }
            return displayedCount;
        }

        private static void AccumulateAcrossUserQueries(Dictionary<int, int> localVideoAccumulator, Dictionary<int, int> localShotAccumulator, int topVideoPosition, int topShotPosition)
        {
            // accumulate across all queries
            if (localVideoAccumulator.ContainsKey(topVideoPosition))
            {
                localVideoAccumulator[topVideoPosition]++;
            }
            else
            {
                localVideoAccumulator[topVideoPosition] = 1;
            }

            if (localShotAccumulator.ContainsKey(topShotPosition))
            {
                localShotAccumulator[topShotPosition]++;
            }
            else
            {
                localShotAccumulator[topShotPosition] = 1;
            }
        }

        private static void AccumulateAcrossAllQueries(Dictionary<int, int> videoAccumulator, Dictionary<int, int> shotAccumulator, int topVideoPosition, int topShotPosition)
        {
            // accumulate across all queries
            if (videoAccumulator.ContainsKey(topVideoPosition))
            {
                videoAccumulator[topVideoPosition]++;
            }
            else
            {
                videoAccumulator[topVideoPosition] = 1;
            }
            if (shotAccumulator.ContainsKey(topShotPosition))
            {
                shotAccumulator[topShotPosition]++;
            }
            else
            {
                shotAccumulator[topShotPosition] = 1;
            }
        }

        private static void PrintResultAcrossUserQueries(
            Dictionary<int, int> localVideoAccumulator, Dictionary<int, int> localShotAccumulator, 
            string localTopRankedFilePath)
        {
            // cumulative graph across user queries
            localVideoAccumulator.Remove(-1);
            localShotAccumulator.Remove(-1);

            int localVideoRankSum = 0;
            using (StreamWriter writer = new StreamWriter(localTopRankedFilePath + ".v.txt"))
            {
                foreach (KeyValuePair<int, int> keyValue in localVideoAccumulator.OrderBy(x => x.Key))
                {
                    localVideoRankSum += keyValue.Value;
                    writer.WriteLine($"{keyValue.Key};{localVideoRankSum}");
                }
            }

            int localShotRankSum = 0;
            using (StreamWriter writer = new StreamWriter(localTopRankedFilePath + ".s.txt"))
            {
                foreach (KeyValuePair<int, int> keyValue in localShotAccumulator.OrderBy(x => x.Key))
                {
                    localShotRankSum += keyValue.Value;
                    writer.WriteLine($"{keyValue.Key};{localShotRankSum}");
                }
            }
        }

        private static void PrintResultAcrossAllQueries(
            DatasetServices dataset, Dictionary<int, int> videoAccumulator, Dictionary<int, int> shotAccumulator, 
            string topRankedFilePath)
        {
            // cumulative graph across all queries
            videoAccumulator.Remove(-1);
            shotAccumulator.Remove(-1);

            //videoAccumulator.Add(videoAccumulator.Keys.Max() + 1, dataset.DatasetService.Dataset.Frames.Count);
            //shotAccumulator.Add(shotAccumulator.Keys.Max() + 1, dataset.DatasetService.Dataset.Frames.Count);

            int videoRankSum = 0;
            using (StreamWriter writer = new StreamWriter(topRankedFilePath + ".v.txt"))
            {
                foreach (KeyValuePair<int, int> keyValue in videoAccumulator.OrderBy(x => x.Key))
                {
                    videoRankSum += keyValue.Value;
                    writer.WriteLine($"{keyValue.Key};{videoRankSum}");
                }
            }

            int shotRankSum = 0;
            using (StreamWriter writer = new StreamWriter(topRankedFilePath + ".s.txt"))
            {
                foreach (KeyValuePair<int, int> keyValue in shotAccumulator.OrderBy(x => x.Key))
                {
                    shotRankSum += keyValue.Value;
                    writer.WriteLine($"{keyValue.Key};{shotRankSum}");
                }
            }
        }

        private static int[] ParseSteps(string inputString)
        {
            return inputString.Split(',').Select(x => int.Parse(x)).ToArray();
        }

        private static int GetFramesetTopPosition(int[] frameSet, float[] ranks)
        {
            // prepare indexes
            int[] indexes = Enumerable.Range(0, ranks.Length).ToArray();
            // hide filtered indexes
            for (int i = 0; i < frameSet.Length; i++)
            {
                if (ranks[frameSet[i]] == float.MinValue)
                {
                    indexes[frameSet[i]] = -1;
                }
            }
            // sort indexes by rank descending
            float[] ranksCopy = (float[])ranks.Clone();
            Array.Sort(ranksCopy, indexes);
            Array.Reverse(indexes);

            // find the first position containing item from the frameSet
            for (int iPosition = 0; iPosition < indexes.Length; iPosition++)
            {
                if (frameSet.Contains(indexes[iPosition]))
                {
                    return iPosition;
                }
            }
            return -1;
        }


        private static void SetNoFilter(BiTemporalQuery query)
        {
            query.FormerFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Disabled;
            query.LatterFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Disabled;

            query.FormerFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.FormerFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.FormerFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.FormerFusionQuery.TextSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.FormerFusionQuery.FaceSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;

            query.LatterFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.LatterFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.LatterFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.LatterFusionQuery.TextSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
            query.LatterFusionQuery.FaceSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
        }

        private static void SetFiltersMiddle(BiTemporalQuery query)
        {
            query.FormerFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Enabled;
            query.FormerFilteringQuery.CountFilteringQuery.MaxPerVideo = 10;
            query.FormerFilteringQuery.CountFilteringQuery.MaxPerShot = 3;
            query.LatterFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Enabled;
            query.LatterFilteringQuery.CountFilteringQuery.MaxPerVideo = 10;
            query.LatterFilteringQuery.CountFilteringQuery.MaxPerShot = 3;

            query.FormerFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.FormerFusionQuery.KeywordFilteringQuery.Threshold = 0.3;
            query.FormerFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.FormerFusionQuery.ColorSketchFilteringQuery.Threshold = 0.3;
            query.FormerFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.FormerFusionQuery.SemanticExampleFilteringQuery.Threshold = 0.3;

            query.LatterFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.LatterFusionQuery.KeywordFilteringQuery.Threshold = 0.3;
            query.LatterFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.LatterFusionQuery.ColorSketchFilteringQuery.Threshold = 0.3;
            query.LatterFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.LatterFusionQuery.SemanticExampleFilteringQuery.Threshold = 0.3;
        }

        private static void SetFilters(BiTemporalQuery query, int maxPerVideo, int maxPerShot, 
            float keywordThreshold, float colorThreshold, float semanticThreshold)
        {
            query.FormerFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Enabled;
            query.FormerFilteringQuery.CountFilteringQuery.MaxPerVideo = maxPerVideo;
            query.FormerFilteringQuery.CountFilteringQuery.MaxPerShot = maxPerShot;
            query.LatterFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Enabled;
            query.LatterFilteringQuery.CountFilteringQuery.MaxPerVideo = maxPerVideo;
            query.LatterFilteringQuery.CountFilteringQuery.MaxPerShot = maxPerShot;

            query.FormerFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.FormerFusionQuery.KeywordFilteringQuery.Threshold = keywordThreshold;
            query.FormerFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.FormerFusionQuery.ColorSketchFilteringQuery.Threshold = colorThreshold;
            query.FormerFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.FormerFusionQuery.SemanticExampleFilteringQuery.Threshold = semanticThreshold;

            query.LatterFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.LatterFusionQuery.KeywordFilteringQuery.Threshold = keywordThreshold;
            query.LatterFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.LatterFusionQuery.ColorSketchFilteringQuery.Threshold = colorThreshold;
            query.LatterFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.IncludeAboveThreshold;
            query.LatterFusionQuery.SemanticExampleFilteringQuery.Threshold = semanticThreshold;
        }
    }
}
