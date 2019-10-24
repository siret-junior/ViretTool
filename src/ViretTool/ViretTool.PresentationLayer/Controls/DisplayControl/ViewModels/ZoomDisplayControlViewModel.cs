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
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class ZoomDisplayControlViewModel : DisplayControlViewModelBase
    {
        private FrameViewModel _gpsFrame;

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
            // TODO: load the highest level of SOM
            await base.LoadInitialDisplay();
        }

        public async Task LoadDisplayForFrame(FrameViewModel selectedFrame)
        {
            await LoadFramesForIds(new int[] { _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(selectedFrame.VideoId, selectedFrame.FrameNumber) });
        }

        public override async Task LoadFramesForIds(IEnumerable<int> inputFrameIds)
        {
            // TODO: populate display based on the lowest level of SOM, with the first item of input seqence in the middle.
            
            // Converts the input set of integer frameIds into a set of displayed FrameViewModels
            // Here we will consider only a single input frameId that will be in the center of the grid display
            // and we will populate its surroundings based on the lowest level of SOM

            // Example code populating the first row of the display grid with temporal context around the input frame from its video:

            // we expect exactly one frameId
            // (change to .First() to have a more robust code taking any nonzero number of inputs but considering only the first one)
            int inputFrameId = inputFrameIds.Single();

            // for now, it is required to precompute row and column counts
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;


            // get parent videoId of the input frameId
            int videoId = _datasetServicesManager.CurrentDataset.DatasetService.GetVideoIdForFrameId(inputFrameId);
            
            // get all frameIds of the video
            int[] videoFrameIds = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdsForVideo(videoId);

            // extract a segment with the same length as column count, centered around the input frame
            int inputFrameIndex = Array.IndexOf(videoFrameIds, inputFrameId);
            int startIndex = Math.Max(0, inputFrameIndex - (ColumnCount / 2) + 1);
            int endIndex = Math.Min(videoFrameIds.Length - 1, inputFrameIndex + (ColumnCount / 2));
            int segmentLength = endIndex - startIndex + 1;
            if (segmentLength > ColumnCount)
            {
                throw new IndexOutOfRangeException($"segmentLength = {segmentLength} is more than ColumnCount = {ColumnCount}.");
            }
            int[] expandedFrameIds = new int[segmentLength];
            Array.Copy(videoFrameIds, startIndex, expandedFrameIds, 0, segmentLength);

            // base class will convert the expanded set of frameIds into FrameViewModels that are ready to be displayed (stored it _loadedFrames).
            await base.LoadFramesForIds(expandedFrameIds);
        }


        protected override void UpdateVisibleFrames()
        {
            // TODO: Populate display frames using _loadedFrames
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
