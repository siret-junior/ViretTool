using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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

    public abstract class ResultLogBase : IResultLog
    {
        //public string Teamname { get; set; } = "VIRET";
        public int TeamId { get; set; }
        public int MemberId { get; set; }
        public long Timestamp { get; set; }

        
        public LogCategory[] UsedCategories { get; set; }
        
        public LogType[] UsedTypes { get; set; }

        public string[] Values { get; set; }

        public LogType[] SortType { get; set; }
        
        public ResultSetAvailability ResultSetAvailability { get; set;}
        
        public SubmissionType Type { get; set; }


        private readonly object _lockObject = new object();


        public ResultLogBase()
        {
            TeamId = int.Parse(ConfigurationManager.AppSettings["teamId"] ?? "7");
            MemberId = int.Parse(ConfigurationManager.AppSettings["memberId"] ?? "-1");
        }

        public ResultLogBase(BiTemporalQuery query) : this()
        {
            UsedCategories = GetUsedCategories(query);
            UsedTypes = GetUsedTypes(query);
            Values = GetValues(query);
            SortType = new LogType[] { GetSortType(query) };
            ResultSetAvailability = ResultSetAvailability.All;
            Type = SubmissionType.Result;
        }

        public string GetJson(long unixTimestamp)
        {
            lock (_lockObject)
            {
                Timestamp = unixTimestamp;
                return CamelcaseJsonSerializer.SerializeObject(this);
            }
        }

        public string GetJsonIndented(long unixTimestamp)
        {
            lock (_lockObject)
            {
                Timestamp = unixTimestamp;
                return CamelcaseJsonSerializer.SerializeObjectIndented(this);
            }
        }


        private static LogCategory[] GetUsedCategories(BiTemporalQuery query)
        {
            List<LogCategory> interactionCategories = new List<LogCategory>();

            // keyword
            if (query.BiTemporalSimilarityQuery.KeywordQuery.FormerQuery.Query.Any()
                || query.BiTemporalSimilarityQuery.KeywordQuery.LatterQuery.Query.Any()
                // face/text
                || query.BiTemporalSimilarityQuery.FaceSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.FaceSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                // color saturarion / percent of black
                || query.FormerFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.FormerFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off
                )
            {
                interactionCategories.Add(LogCategory.Text);
            }

            // color
            if (query.BiTemporalSimilarityQuery.ColorSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.ColorSketchQuery.LatterQuery.ColorSketchEllipses.Any())
            {
                interactionCategories.Add(LogCategory.Sketch);
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
                interactionCategories.Add(LogCategory.Image);
            }

            // max frames
            if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled
                || query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled
                // lifelog
                || !query.FormerFilteringQuery.LifelogFilteringQuery.IsEmpty()
                || !query.LatterFilteringQuery.LifelogFilteringQuery.IsEmpty()
                )
            {
                interactionCategories.Add(LogCategory.Filter);
            }
            else if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled
                || query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled)
            {
                // should be always on by default
                //throw new NotImplementedException("Unexpected behavior.");
                // TODO: should not throw exception during log submission
            }

            return interactionCategories.ToArray();
        }

        private static LogType[] GetUsedTypes(BiTemporalQuery query)
        {
            List<LogType> interactionTypes = new List<LogType>();

            // keyword
            if (query.BiTemporalSimilarityQuery.KeywordQuery.FormerQuery.Query.Any()
                || query.BiTemporalSimilarityQuery.KeywordQuery.LatterQuery.Query.Any())
            {
                interactionTypes.Add(LogType.JointEmbedding);
            }
            // color
            if (query.BiTemporalSimilarityQuery.ColorSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.ColorSketchQuery.LatterQuery.ColorSketchEllipses.Any())
            {
                interactionTypes.Add(LogType.Color);
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
                interactionTypes.Add(LogType.GlobalFeatures);
            }
            // face/text
            if (query.BiTemporalSimilarityQuery.FaceSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.FaceSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                )
            {
                interactionTypes.Add(LogType.LocalizedObject);
            }

            // color saturarion / percent of black
            if (query.FormerFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.FormerFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off)
            {
                interactionTypes.Add(LogType.BW);
            }

            // max frames
            if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled
                || query.LatterFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled)
            {
                interactionTypes.Add(LogType.MaxFrames);
            }
            else if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled
                || query.LatterFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled)
            {
                // should be always on by default
                //throw new NotImplementedException("Unexpected behavior.");
                // TODO: should not throw exception during log submission
            }

            // lifelog
            if (!query.FormerFilteringQuery.LifelogFilteringQuery.IsEmpty()
                || !query.LatterFilteringQuery.LifelogFilteringQuery.IsEmpty())
            {
                interactionTypes.Add(LogType.Lifelog);
            }

            return interactionTypes.ToArray();
        }

        private static string[] GetValues(BiTemporalQuery query)
        {
            List<string> interactionValues = new List<string>();

            if (query.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former)
            {
                interactionValues.Add("Primary_1");
            }
            else
            {
                interactionValues.Add("Primary_2");
            }

            // keyword
            if (query.BiTemporalSimilarityQuery.KeywordQuery.FormerQuery.Query.Any())
            {
                string[] values = query.BiTemporalSimilarityQuery.KeywordQuery.FormerQuery.Query;
                interactionValues.Add($"JointEmbedding_1({values.Length}: {string.Join(" ", values)}"
                    + $"|{(int)(query.FormerFusionQuery.KeywordFilteringQuery.Threshold * 100)}%)");
            }
            if (query.BiTemporalSimilarityQuery.KeywordQuery.LatterQuery.Query.Any())
            {
                string[] values = query.BiTemporalSimilarityQuery.KeywordQuery.LatterQuery.Query;
                interactionValues.Add($"JointEmbedding_2({values.Length}: {string.Join(" ", values)}"
                    + $"|{(int)(query.LatterFusionQuery.KeywordFilteringQuery.Threshold * 100)}%)");
            }

            // color
            if (query.BiTemporalSimilarityQuery.ColorSketchQuery.FormerQuery.ColorSketchEllipses.Any())
            {
                interactionValues.Add(
                    $"Color_1({query.BiTemporalSimilarityQuery.ColorSketchQuery.FormerQuery.ColorSketchEllipses.Count()}"
                    + $"|{(int)(query.FormerFusionQuery.ColorSketchFilteringQuery.Threshold * 100)}%)");
            }
            if (query.BiTemporalSimilarityQuery.ColorSketchQuery.LatterQuery.ColorSketchEllipses.Any())
            {
                interactionValues.Add(
                    $"Color_2({query.BiTemporalSimilarityQuery.ColorSketchQuery.LatterQuery.ColorSketchEllipses.Count()}"
                    + $"|{(int)(query.LatterFusionQuery.ColorSketchFilteringQuery.Threshold * 100)}%)");
            }

            // semantic example
            // TODO: simplify
            if (query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.PositiveExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.NegativeExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.ExternalImages.Any()
                )
            {
                interactionValues.Add($"GlobalFeatures_1("
                    + $"+{query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.PositiveExampleIds.Count()}"
                    + $"|-{query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.NegativeExampleIds.Count()}"
                    + $"|E{query.BiTemporalSimilarityQuery.SemanticExampleQuery.FormerQuery.ExternalImages.Count()}"
                    + $"|{(int)(query.FormerFusionQuery.SemanticExampleFilteringQuery.Threshold * 100)}%)");
            }
            if (query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.PositiveExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.NegativeExampleIds.Any()
                || query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.ExternalImages.Any()
                )
            {
                interactionValues.Add($"GlobalFeatures_2("
                    + $"+{query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.PositiveExampleIds.Count()}"
                    + $"|-{query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.NegativeExampleIds.Count()}"
                    + $"|E{query.BiTemporalSimilarityQuery.SemanticExampleQuery.LatterQuery.ExternalImages.Count()}"
                    + $"|{(int)(query.FormerFusionQuery.SemanticExampleFilteringQuery.Threshold * 100)}%)");
            }

            // face/text
            if (query.BiTemporalSimilarityQuery.FaceSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.FormerQuery.ColorSketchEllipses.Any()
                )
            {
                interactionValues.Add($"LocalizedObject_1("
                    + $"F{query.BiTemporalSimilarityQuery.FaceSketchQuery.FormerQuery.ColorSketchEllipses.Count()}"
                    + $"|T{query.BiTemporalSimilarityQuery.TextSketchQuery.FormerQuery.ColorSketchEllipses.Count()}"
                    + $")");
            }
            if (query.BiTemporalSimilarityQuery.FaceSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                || query.BiTemporalSimilarityQuery.TextSketchQuery.LatterQuery.ColorSketchEllipses.Any()
                )
            {
                interactionValues.Add($"LocalizedObject_2("
                    + $"F{query.BiTemporalSimilarityQuery.FaceSketchQuery.LatterQuery.ColorSketchEllipses.Count()}"
                    + $"|T{query.BiTemporalSimilarityQuery.TextSketchQuery.LatterQuery.ColorSketchEllipses.Count()}"
                    + $")");
            }

            // color saturation / percent of black
            if (query.FormerFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.FormerFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off)
            {
                interactionValues.Add($"BW_1("
                    + $"CS"
                    + $"-{(query.FormerFilteringQuery.ColorSaturationQuery.FilterState == ThresholdFilteringQuery.State.ExcludeAboveThreshold ? "E" : "I")}"
                    + $"-{(int)(query.FormerFilteringQuery.ColorSaturationQuery.Threshold * 100)}%"
                    + $"|PBC"
                    + $"-{(query.FormerFilteringQuery.PercentOfBlackQuery.FilterState == ThresholdFilteringQuery.State.ExcludeAboveThreshold ? "E" : "I")}"
                    + $"-{(int)(query.FormerFilteringQuery.PercentOfBlackQuery.Threshold * 100)}%"
                    + $")");
            }
            if (query.LatterFilteringQuery.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.LatterFilteringQuery.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off)
            {
                interactionValues.Add($"BW_2("
                    + $"CS"
                    + $"-{(query.LatterFilteringQuery.ColorSaturationQuery.FilterState == ThresholdFilteringQuery.State.ExcludeAboveThreshold ? "E" : "I")}"
                    + $"-{(int)(query.LatterFilteringQuery.ColorSaturationQuery.Threshold * 100)}%"
                    + $"|PBC"
                    + $"-{(query.LatterFilteringQuery.PercentOfBlackQuery.FilterState == ThresholdFilteringQuery.State.ExcludeAboveThreshold ? "E" : "I")}"
                    + $"-{(int)(query.LatterFilteringQuery.PercentOfBlackQuery.Threshold * 100)}%"
                    + $")");
            }

            // TODO: both former and latter filtering queries are the same

            // max frames
            if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled
                || query.LatterFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Enabled)
            {
                interactionValues.Add($"MaxFrames("
                    + $"V{query.FormerFilteringQuery.CountFilteringQuery.MaxPerVideo}"
                    + $"|S{query.FormerFilteringQuery.CountFilteringQuery.MaxPerShot}"
                    + $")");
            }
            else if (query.FormerFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled
                || query.LatterFilteringQuery.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled)
            {
                // should be always on by default
                //throw new NotImplementedException("Unexpected behavior.");
                // TODO: should not throw exception during log submission
            }

            // ASR
            if (!query.FormerFilteringQuery.TranscriptFilteringQuery.IsEmpty())
            {
                interactionValues.Add($"ASR({query.FormerFilteringQuery.TranscriptFilteringQuery.VideoTranscriptQuery})");
            }

            // lifelog
            if (!query.FormerFilteringQuery.LifelogFilteringQuery.IsEmpty())
            {
                LifelogFilteringQuery lifelogQuery = query.FormerFilteringQuery.LifelogFilteringQuery;
                interactionValues.Add($"Lifelog(" +
                    $"Days:{string.Join(",", lifelogQuery.DaysOfWeek.Select(x => Enum.GetName(typeof(DayOfWeek), x)))}" +
                    $"|Months:{string.Join(",", lifelogQuery.MonthsOfYear)}" +
                    $"|Years:{string.Join(",", lifelogQuery.Years)}" +
                    $"|Time:{lifelogQuery.TimeFrom:hh\\:mm\\:ss}-{lifelogQuery.TimeTo:hh\\:mm\\:ss}" +
                    $"|Heartrate:{lifelogQuery.HeartRateLow}-{lifelogQuery.HeartRateHigh}" +
                    $"|Location:{lifelogQuery.GpsLocation ?? ""}" +
                    $"|GPS:[{(lifelogQuery?.GpsLatitude != null ? lifelogQuery.GpsLatitude.ToString() : "")}" +
                    $",{(lifelogQuery.GpsLongitude != null ? lifelogQuery.GpsLongitude.ToString() : "")}]" +
                    $")");
            }

            return interactionValues.ToArray();
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


    }
}
