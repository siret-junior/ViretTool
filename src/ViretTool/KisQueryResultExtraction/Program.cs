using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Proxy;
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
            dynamic[] tasks = File.ReadAllLines(tasksFilePath)
                .Select(jsonLine => JsonConvert.DeserializeObject(jsonLine))
                .ToArray();

            using (WindsorContainer container = new WindsorContainer(
                    new DefaultKernel(
                        new ArgumentPassingDependencyResolver(), 
                        new NotSupportedProxyFactory()), 
                    new DefaultComponentInstaller()))
            {
                // init business layer
                Console.WriteLine("Initializing ranking service...");
                container.AddFacility<TypedFactoryFacility>();
                container.Install(FromAssembly.This());
                IDatasetServicesManager datasetServiceManager = container.Resolve<IDatasetServicesManager>();
                datasetServiceManager.OpenDataset(datasetDirectory);

                // init services
                DatasetServices dataset = datasetServiceManager.CurrentDataset;
                IQueryPersistingService queryPersistingService = dataset.QueryPersistingService;
                IBiTemporalRankingService rankingService = dataset.RankingService;

                // compute result set for each query in query directory
                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    string[] queryFiles = Directory.GetFiles(queryDirectory); // TODO: search pattern, check...
                    foreach (string queryFile in queryFiles)
                    {
                        Console.WriteLine($"Computing ranking for {Path.GetFileName(queryFile)}");
                        long queryTimestamp = long.Parse(Path.GetFileNameWithoutExtension(queryFile));
                        
                        // find correct task based on timestamp
                        dynamic task = tasks
                            .Where(t => queryTimestamp >= (long)t.startTimeStamp && queryTimestamp <= (long)t.endTimeStamp)
                            .FirstOrDefault();
                        if (task == null)
                        {
                            // TODO
                            Console.WriteLine("SKIPPED");
                            continue;
                        }

                        // compute ranking
                        BiTemporalQuery query = queryPersistingService.LoadQuery(queryFile);
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
                        int[] videoFrameSet = task.videoFrameSet.ToObject<int[]>();
                        int[] shotFrameSet = task.shotFrameSet.ToObject<int[]>();

                        int topVideoPosition = GetFramesetTopPosition(videoFrameSet, outputRanks);
                        int topShotPosition = GetFramesetTopPosition(shotFrameSet, outputRanks);

                        // find where the video/shot was filtered
                        string filteredBy = "TODO";

                        // write result
                        dynamic Result = new ExpandoObject();

                        Result.taskName = (string)task.name;
                        Result.taskStartTimeStamp = (string)task.startTimeStamp;
                        Result.taskEndTimeStamp = (string)task.endTimeStamp;
                        Result.videoId = (int)task.videoId;
                        Result.startFrame = (int)task.startFrame;
                        Result.endFrame = (int)task.endFrame;

                        Result.queryTimestamp = queryTimestamp;
                        Result.topVideoPosition = topVideoPosition;
                        Result.topShotPosition = topShotPosition;
                        Result.filteredBy = filteredBy;
                        
                        writer.WriteLine(JsonConvert.SerializeObject(Result));
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
