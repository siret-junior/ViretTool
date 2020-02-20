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
using System.Drawing;
using ViretTool.PresentationLayer.Helpers;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class ZoomDisplayControlViewModel : DisplayControlViewModelBase
    {
        private FrameViewModel _gpsFrame;

        /// <summary>
        /// Indicates the layer of SOM, in which the ZoomDisplay is currently located. Zero-based numbering.
        /// </summary>
        protected int _currentLayer;

        protected IZoomDisplayProvider _zoomDisplayProvider;

        public ZoomDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger iterationLogger)
            : base(logger, datasetServicesManager, iterationLogger)
        {
            datasetServicesManager.DatasetOpened += (_, services) =>
            {
                _zoomDisplayProvider = datasetServicesManager.CurrentDataset.ZoomDisplayProvider;
                LoadInitialDisplay();
            };
        }

        public int CurrentLayer => _currentLayer;
        public int LayerCount => _zoomDisplayProvider.GetMaxDepth();

        public bool ShowZoomIntoButton
        {
            get
            {
                if(_currentLayer >= _zoomDisplayProvider.GetMaxDepth())
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
                // TODO: enable lifelog filter
                //_interactionLogger.LogInteraction(LogCategory.Filter, LogType.Lifelog, _gpsFrame == null ? "" : $"{_gpsFrame.VideoId}|{_gpsFrame.FrameNumber}");
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


            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            // Get first layer of SOM
            int[] ids = null;
            try
            {
                _zoomDisplayProvider.GetInitialLayer(RowCount, ColumnCount);
            }
            catch (ArgumentOutOfRangeException)
            {
                (ids, ColumnCount, RowCount) = _zoomDisplayProvider.GetSmallLayer(_currentLayer, RowCount, ColumnCount);
            }
            if (ids != null) 
            {
                _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

                InitBorders();

            }
            else
            {
                await RandomGridDisplay();
            }

            // UpdateVisibleFrames() loads frames to screen
            UpdateVisibleFrames();
            IsInitialDisplayShown = true;
        }

        /// <summary>
        /// Computes position of center in _loadedFrames
        /// </summary>
        /// <returns></returns>
        protected int ComputeCenter()
        {
            return ColumnCount * (RowCount / 2 - 1) + (ColumnCount / 2) - 1;
        }
        public async void KeyRightPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() + 1], _zoomDisplayProvider.MoveRight);
        }
        public async void KeyLeftPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() - 1], _zoomDisplayProvider.MoveLeft);
        }
        public async void KeyUpPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() - ColumnCount], _zoomDisplayProvider.MoveUp);
        }
        public async void KeyDownPressed()
        {
            await LoadMoveAtCurrentLayerDisplayForFrame(_loadedFrames[ComputeCenter() + ColumnCount], _zoomDisplayProvider.MoveDown);
        }

        public async virtual Task LoadMoveAtCurrentLayerDisplayForFrame(FrameViewModel selectedFrame, Func<int,int,int,int,int[]> typeOfMove)
        {
            await LoadFramesForIds(new int[] { _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(selectedFrame.VideoId, selectedFrame.FrameNumber) }, typeOfMove);
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

        protected void InitBorders()
        {
            // get border values from ZoomDisplayProvider, bottom border value for n-th frame is at (2*n)-th index, right border value for n-th frame is at (2*n + 1)-th index
            float[] borders = _zoomDisplayProvider.GetColorSimilarity(_currentLayer, RowCount, ColumnCount);
            for (int iFrame = 0; iFrame < _loadedFrames.Count; iFrame++)
            {
                (float BottomBorderSimilarity, float RightBorderSimilarity) = (borders[iFrame * 2], borders[(iFrame * 2) + 1]);

                System.Drawing.Color colorSimilar = System.Drawing.Color.Lime;
                System.Drawing.Color colorDissimilar = System.Drawing.Color.Red;

                // Map the similarity into Color
                System.Drawing.Color bottomColor = ColorInterpolationHelper.InterpolateColorHSV(colorSimilar, colorDissimilar, 1 - BottomBorderSimilarity, true);
                System.Drawing.Color rightColor = ColorInterpolationHelper.InterpolateColorHSV(colorSimilar, colorDissimilar, 1 - RightBorderSimilarity, true);

                // Convert System.Drawing.Color (used by ColorInterpolationHelper) to System.Windows.Media.Color (used by WPF)
                FrameViewModel frame = _loadedFrames[iFrame];
                frame.BottomBorderColor = System.Windows.Media.Color.FromArgb(bottomColor.A, bottomColor.R, bottomColor.G, bottomColor.B);
                frame.RightBorderColor = System.Windows.Media.Color.FromArgb(rightColor.A, rightColor.R, rightColor.G, rightColor.B);

            }

            // Make bottom border invisible for the last row
            // Iterate over whole grid except the last row
            for (int i = 0; i < _loadedFrames.Count() - ColumnCount; i++)
            {
                _loadedFrames[i].IsBottomBorderVisible = true;
            }

            // Make right border invisible for the last column
            // Iterate over whole grid
            for (int i = 0; i < _loadedFrames.Count(); i++)
            {
                // If iterator is not the last element in row then set Visibility to true
                if ((i % ColumnCount) != ColumnCount - 1)
                    _loadedFrames[i].IsRightBorderVisible = true;
            }
        }
        public async Task LoadFramesForIds(IEnumerable<int> inputFrameIds, Func<int,int,int,int,int[]> TypeOfZoom)
        {
            int inputFrameId = inputFrameIds.First();

            // for now, it is required to precompute row and column counts
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            int[] ids = null;
            try
            {
                ids = TypeOfZoom(_currentLayer, inputFrameId, RowCount, ColumnCount);
            }
            catch (ArgumentOutOfRangeException)
            {
                (ids, ColumnCount, RowCount) = _zoomDisplayProvider.GetSmallLayer(_currentLayer, RowCount, ColumnCount);
            }
            if (ids != null)
            {
                _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

                InitBorders();

                
            }
            else
            {
                await RandomGridDisplay();
            }
            // TODO: disable async loading for consistent loading when scrolling

            UpdateVisibleFrames();

            IsInitialDisplayShown = false;

        }
        private async Task ResizeDisplay()
        {
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;
            if(_loadedFrames.Count > 0)
            {
                int frameNumber = _loadedFrames.First().FrameNumber;
                int videoId = _loadedFrames.First().VideoId;
                int frameId = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(videoId, frameNumber);
                int[] ids = null;
                try
                {
                    ids = _zoomDisplayProvider.Resize(_currentLayer, frameId, RowCount, ColumnCount);
                }
                catch (ArgumentOutOfRangeException)
                {
                    (ids,ColumnCount,RowCount) = _zoomDisplayProvider.GetSmallLayer(_currentLayer, RowCount, ColumnCount);
                }
                if(ids != null)
                {
                    _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

                    InitBorders();

                    // TODO: disable async loading for consistent loading when scrolling
                }
                else
                {
                    await RandomGridDisplay();
                }
                UpdateVisibleFrames();
            }
        }

        public async Task RandomGridDisplay()
        {

            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            Random rnd = new Random((int)DateTime.Now.Ticks);
            int[] ids = Enumerable.Range(0, RowCount * ColumnCount).Select(_ => rnd.Next(0, _datasetServicesManager.CurrentDataset.DatasetService.FrameCount)).ToArray();
            _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

        }

        public new Action DisplaySizeChangedHandler => async () =>
        {
            ResetGrid?.Invoke();
            await ResizeDisplay();
            UpdateVisibleFrames();
        };

        protected override void UpdateVisibleFrames()
        {
            // Contents of _loadedFrames depend on context. 
            // As an example, it could be entire 1M dataset sorted by relevance from which we select only the top RowCount*ColumnCount items.

            

            // In the example code in LoadFramesForIds we already precomputed frames that are ready to be displayed.
            AddFramesToVisibleItems(VisibleFrames, _loadedFrames);
        }

        private void NotifyQuerySettingsChanged()
        {
            QuerySettingsChanged.OnNext(Unit.Default);
        }

    }
}
