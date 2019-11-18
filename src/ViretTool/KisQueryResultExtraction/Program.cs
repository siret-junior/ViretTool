using System;
using System.Collections.Generic;
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
            // TODO: print usage, parameter check
            string datasetDirectory = args[0];
            string queryDirectory = args[1];
            //string queryImageDirectory = args[];
            string tasksFilePath = args[2];
            string outputFilePath = args[3];
            Directory.CreateDirectory(Directory.GetParent(outputFilePath).FullName);

            // load tasks
            //dynamic[] tasks = File.ReadAllLines(tasksFilePath)
            //    .Select(jsonLine => JsonConvert.DeserializeObject(jsonLine))
            //    .ToArray();
            var tasks = JsonConvert.DeserializeObject<string[]>(File.ReadAllLines(tasksFilePath)[0])
                .Select(line => line.Split(';'))
                .Select(item => new
                {
                    videoId = int.Parse(item[3]),
                    startFrame = int.Parse(item[4]),
                    endFrame = int.Parse(item[5]),
                    startTimeStamp = long.Parse(item[6] + "000"),
                    endTimeStamp = long.Parse(item[7] + "000")
                });

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

                // accumulators
                Dictionary<int, int> videoAccumulator = new Dictionary<int, int>();
                Dictionary<int, int> shotAccumulator = new Dictionary<int, int>();

                // compute result set for each query in query directory
                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    string[] queryFiles = Directory.GetFiles(queryDirectory); // TODO: search pattern, check...
                    foreach (string queryFile in queryFiles)
                    {
                        Console.WriteLine($"Computing ranking for {Path.GetFileName(queryFile)}");
                        string queryTimestampString = Path.GetFileNameWithoutExtension(queryFile)
                            .Split('_')[1];
                        long queryTimestamp = long.Parse(queryTimestampString);
                        
                        // find correct task based on timestamp
                        dynamic task = tasks
                            .Where(t => queryTimestamp >= (long)t.startTimeStamp && queryTimestamp <= (long)t.endTimeStamp)
                            .FirstOrDefault();
                        if (task == null)
                        {
                            // TODO
                            //Console.WriteLine("SKIPPED");
                            continue;
                        }

                        // compute ranking
                        BiTemporalQuery query = queryPersistingService.LoadQuery(queryFile);

                        //**** no filter ****/
                        //query.FormerFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Disabled;
                        //query.LatterFilteringQuery.CountFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.CountFilteringQuery.State.Disabled;
                        //query.FormerFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        //query.FormerFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        //query.FormerFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        query.FormerFusionQuery.TextSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        query.FormerFusionQuery.FaceSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;

                        //query.LatterFusionQuery.KeywordFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        //query.LatterFusionQuery.ColorSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        //query.LatterFusionQuery.SemanticExampleFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        query.LatterFusionQuery.TextSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;
                        query.LatterFusionQuery.FaceSketchFilteringQuery.FilterState = ViretTool.BusinessLayer.RankingModels.Queries.ThresholdFilteringQuery.State.Off;

                        //**** filters middle ****/
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


                        BiTemporalRankedResultSet resultSet = rankingService.ComputeRankedResultSet(query);

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

                        writer.WriteLine($"{queryTimestamp};{topVideoPosition};{topShotPosition}");

                        
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
                }

                // cumulative graph
                //videoAccumulator.Remove(-1);
                //shotAccumulator.Remove(-1);

                int videoRankSum = 0;
                int shotRankSum = 0;

                using (StreamWriter writer = new StreamWriter(outputFilePath + ".v.txt"))
                {
                    foreach (KeyValuePair<int, int> keyValue in videoAccumulator.OrderBy(x => x.Key))
                    {
                        videoRankSum += keyValue.Value;
                        writer.WriteLine($"{keyValue.Key};{videoRankSum}");
                    }
                }

                using (StreamWriter writer = new StreamWriter(outputFilePath + ".s.txt"))
                {
                    foreach (KeyValuePair<int, int> keyValue in shotAccumulator.OrderBy(x => x.Key))
                    {
                        shotRankSum += keyValue.Value;
                        writer.WriteLine($"{keyValue.Key};{shotRankSum}");
                    }
                }
            }
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
    }
}
