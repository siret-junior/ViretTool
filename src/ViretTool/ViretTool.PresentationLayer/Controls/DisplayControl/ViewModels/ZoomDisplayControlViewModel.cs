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
        private readonly Random _random = new Random();

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
            };
        }

        public int CurrentLayer => _currentLayer;
        public int LayerCount => _zoomDisplayProvider.GetMaxDepth();

        /// <summary>
        /// True if user is not in last layer
        /// </summary>
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

        /// <summary>
        /// true if user is not in first layer
        /// </summary>
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
       

        /// <summary>
        /// Loads initial display
        /// </summary>
        /// <returns></returns>
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
                ids = _zoomDisplayProvider.GetInitialLayer(RowCount, ColumnCount);

            }
            // load smaller layer if screen is too big
            catch (ArgumentOutOfRangeException)
            {
                (ids, ColumnCount, RowCount) = _zoomDisplayProvider.GetSmallLayer(_currentLayer, RowCount, ColumnCount);
                ImageWidth = DisplayWidth / ColumnCount;
                ImageHeight = DisplayHeight / RowCount;
            }
            if (ids != null) 
            {
                _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

                // update borders
                UpdateBorderColors();

            }
            else
            {
                // calculate random grid
                await RandomGridDisplay();
            }

            // load frames to screen
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

        /// <summary>
        /// Move display in specified direction
        /// </summary>
        /// <param name="selectedFrame"></param>
        /// <param name="typeOfMove">direction</param>
        /// <returns></returns>
        public virtual async Task LoadMoveAtCurrentLayerDisplayForFrame(FrameViewModel selectedFrame, Func<int,int,int,int,int[]> typeOfMove)
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

        /// <summary>
        /// loads and updates border colors
        /// </summary>
        protected void UpdateBorderColors()
        {
            // get border values from ZoomDisplayProvider, 
            // bottom border value for n-th frame is at (2*n)-th index, 
            // right border value for n-th frame is at (2*n + 1)-th index
            float[] borderSimilarities = _zoomDisplayProvider.GetBorderSimilarities(_currentLayer, RowCount, ColumnCount);
            for (int iFrame = 0; iFrame < _loadedFrames.Count; iFrame++)
            {
                (float BottomBorderSimilarity, float RightBorderSimilarity) = (borderSimilarities[iFrame * 2], borderSimilarities[(iFrame * 2) + 1]);

                // set color interval
                System.Drawing.Color colorSimilar = System.Drawing.Color.Lime;
                System.Drawing.Color colorDissimilar = System.Drawing.Color.Red;

                // Map the similarity into Color
                System.Drawing.Color bottomColor = ColorInterpolationHelper.InterpolateColorHSV(colorDissimilar, colorSimilar, BottomBorderSimilarity);
                System.Drawing.Color rightColor = ColorInterpolationHelper.InterpolateColorHSV(colorDissimilar, colorSimilar, RightBorderSimilarity);

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
                ImageWidth = DisplayWidth / ColumnCount;
                ImageHeight = DisplayHeight / RowCount;
            }

            if (ids != null)
            {
                _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());
                UpdateBorderColors();
            }
            else
            {
                await RandomGridDisplay();
            }

            UpdateVisibleFrames();
            IsInitialDisplayShown = false;
        }

        /// <summary>
        /// operate resizing of the display
        /// </summary>
        /// <returns></returns>
        private async Task ResizeDisplay()
        {
            // new row/column length
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;
            if(_loadedFrames.Count > 0)
            {
                // get frameID
                int frameNumber = _loadedFrames.First().FrameNumber;
                int videoId = _loadedFrames.First().VideoId;
                int frameId = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(videoId, frameNumber);


                int[] ids = null;
                try
                {
                    // compute new items
                    ids = _zoomDisplayProvider.Resize(_currentLayer, frameId, RowCount, ColumnCount);
                }
                catch (ArgumentOutOfRangeException)
                {
                    (ids,ColumnCount,RowCount) = _zoomDisplayProvider.GetSmallLayer(_currentLayer, RowCount, ColumnCount);
                    ImageWidth = DisplayWidth / ColumnCount;
                    ImageHeight = DisplayHeight / RowCount;
                }

                if(ids != null)
                {
                    _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());
                    UpdateBorderColors();
                }
                else
                {
                    await RandomGridDisplay();
                }
                UpdateVisibleFrames();
            }
        }

        /// <summary>
        /// Computes random items into display
        /// </summary>
        /// <returns></returns>
        public async Task RandomGridDisplay()
        {
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            int displayedFrameCount = RowCount * ColumnCount;
            int datasetFrameCount = _datasetServicesManager.CurrentDataset.DatasetService.FrameCount;
            IEnumerable<int> randomFrameIds = Enumerable.Range(0, displayedFrameCount).Select(_ => _random.Next(datasetFrameCount));
            _loadedFrames = await Task.Run(() => randomFrameIds.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

        }

        public new Action DisplaySizeChangedHandler => async () =>
        {
            ResetGrid?.Invoke();
            await ResizeDisplay();
            UpdateVisibleFrames();
        };

        protected override void UpdateVisibleFrames()
        {
            AddFramesToVisibleItems(VisibleFrames, _loadedFrames);
        }


    }
}
