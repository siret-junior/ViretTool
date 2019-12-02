using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.BusinessLayer.Datasets;
using Action = System.Action;
using System.Windows.Media;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class ZoomDisplayControlViewModel : DisplayControlViewModelBase
    {
        private FrameViewModel _gpsFrame;

        /// <summary>
        /// Indicates the layer of SOM, in which the ZoomDisplay is currently located. Zero-based numbering.
        /// </summary>
        private int _currentLayer;

        private IZoomDisplayProvider _zoomDisplayProvider;

        public ZoomDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger iterationLogger)
            : base(logger, datasetServicesManager, iterationLogger)
        {
            datasetServicesManager.DatasetOpened += (_, services) =>
            {
                _zoomDisplayProvider = datasetServicesManager.CurrentDataset.ZoomDisplayProvider;
            };
        }


        public bool ShowZoomIntoButton
        {
            get
            {
                if(_currentLayer == _zoomDisplayProvider.GetMaxDepth())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool ShowZoomOutButton
        {
            get
            {
                if (_currentLayer == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public FrameViewModel GpsFrame
        {
            get => _gpsFrame;
            set
            {
                if (_gpsFrame?.Equals(value) == true)
                {
                    return;
                }

                _gpsFrame = value;
                _interactionLogger.LogInteraction(LogCategory.Filter, LogType.Lifelog, _gpsFrame == null ? "" : $"{_gpsFrame.VideoId}|{_gpsFrame.FrameNumber}");
                NotifyQuerySettingsChanged();
                NotifyOfPropertyChange();

            }
        }

        public void DeleteGpsFrame()
        {
            GpsFrame = null;
        }


        public ISubject<Unit> QuerySettingsChanged { get; } = new Subject<Unit>();


        public override async Task LoadInitialDisplay()
        {
            IsInitialDisplayShown = false;

            _currentLayer = 0;
            this.NotifyOfPropertyChange("ShowZoomOutButton");
            this.NotifyOfPropertyChange("ShowZoomIntoButton");

            // Get first layer of SOM
            int[][] ids = _zoomDisplayProvider.GetInitialLayer();
            _loadedFrames = await Task.Run(() => ids.SelectMany(x=>x).Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

            
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            InitBorders();

            // UpdateVisibleFrames() loads frames to screen
            UpdateVisibleFrames();
            IsInitialDisplayShown = true;
        }

        /// <summary>
        /// Computes position of center in _loadedFrames
        /// </summary>
        /// <returns></returns>
        private int ComputeCenter()
        {
            return ColumnCount * (RowCount / 2 - 1) + (ColumnCount / 2) - 1;
        }
        public async void KeyRightPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() + 1]);
        }
        public async void KeyLeftPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() - 1]);
        }
        public async void KeyUpPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() - ColumnCount]);
        }
        public async void KeyDownPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() + ColumnCount]);
        }

        public async Task LoadMoveAtCurrentLayerDisplayForFrame(FrameViewModel selectedFrame)
        {
            await LoadFramesForIds(new int[] { _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(selectedFrame.VideoId, selectedFrame.FrameNumber) }, _zoomDisplayProvider.ZoomIntoLayer);
        }

        public async Task LoadZoomIntoDisplayForFrame(FrameViewModel selectedFrame)
        {
            _currentLayer++;
            this.NotifyOfPropertyChange("ShowZoomOutButton");
            this.NotifyOfPropertyChange("ShowZoomIntoButton");
            await LoadFramesForIds(new int[] { _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(selectedFrame.VideoId, selectedFrame.FrameNumber) }, _zoomDisplayProvider.ZoomIntoLayer);
        }

        public async Task LoadZoomOutDisplayForFrame(FrameViewModel selectedFrame)
        {
            _currentLayer--;
            this.NotifyOfPropertyChange("ShowZoomOutButton");
            this.NotifyOfPropertyChange("ShowZoomIntoButton");
            // TODO: Change functionality to ZoomOut
            await LoadFramesForIds(new int[] { _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(selectedFrame.VideoId, selectedFrame.FrameNumber) }, _zoomDisplayProvider.ZoomOutOfLayer);
        }

        private void InitBorders()
        {
            foreach(FrameViewModel f in _loadedFrames)
            {
                int frameID = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(f.VideoId, f.FrameNumber);
                (float BottomBorderSimilarity, float RightBorderSimilarity) = _zoomDisplayProvider.GetColorSimilarity(_currentLayer, frameID);
                if(BottomBorderSimilarity > 0.5f)
                {
                    f.BottomBorderColor = Colors.Red;
                }
                else
                {
                    f.BottomBorderColor = Colors.Green;
                }
                if (RightBorderSimilarity > 0.5f)
                {
                    f.RightBorderColor = Colors.Blue;
                }
                else
                {
                    f.RightBorderColor = Colors.Yellow;
                }
            }
            // Make bottom border invisible for the last row
            for (int i = 1; i <= ColumnCount; i++)
            {
                if (_loadedFrames.Count() - 1 - ColumnCount + i >= _loadedFrames.Count())
                    break;
                _loadedFrames[_loadedFrames.Count() - 1 - ColumnCount + i].IsBottomBorderVisible = false;

            }

            // Make right border invisible for the last column
            for (int i = 1; i <= RowCount; i++)
            {
                if ((i * ColumnCount) - 1 >= _loadedFrames.Count())
                    break;
                _loadedFrames[(i * ColumnCount) - 1].IsRightBorderVisible = false;
            }
        }
        public async Task LoadFramesForIds(IEnumerable<int> inputFrameIds, Func<int,int,int,int,int[]> TypeOfZoom)
        {
            int inputFrameId = inputFrameIds.First();

            // for now, it is required to precompute row and column counts
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            int[] ids = TypeOfZoom(_currentLayer,inputFrameId, RowCount, ColumnCount);
            _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

            InitBorders();
            
            // TODO: disable async loading for consistent loading when scrolling

            UpdateVisibleFrames();
            IsInitialDisplayShown = false;
        }


        protected override void UpdateVisibleFrames()
        {
            // Contents of _loadedFrames depend on context. 
            // As an example, it could be entire 1M dataset sorted by relevance from which we select only the top RowCount*ColumnCount items.

            // for now, it is required to set row and column counts (do not remove this)
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            // In the example code in LoadFramesForIds we already precomputed frames that are ready to be displayed.
            AddFramesToVisibleItems(VisibleFrames, _loadedFrames);
        }

        private void NotifyQuerySettingsChanged()
        {
            QuerySettingsChanged.OnNext(Unit.Default);
        }

    }
}
