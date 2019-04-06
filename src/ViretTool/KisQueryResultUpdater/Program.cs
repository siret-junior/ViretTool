using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

                double createWriteTimeDif = GetCreateWriteTimeDiff(queryFile);
                if (createWriteTimeDif < 0.25)
                {
                    File.Copy(queryFile, Path.Combine(outputDirectory, $"{queryTimestamp}.json"), true);
                    previousQueryFile = queryFile;
                    continue;
                }

                //original query is shifted
                File.Copy(queryFile, Path.Combine(outputDirectory, $"{queryTimestamp + Math.Round(createWriteTimeDif * 1000)}.json"), true);
                if (string.IsNullOrEmpty(previousQueryFile))
                {
                    previousQueryFile = queryFile;
                    continue;
                }

                BiTemporalQuery query = DeserializeQueryObject(queryFile);
                FusionQuery.SimilarityModels sortedBy = (query.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former ? query.FormerFusionQuery : query.LatterFusionQuery).SortingSimilarityModel;

                BiTemporalQuery previousQuery = DeserializeQueryObject(previousQueryFile);
                List<FusionQuery.SimilarityModels> changedModels = GetChangedModels(query, previousQuery).ToList();

                bool sortedByChanged = changedModels.Any() && !changedModels.Contains(sortedBy) && !IsQueryEmpty(query);
                if (!sortedByChanged)
                {
                    previousQueryFile = queryFile;
                    continue;
                }

                if (changedModels.Count > 1)
                {
                    Console.WriteLine($"{queryFile} undecided log");
                }

                FusionQuery.SimilarityModels newSortedBy = changedModels.First();

                //query timestamp is saved with different sorted by model
                //not nice...
                FieldInfo field = typeof(FusionQuery).GetField("<SortingSimilarityModel>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                field.SetValue((query.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former ? query.FormerFusionQuery : query.LatterFusionQuery), newSortedBy);

                SaveQueryObject(Path.Combine(outputDirectory, $"{queryTimestamp}.json"), query);

                previousQueryFile = queryFile;
            }
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
            BiTemporalSimilarityQuery qSim = query.BiTemporalSimilarityQuery;
            BiTemporalSimilarityQuery pqSim = previousQuery.BiTemporalSimilarityQuery;

            if (!qSim.ColorSketchQuery.Equals(pqSim.ColorSketchQuery) ||
                !qSim.FaceSketchQuery.Equals(pqSim.FaceSketchQuery) ||
                !qSim.TextSketchQuery.Equals(pqSim.TextSketchQuery))
            {
                yield return FusionQuery.SimilarityModels.ColorSketch;
            }

            if (!qSim.KeywordQuery.Equals(pqSim.KeywordQuery))
            {
                yield return FusionQuery.SimilarityModels.Keyword;
            }

            if (!qSim.SemanticExampleQuery.Equals(pqSim.SemanticExampleQuery))
            {
                yield return FusionQuery.SimilarityModels.SemanticExample;
            }
        }

        private static bool IsQueryEmpty(BiTemporalQuery query)
        {
            bool isQueryFormer = query.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former;
            BiTemporalSimilarityQuery qSim = query.BiTemporalSimilarityQuery;
            SemanticExampleQuery semanticExampleQuery = GetQueryModel(qSim.SemanticExampleQuery, isQueryFormer);

            return !GetQueryModel(qSim.ColorSketchQuery, isQueryFormer).ColorSketchEllipses.Any() &&
                   !GetQueryModel(qSim.FaceSketchQuery, isQueryFormer).ColorSketchEllipses.Any() &&
                   !GetQueryModel(qSim.TextSketchQuery, isQueryFormer).ColorSketchEllipses.Any() &&
                   !GetQueryModel(qSim.KeywordQuery, isQueryFormer).SynsetGroups.Any() &&
                   !semanticExampleQuery.PositiveExampleIds.Any() &&
                   !semanticExampleQuery.ExternalImages.Any();
        }

        private static TQuery GetQueryModel<TQuery>(BiTemporalModelQuery<TQuery> queryModel, bool takeFormer) where TQuery : IQuery
        {
            return takeFormer ? queryModel.FormerQuery : queryModel.LatterQuery;
        }
    }
}
