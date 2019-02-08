using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.Submission;

namespace KisQueryResultUpdater
{
    class Program
    {
        public static void Main(string[] args)
        {
            //updates queries if create and last accessed time differs and set different sorting model if necessary

            string queryDirectory = args[0];
            string outputDirectory = args[1];
            
            string previousQueryFile = null;
            string[] queryFiles = Directory.GetFiles(queryDirectory); // TODO: search pattern, check...
            foreach (string queryFile in queryFiles)
            {
                long queryTimestamp = long.Parse(Path.GetFileNameWithoutExtension(queryFile));
                if (!GetIfQueryNeedsToBeUpdated(queryFile, previousQueryFile))
                {
                    //Console.WriteLine($"{queryFile} update not needed");
                    File.Copy(queryFile, Path.Combine(outputDirectory, $"{queryTimestamp}.json"), true);
                    previousQueryFile = queryFile;
                    continue;
                }

                double createWriteTimeDif = GetCreateWriteTimeDiff(queryFile);
                BiTemporalQuery query = DeserializeQueryObject(queryFile);
                List<FusionQuery.SimilarityModels> changedModels = GetChangedModels(query, DeserializeQueryObject(previousQueryFile)).ToList();
                if (changedModels.Count > 1)
                {
                    Console.WriteLine($"{queryFile} undecided log");
                }

                FusionQuery.SimilarityModels newSortedBy = changedModels.First();
                

                //original query is shifted
                SaveQueryObject(Path.Combine(outputDirectory, $"{queryTimestamp + Math.Round(createWriteTimeDif * 1000)}.json"), query);

                //query timestamp is saved with different sorted by model
                //not nice...
                FieldInfo field = typeof(FusionQuery).GetField("<SortingSimilarityModel>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                field.SetValue((query.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former ? query.FormerFusionQuery : query.LatterFusionQuery), newSortedBy);

                SaveQueryObject(Path.Combine(outputDirectory, $"{queryTimestamp}.json"), query);

                previousQueryFile = queryFile;
            }
        }

        private static bool GetIfQueryNeedsToBeUpdated(string queryFile, string previousQueryFile)
        {
            double createWriteTimeDif = GetCreateWriteTimeDiff(queryFile);
            if (createWriteTimeDif < 0.25 || string.IsNullOrEmpty(previousQueryFile))
            {
                return false;
            }

            BiTemporalQuery query = DeserializeQueryObject(queryFile);
            FusionQuery.SimilarityModels sortedBy = (query.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former ? query.FormerFusionQuery : query.LatterFusionQuery).SortingSimilarityModel;
            if (sortedBy == FusionQuery.SimilarityModels.None)
            {
                return false;
            }

            BiTemporalQuery previousQuery = DeserializeQueryObject(previousQueryFile);
            List<FusionQuery.SimilarityModels> changedModels = GetChangedModels(query, previousQuery).ToList();

            return changedModels.Any() && !changedModels.Contains(sortedBy);
        }

        private static BiTemporalQuery DeserializeQueryObject(string queryFile)
        {
            return LowercaseJsonSerializer.DeserializeObject<BiTemporalQuery>(File.ReadAllText(queryFile));
        }

        private static void SaveQueryObject(string fileName, BiTemporalQuery query)
        {
            string jsonQuery = LowercaseJsonSerializer.SerializeObject(query);
            File.WriteAllText(fileName, jsonQuery);
        }

        private static double GetCreateWriteTimeDiff(string queryFile)
        {
            FileInfo fileInfo = new FileInfo(queryFile);
            return (fileInfo.LastWriteTimeUtc - fileInfo.CreationTimeUtc).TotalSeconds;
        }

        private static IEnumerable<FusionQuery.SimilarityModels> GetChangedModels(BiTemporalQuery query, BiTemporalQuery previousQuery)
        {
            bool isQueryFormer = query.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former;
            bool isPrevQueryFormer = previousQuery.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former;

            BiTemporalSimilarityQuery qSim = query.BiTemporalSimilarityQuery;
            BiTemporalSimilarityQuery pqSim = previousQuery.BiTemporalSimilarityQuery;

            if (!GetQueryModel(qSim.ColorSketchQuery, isQueryFormer).Equals(GetQueryModel(pqSim.ColorSketchQuery, isPrevQueryFormer)) ||
                !GetQueryModel(qSim.FaceSketchQuery, isQueryFormer).Equals(GetQueryModel(pqSim.FaceSketchQuery, isPrevQueryFormer)) ||
                !GetQueryModel(qSim.TextSketchQuery, isQueryFormer).Equals(GetQueryModel(pqSim.TextSketchQuery, isPrevQueryFormer)))
            {
                yield return FusionQuery.SimilarityModels.ColorSketch;
            }

            if (!GetQueryModel(qSim.KeywordQuery, isQueryFormer).Equals(GetQueryModel(pqSim.KeywordQuery, isPrevQueryFormer)))
            {
                yield return FusionQuery.SimilarityModels.Keyword;
            }

            if (!GetQueryModel(qSim.SemanticExampleQuery, isQueryFormer).Equals(GetQueryModel(pqSim.SemanticExampleQuery, isPrevQueryFormer)))
            {
                yield return FusionQuery.SimilarityModels.SemanticExample;
            }
        }

        private static TQuery GetQueryModel<TQuery>(BiTemporalModelQuery<TQuery> queryModel, bool takeFormer) where TQuery : IQuery
        {
            return takeFormer ? queryModel.FormerQuery : queryModel.LatterQuery;
        }
    }
}
