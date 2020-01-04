using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.Submission;

namespace ViretTool.BusinessLayer.ResultLogging
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultSetAvailability
    {
        All, Top, Sample
    }

    public class ResultLog
    {
        public string TeamName { get; set; } = "VIRET";
        public int TeamId { get; set; }
        public int MemberId { get; set; }
        public long TimeStamp { get; set; }

        
        public LogCategory[] UsedCategories { get; set; }
        
        public LogType[] UsedTypes { get; set; }
        
        public LogType SortType { get; set; }
        
        public ResultSetAvailability ResultSetAvailability { get; set;}
        
        public SubmissionType Type { get; set; }
        public Result[] Results { get; set; }


        private readonly object _lockObject = new object();

        public ResultLog()
        {
            TeamId = int.Parse(ConfigurationManager.AppSettings["teamId"] ?? "7");
            MemberId = int.Parse(ConfigurationManager.AppSettings["memberId"] ?? "-1");
        }

        public ResultLog(BiTemporalQuery query, Result[] results) : this()
        {
            UsedCategories = GetUsedCategories(query);
            UsedTypes = GetUsedTypes(query);
            SortType = GetSortType(query);
            ResultSetAvailability = ResultSetAvailability.All;
            Type = SubmissionType.Result;
            Results = results;
        }

        private static LogCategory[] GetUsedCategories(BiTemporalQuery query)
        {
            List<LogCategory> usedLogCategories = new List<LogCategory>();

            // keyword
            if (query.BiTemporalSimilarityQuery.KeywordQuery.FormerQuery.Query.Any()
                || query.BiTemporalSimilarityQuery.KeywordQuery.LatterQuery.Query.Any())
            {
                usedLogCategories.Add(LogCategory.Text);
            }
            // color
            if (query.BiTemporalSimilarityQuery.ColorSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.ColorSketchQuery.LatterQuery.ColorSketchEllipses.Any())
            {
                usedLogCategories.Add(LogCategory.Sketch);
            }
            // semantic example
            if (query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.PositiveExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.PositiveExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.NegativeExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.NegativeExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.ExternalImages.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.ExternalImages.Any()
                )
            {
                usedLogCategories.Add(LogCategory.Image);
            }
            // face/text
            if (query.BiTemporalSimilarityQuery.FaceSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.FaceSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                )
            {
                usedLogCategories.Add(LogCategory.Sketch);
            }

            // color saturarion / percent of black
            if (query.FormerFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.FormerFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off)
            {
                usedLogCategories.Add(LogCategory.Filter);
            }

            // max frames
            if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled
                || query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled)
            {
                usedLogCategories.Add(LogCategory.Filter);
            }
            else if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled
                || query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled)
            {
                // should be always on by default
                throw new NotImplementedException("Unexpected behavior.");
            }

            // TODO: lifelog is not send because the result log is used only in VBS

            return usedLogCategories.ToArray();
        }

        private static LogType[] GetUsedTypes(BiTemporalQuery query)
        {
            List<LogType> usedLogTypes = new List<LogType>();

            // keyword
            if (query.BiTemporalSimilarityQuery.KeywordQuery.FormerQuery.Query.Any()
                || query.BiTemporalSimilarityQuery.KeywordQuery.LatterQuery.Query.Any())
            {
                usedLogTypes.Add(LogType.JointEmbedding);
            }
            // color
            if (query.BiTemporalSimilarityQuery.ColorSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.ColorSketchQuery.LatterQuery.ColorSketchEllipses.Any())
            {
                usedLogTypes.Add(LogType.Color);
            }
            // semantic example
            if (query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.PositiveExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.PositiveExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.NegativeExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.NegativeExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.ExternalImages.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.ExternalImages.Any()
                )
            {
                usedLogTypes.Add(LogType.GlobalFeatures);
            }
            // face/text
            if (query.BiTemporalSimilarityQuery.FaceSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.FaceSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                )
            {
                usedLogTypes.Add(LogType.LocalizedObject);
            }

            // color saturarion / percent of black
            if (query.FormerFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.FormerFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off)
            {
                usedLogTypes.Add(LogType.BW);
            }

            // max frames
            if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled
                || query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled)
            {
                usedLogTypes.Add(LogType.MaxFrames);
            }
            else if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled
                || query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled)
            {
                // should be always on by default
                throw new NotImplementedException("Unexpected behavior.");
            }

            // TODO: lifelog is not send because the result log is used only in VBS

            return usedLogTypes.ToArray();
        }

        private static LogType GetSortType(BiTemporalQuery query)
        {
            FusionQuery primaryFusionQuery;
            switch (query.PrimaryTemporalQuery)
            {
                case BiTemporalQuery.TemporalQueries.Former:
                    primaryFusionQuery = query.FormerFusionQuery;
                    break;
                case BiTemporalQuery.TemporalQueries.Latter:
                    primaryFusionQuery = query.LatterFusionQuery;
                    break;
                default:
                    throw new NotImplementedException("Unknown primary temporal query.");
            }

            switch (primaryFusionQuery.SortingSimilarityModel)
            {
                case FusionQuery.SimilarityModels.ColorSketch:
                    return LogType.Color;
                case FusionQuery.SimilarityModels.Keyword:
                    return LogType.JointEmbedding;
                case FusionQuery.SimilarityModels.SemanticExample:
                    return LogType.GlobalFeatures;
                case FusionQuery.SimilarityModels.FaceSketch:
                case FusionQuery.SimilarityModels.TextSketch:
                    return LogType.LocalizedObject;
                case FusionQuery.SimilarityModels.None:
                    return LogType.None;
                default:
                    throw new NotImplementedException("Unknown sorting similarity model.");
            }
        }


        internal string GetContent(long unixTimestamp)
        {
            lock (_lockObject)
            {
                TimeStamp = unixTimestamp;
                return LowercaseJsonSerializer.SerializeObject(this);
            }
        }

        internal string GetContentIndented(long unixTimestamp)
        {
            lock (_lockObject)
            {
                TimeStamp = unixTimestamp;
                return LowercaseJsonSerializer.SerializeObjectIndented(this);
            }
        }
    }
}
