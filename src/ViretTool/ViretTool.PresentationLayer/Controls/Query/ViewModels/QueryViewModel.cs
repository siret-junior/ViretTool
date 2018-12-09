﻿using System;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch;
using ViretTool.PresentationLayer.Controls.Common.Sketches;

namespace ViretTool.PresentationLayer.Controls.Query.ViewModels
{
    public class QueryViewModel : PropertyChangedBase
    {
        private readonly ILogger _logger;

        public EventHandler QuerySettingsChanged;
        private FilterControl.FilterState _bwFilterState;
        private double _bwFilterValue;
        private bool _colorUseForSorting;
        private double _colorValue;
        private KeywordQueryResult _keywordQueryResult;
        private bool _keywordUseForSorting;
        private double _keywordValue;
        private FilterControl.FilterState _percentageBlackFilterState;
        private double _percentageBlackFilterValue;
        private bool _semanticUseForSorting;
        private double _semanticValue;
        private SketchQueryResult _sketchQueryResult;

        public QueryViewModel(ILogger logger)
        {
            _logger = logger;
            //when any property is changed, new settings are rebuild - maybe we want to throttle?
            PropertyChanged += BuildQuerySettings;
        }

        public FilterControl.FilterState BwFilterState
        {
            get => _bwFilterState;
            set
            {
                if (_bwFilterState == value)
                {
                    return;
                }

                _bwFilterState = value;
                NotifyOfPropertyChange();
            }
        }

        public double BwFilterValue
        {
            get => _bwFilterValue;
            set
            {
                if (_bwFilterValue == value)
                {
                    return;
                }

                _bwFilterValue = value;
                NotifyOfPropertyChange();
            }
        }

        public bool ColorUseForSorting
        {
            get => _colorUseForSorting;
            set
            {
                if (_colorUseForSorting == value)
                {
                    return;
                }

                _colorUseForSorting = value;
                NotifyOfPropertyChange();
            }
        }

        public double ColorValue
        {
            get => _colorValue;
            set
            {
                if (_colorValue == value)
                {
                    return;
                }

                _colorValue = value;
                NotifyOfPropertyChange();
            }
        }

        public BusinessLayer.RankingModels.Queries.Query FinalQuery { get; private set; }

        public Action<string, string[]> InitializeKeywordSearchMethod { private get; set; }

        public KeywordQueryResult KeywordQueryResult
        {
            get => _keywordQueryResult;
            set
            {
                if (_keywordQueryResult == value)
                {
                    return;
                }

                _keywordQueryResult = value;
                NotifyOfPropertyChange();
            }
        }

        public bool KeywordUseForSorting
        {
            get => _keywordUseForSorting;
            set
            {
                if (_keywordUseForSorting == value)
                {
                    return;
                }

                _keywordUseForSorting = value;
                NotifyOfPropertyChange();
            }
        }

        public double KeywordValue
        {
            get => _keywordValue;
            set
            {
                if (_keywordValue == value)
                {
                    return;
                }

                _keywordValue = value;
                NotifyOfPropertyChange();
            }
        }

        public FilterControl.FilterState PercentageBlackFilterState
        {
            get => _percentageBlackFilterState;
            set
            {
                if (_percentageBlackFilterState == value)
                {
                    return;
                }

                _percentageBlackFilterState = value;
                NotifyOfPropertyChange();
            }
        }

        public double PercentageBlackFilterValue
        {
            get => _percentageBlackFilterValue;
            set
            {
                if (_percentageBlackFilterValue == value)
                {
                    return;
                }

                _percentageBlackFilterValue = value;
                NotifyOfPropertyChange();
            }
        }

        public bool SemanticUseForSorting
        {
            get => _semanticUseForSorting;
            set
            {
                if (_semanticUseForSorting == value)
                {
                    return;
                }

                _semanticUseForSorting = value;
                NotifyOfPropertyChange();
            }
        }

        public double SemanticValue
        {
            get => _semanticValue;
            set
            {
                if (_semanticValue == value)
                {
                    return;
                }

                _semanticValue = value;
                NotifyOfPropertyChange();
            }
        }

        public SketchQueryResult SketchQueryResult
        {
            get => _sketchQueryResult;
            set
            {
                if (_sketchQueryResult == value)
                {
                    return;
                }

                _sketchQueryResult = value;
                NotifyOfPropertyChange();
            }
        }

        public void InitializeKeywordSearch(string datasetPath, string[] annotationSources)
        {
            InitializeKeywordSearchMethod(datasetPath, annotationSources);
        }

        private void BuildQuerySettings(object sender, PropertyChangedEventArgs args)
        {
            _logger.Info($"Query changed: {args.PropertyName}: {sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender)}");
            //MessageBox.Show($"{args.PropertyName}: {sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender)}");

            //TODO a lot of unclear settings
            string keywordAnnotationSource = KeywordQueryResult?.AnnotationSource;
            KeywordQuery keyWordQuery = new KeywordQuery(
                KeywordQueryResult?.Query?.Select(parts => new SynsetGroup(parts.Select(p => new Synset(keywordAnnotationSource, p)).ToArray())).ToArray() ?? new SynsetGroup[0]);

            var colorSketchQuery = new ColorSketchQuery(
                -1,
                -1,
                SketchQueryResult?.SketchColorPoints?.Select(
                                     point => new Ellipse(
                                         point.Area ? Ellipse.State.Any : Ellipse.State.All,
                                         (int)point.Position.X,
                                         (int)point.Position.Y,
                                         (int)point.EllipseAxis.X,
                                         (int)point.EllipseAxis.Y,
                                         0,
                                         point.FillColor.R,
                                         point.FillColor.G,
                                         point.FillColor.B))
                                 .ToArray());

            //TODO use (e.g. color) for sorting?
            var semanticExampleQuery = new SemanticExampleQuery(new int[0], new int[0]);

            FinalQuery = new BusinessLayer.RankingModels.Queries.Query(
                new SimilarityQuery(keyWordQuery, colorSketchQuery, semanticExampleQuery),
                new FilteringQuery(
                    new ThresholdFilteringQuery(ConvertToFilterState(BwFilterState), BwFilterValue),
                    new ThresholdFilteringQuery(ConvertToFilterState(PercentageBlackFilterState), PercentageBlackFilterValue),
                    new ThresholdFilteringQuery(ThresholdFilteringQuery.State.FilterAboveThreshold, ColorValue),
                    new ThresholdFilteringQuery(ThresholdFilteringQuery.State.FilterAboveThreshold, KeywordValue),
                    new ThresholdFilteringQuery(ThresholdFilteringQuery.State.FilterAboveThreshold, SemanticValue)));

            QuerySettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private ThresholdFilteringQuery.State ConvertToFilterState(FilterControl.FilterState filterState)
        {
            switch (filterState)
            {
                case FilterControl.FilterState.Y:
                    return ThresholdFilteringQuery.State.FilterAboveThreshold;
                case FilterControl.FilterState.N:
                    return ThresholdFilteringQuery.State.FilterBelowThreshold;
                case FilterControl.FilterState.Off:
                    return ThresholdFilteringQuery.State.Off;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filterState), filterState, "Uknown filtering state.");
            }
        }
    }
}