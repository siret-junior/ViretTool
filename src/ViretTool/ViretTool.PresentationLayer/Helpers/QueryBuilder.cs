//using System;
//using System.Linq;
//using ViretTool.BusinessLayer.Descriptors.Models;
//using ViretTool.BusinessLayer.RankingModels.Queries;
//using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
//using ViretTool.BusinessLayer.Services;
//using ViretTool.PresentationLayer.Controls.Common;
//using ViretTool.PresentationLayer.Controls.Common.LifelogFilters;
//using ViretTool.PresentationLayer.Controls.Common.TranscriptFilter;
//using ViretTool.PresentationLayer.Controls.Query.ViewModels;

//namespace ViretTool.PresentationLayer.Helpers
//{
//    public class QueryBuilder
//    {
//        private readonly IDatasetServicesManager _datasetServicesManager;

//        public QueryBuilder(IDatasetServicesManager datasetServicesManager)
//        {
//            _datasetServicesManager = datasetServicesManager;
//        }

//        public BiTemporalQuery BuildQuery(
//            QueryViewModel query1,
//            QueryViewModel query2,
//            bool isFirstQueryPrimary,
//            int maxFramesFromVideo,
//            int maxFramesFromShot,
//            IDatasetParameters datasetParameters,
//            FrameViewModel frameForGpsFilter,
//            LifelogFilterViewModel lifelogFilters,
//            TranscriptFilterViewModel transcriptFilters)
//        {
//            BiTemporalModelQuery<KeywordQuery> biTemporalKeywordQuery =
//                new BiTemporalModelQuery<KeywordQuery>(
//                    new KeywordQuery(
//                        query1.KeywordQueryResult?.Query ?? new string[] { }),
//                    new KeywordQuery(
//                        query2.KeywordQueryResult?.Query ?? new string[] { }));

//            BiTemporalModelQuery<ColorSketchQuery> biTemporalColorSketchQuery =
//                new BiTemporalModelQuery<ColorSketchQuery>(
//                    new ColorSketchQuery(
//                        query1.CanvasWidth,
//                        query1.CanvasHeight,
//                        query1.SketchQueryResult?.SketchColorPoints?
//                              .Where(p => p.SketchType == Controls.Common.Sketches.SketchType.Color)
//                              .Select(
//                                  point => new Ellipse(
//                                      point.Area ? Ellipse.State.All : Ellipse.State.Any,
//                                      point.Position.X,
//                                      point.Position.Y,
//                                      point.EllipseAxis.X,
//                                      point.EllipseAxis.Y,
//                                      0,
//                                      point.FillColor.R,
//                                      point.FillColor.G,
//                                      point.FillColor.B))
//                              .ToArray() ??
//                        new Ellipse[0]
//                    ),
//                    new ColorSketchQuery(
//                        query2.CanvasWidth,
//                        query2.CanvasHeight,
//                        query2.SketchQueryResult?.SketchColorPoints?
//                              .Where(p => p.SketchType == Controls.Common.Sketches.SketchType.Color)
//                              .Select(
//                                  point => new Ellipse(
//                                      point.Area ? Ellipse.State.All : Ellipse.State.Any,
//                                      point.Position.X,
//                                      point.Position.Y,
//                                      point.EllipseAxis.X,
//                                      point.EllipseAxis.Y,
//                                      0,
//                                      point.FillColor.R,
//                                      point.FillColor.G,
//                                      point.FillColor.B))
//                              .ToArray() ??
//                        new Ellipse[0]
//                    )
//                );

//            BiTemporalModelQuery<ColorSketchQuery> biTemporalFaceSketchQuery =
//                new BiTemporalModelQuery<ColorSketchQuery>(
//                    new ColorSketchQuery(
//                        query1.CanvasWidth,
//                        query1.CanvasHeight,
//                        query1.SketchQueryResult?.SketchColorPoints?
//                              .Where(p => p.SketchType == Controls.Common.Sketches.SketchType.Face)
//                              .Select(
//                                  point => new Ellipse(
//                                      point.Area ? Ellipse.State.All : Ellipse.State.Any,
//                                      point.Position.X,
//                                      point.Position.Y,
//                                      point.EllipseAxis.X,
//                                      point.EllipseAxis.Y,
//                                      0,
//                                      point.FillColor.R,
//                                      point.FillColor.G,
//                                      point.FillColor.B))
//                              .ToArray() ??
//                        new Ellipse[0]
//                    ),
//                    new ColorSketchQuery(
//                        query2.CanvasWidth,
//                        query2.CanvasHeight,
//                        query2.SketchQueryResult?.SketchColorPoints?
//                              .Where(p => p.SketchType == Controls.Common.Sketches.SketchType.Face)
//                              .Select(
//                                  point => new Ellipse(
//                                      point.Area ? Ellipse.State.All : Ellipse.State.Any,
//                                      point.Position.X,
//                                      point.Position.Y,
//                                      point.EllipseAxis.X,
//                                      point.EllipseAxis.Y,
//                                      0,
//                                      point.FillColor.R,
//                                      point.FillColor.G,
//                                      point.FillColor.B))
//                              .ToArray() ??
//                        new Ellipse[0]
//                    )
//                );

//            BiTemporalModelQuery<ColorSketchQuery> biTemporalTextSketchQuery =
//                new BiTemporalModelQuery<ColorSketchQuery>(
//                    new ColorSketchQuery(
//                        query1.CanvasWidth,
//                        query1.CanvasHeight,
//                        query1.SketchQueryResult?.SketchColorPoints?
//                              .Where(p => p.SketchType == Controls.Common.Sketches.SketchType.Text)
//                              .Select(
//                                  point => new Ellipse(
//                                      point.Area ? Ellipse.State.All : Ellipse.State.Any,
//                                      point.Position.X,
//                                      point.Position.Y,
//                                      point.EllipseAxis.X,
//                                      point.EllipseAxis.Y,
//                                      0,
//                                      point.FillColor.R,
//                                      point.FillColor.G,
//                                      point.FillColor.B))
//                              .ToArray() ??
//                        new Ellipse[0]
//                    ),
//                    new ColorSketchQuery(
//                        query2.CanvasWidth,
//                        query2.CanvasHeight,
//                        query2.SketchQueryResult?.SketchColorPoints?
//                              .Where(p => p.SketchType == Controls.Common.Sketches.SketchType.Text)
//                              .Select(
//                                  point => new Ellipse(
//                                      point.Area ? Ellipse.State.All : Ellipse.State.Any,
//                                      point.Position.X,
//                                      point.Position.Y,
//                                      point.EllipseAxis.X,
//                                      point.EllipseAxis.Y,
//                                      0,
//                                      point.FillColor.R,
//                                      point.FillColor.G,
//                                      point.FillColor.B))
//                              .ToArray() ??
//                        new Ellipse[0]
//                    )
//                );

//            BiTemporalModelQuery<SemanticExampleQuery> biTemporalSemanticExampleQuery =
//                new BiTemporalModelQuery<SemanticExampleQuery>(
//                    new SemanticExampleQuery(
//                        query1.QueryObjects.Where(q => !(q is DownloadedFrameViewModel))
//                              .Select(q => _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(q.VideoId, q.FrameNumber))
//                              .ToArray(),
//                        new int[0],
//                        query1.QueryObjects.OfType<DownloadedFrameViewModel>().Select(q => q.ImageForExtractionPath).ToArray()),
//                    new SemanticExampleQuery(
//                        query2.QueryObjects.Where(q => !(q is DownloadedFrameViewModel))
//                              .Select(q => _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(q.VideoId, q.FrameNumber))
//                              .ToArray(),
//                        new int[0],
//                        query2.QueryObjects.OfType<DownloadedFrameViewModel>().Select(q => q.ImageForExtractionPath).ToArray()));


//            FusionQuery formerFusionQuery =
//                new FusionQuery(
//                    query1.KeywordUseForSorting
//                        ? FusionQuery.SimilarityModels.Keyword
//                        : query1.ColorUseForSorting
//                            ? FusionQuery.SimilarityModels.ColorSketch
//                            //: Query1.Face
//                            //    ? FusionQuery.SimilarityModels.FaceSketch
//                            //: Query1.Text
//                            //    ? FusionQuery.SimilarityModels.TextSketch
//                            : query1.SemanticUseForSorting
//                                ? FusionQuery.SimilarityModels.SemanticExample
//                                : FusionQuery.SimilarityModels.None,

//                    new ThresholdFilteringQuery(
//                        (biTemporalKeywordQuery.FormerQuery.Query.Any() || biTemporalKeywordQuery.LatterQuery.Query.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query1.KeywordValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        (biTemporalColorSketchQuery.FormerQuery.ColorSketchEllipses.Any() || biTemporalColorSketchQuery.LatterQuery.ColorSketchEllipses.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query1.ColorValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        (biTemporalFaceSketchQuery.FormerQuery.ColorSketchEllipses.Any() || biTemporalFaceSketchQuery.LatterQuery.ColorSketchEllipses.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query1.ColorValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        (biTemporalTextSketchQuery.FormerQuery.ColorSketchEllipses.Any() || biTemporalTextSketchQuery.FormerQuery.ColorSketchEllipses.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query1.ColorValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        biTemporalSemanticExampleQuery.FormerQuery.PositiveExampleIds.Any() ||
//                        biTemporalSemanticExampleQuery.FormerQuery.ExternalImages.Any() ||
//                        biTemporalSemanticExampleQuery.LatterQuery.PositiveExampleIds.Any() ||
//                        biTemporalSemanticExampleQuery.LatterQuery.ExternalImages.Any()
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query1.SemanticValue * 0.01)
//                );

//            FusionQuery latterFusionQuery =
//                new FusionQuery(
//                    query2.KeywordUseForSorting
//                        ? FusionQuery.SimilarityModels.Keyword
//                        : query2.ColorUseForSorting
//                            ? FusionQuery.SimilarityModels.ColorSketch
//                            //: Query1.Face
//                            //    ? FusionQuery.SimilarityModels.FaceSketch
//                            //: Query1.Text
//                            //    ? FusionQuery.SimilarityModels.TextSketch
//                            : query2.SemanticUseForSorting
//                                ? FusionQuery.SimilarityModels.SemanticExample
//                                : FusionQuery.SimilarityModels.None,
//                    new ThresholdFilteringQuery(
//                        (biTemporalKeywordQuery.FormerQuery.Query.Any() || biTemporalKeywordQuery.LatterQuery.Query.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query2.KeywordValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        (biTemporalColorSketchQuery.FormerQuery.ColorSketchEllipses.Any() || biTemporalColorSketchQuery.LatterQuery.ColorSketchEllipses.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query2.ColorValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        (biTemporalFaceSketchQuery.FormerQuery.ColorSketchEllipses.Any() || biTemporalFaceSketchQuery.LatterQuery.ColorSketchEllipses.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query2.ColorValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        (biTemporalTextSketchQuery.FormerQuery.ColorSketchEllipses.Any() || biTemporalTextSketchQuery.LatterQuery.ColorSketchEllipses.Any())
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query2.ColorValue * 0.01),

//                    new ThresholdFilteringQuery(
//                        biTemporalSemanticExampleQuery.FormerQuery.PositiveExampleIds.Any() ||
//                        biTemporalSemanticExampleQuery.FormerQuery.ExternalImages.Any() ||
//                        biTemporalSemanticExampleQuery.LatterQuery.PositiveExampleIds.Any() ||
//                        biTemporalSemanticExampleQuery.LatterQuery.ExternalImages.Any()
//                            ? ThresholdFilteringQuery.State.IncludeAboveThreshold
//                            : ThresholdFilteringQuery.State.Off,
//                        query2.SemanticValue * 0.01));


//            FilteringQuery formerFilteringQuery = new FilteringQuery(
//                new ThresholdFilteringQuery(ConvertToFilterState(query1.BwFilterState), query1.BwFilterValue * 0.01),
//                new ThresholdFilteringQuery(ConvertToFilterState(query1.PercentageBlackFilterState), query1.PercentageBlackFilterValue * 0.01),
//                new CountFilteringQuery(CountFilteringQuery.State.Enabled, maxFramesFromVideo, maxFramesFromShot, -1),
//                GetLifelogFilteringQuery(datasetParameters, frameForGpsFilter, lifelogFilters),
//                new TranscriptFilteringQuery(transcriptFilters.InputText));

//            FilteringQuery latterFilteringQuery = new FilteringQuery(
//                new ThresholdFilteringQuery(ConvertToFilterState(query2.BwFilterState), query2.BwFilterValue * 0.01),
//                new ThresholdFilteringQuery(ConvertToFilterState(query2.PercentageBlackFilterState), query2.PercentageBlackFilterValue * 0.01),
//                new CountFilteringQuery(CountFilteringQuery.State.Enabled, maxFramesFromVideo, maxFramesFromShot, -1),
//                GetLifelogFilteringQuery(datasetParameters, frameForGpsFilter, lifelogFilters),
//                new TranscriptFilteringQuery(transcriptFilters.InputText));


//            BiTemporalQuery biTemporalQuery =
//                new BiTemporalQuery(
//                    isFirstQueryPrimary ? BiTemporalQuery.TemporalQueries.Former : BiTemporalQuery.TemporalQueries.Latter,
//                    new BiTemporalSimilarityQuery(
//                        biTemporalKeywordQuery,
//                        biTemporalColorSketchQuery,
//                        biTemporalFaceSketchQuery,
//                        biTemporalTextSketchQuery,
//                        biTemporalSemanticExampleQuery
//                    ),
//                    formerFusionQuery,
//                    latterFusionQuery,
//                    formerFilteringQuery,
//                    latterFilteringQuery);


//            return biTemporalQuery;
//        }
        
//        private static ThresholdFilteringQuery.State ConvertToFilterState(FilterControl.FilterState filterState)
//        {
//            switch (filterState)
//            {
//                case FilterControl.FilterState.Y:
//                    return ThresholdFilteringQuery.State.ExcludeAboveThreshold;
//                case FilterControl.FilterState.N:
//                    return ThresholdFilteringQuery.State.IncludeAboveThreshold;
//                case FilterControl.FilterState.Off:
//                    return ThresholdFilteringQuery.State.Off;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(filterState), filterState, "Uknown filtering state.");
//            }
//        }

//        private LifelogFilteringQuery GetLifelogFilteringQuery(IDatasetParameters datasetParameters, FrameViewModel frameForGpsFilter, LifelogFilterViewModel lifelogFilters)
//        {
//            if (!datasetParameters.IsLifelogData)
//            {
//                return new LifelogFilteringQuery();
//            }

//            LifelogFrameMetadata metadata = null;
//            if (frameForGpsFilter != null)
//            {
//                int frameId = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(frameForGpsFilter.VideoId, frameForGpsFilter.FrameNumber);
//                metadata = _datasetServicesManager.CurrentDataset.LifelogDescriptorProvider[frameId];
//            }

//            return new LifelogFilteringQuery(
//                lifelogFilters.SelectedDaysOfWeek.ToArray(),
//                lifelogFilters.SelectedMonthsOfYear.ToArray(),
//                lifelogFilters.SelectedYears.ToArray(),
//                TimeSpan.FromHours(lifelogFilters.StartTimeHour),
//                TimeSpan.FromHours(lifelogFilters.EndTimeHour),
//                lifelogFilters.HeartbeatLow,
//                lifelogFilters.HeartbeatHigh,
//                metadata?.GpsLocation,
//                metadata?.GpsLatitude,
//                metadata?.GpsLongitude);
//        }
//    }
//}
