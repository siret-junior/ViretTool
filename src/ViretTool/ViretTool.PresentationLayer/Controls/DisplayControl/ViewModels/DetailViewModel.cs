using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Castle.Core.Logging;
using Viret;
using Viret.Logging.DresApi;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class DetailViewModel : ScrollableDisplayControlViewModel
    {
        public DetailViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            ViretCore viretCore/*,
            IInteractionLogger interactionLogger*/) : base(logger, datasetServicesManager/*, interactionLogger*/)
        {
            ColumnCount = viretCore.Config.DetailWindowColumns;
            RowCount = viretCore.Config.DetailWindowRows;
        }

        public BindableCollection<FrameViewModel> SampledFrames { get; } = new BindableCollection<FrameViewModel>();

        public Action<int> ScrollToSampleRow { private get; set; }

        public EventType BrowsingEvent = EventType.VideoSummary;
        public override async Task LoadVideoForFrame(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            await base.LoadVideoForFrame(selectedFrame);

            HashSet<int> shotFrames = _datasetServicesManager.CurrentDataset.DatasetService.GetShotFrameNumbersForVideo(selectedFrame.VideoId)
                                                             .Select(t => t.StartFrame)
                                                             .ToHashSet();
            List<FrameViewModel> viewModelsToAdd = _loadedFrames.Where(f => shotFrames.Contains(f.FrameNumber))
                                                                .Append(_loadedFrames.Last())
                                                                .Select(f => ConvertThumbnailToViewModel(f.VideoId, f.FrameNumber))
                                                                .ToList();
            AddFramesToVisibleItems(SampledFrames, viewModelsToAdd);

            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            ScrollToFrameHorizontally(newlySelectedFrame);
            ScrollToSampleRow(0);
            BrowsingEvent = EventType.VideoSummary;
            IsBusy = false;
        }

        public async Task LoadSortedDisplay(FrameViewModel selectedFrame, int[] sortedFrameIds)
        {
            IsBusy = true;

            await LoadFramesForIds(sortedFrameIds);

            const int sampledFramesCount = 41;
            List<FrameViewModel> viewModelsToAdd = Enumerable.Range(0, sampledFramesCount)
                                                             .Select(i => _loadedFrames[_loadedFrames.Count / sampledFramesCount * i])
                                                             .Append(_loadedFrames.Last())
                                                             .Select(f => ConvertThumbnailToViewModel(f.VideoId, f.FrameNumber))
                                                             .ToList();
            AddFramesToVisibleItems(SampledFrames, viewModelsToAdd);

            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            ScrollToFrameHorizontally(newlySelectedFrame);
            ScrollToSampleRow(0);
            BrowsingEvent = EventType.JointEmbedding;
            IsBusy = false;
        }

        public event EventHandler Close;

        public void CloseButton()
        {
            Close?.Invoke(this, EventArgs.Empty);
        }

        protected override void BeforeEventAction()
        {
            CloseButton();
        }

        public void OnFrameSelectedSampled(FrameViewModel selectedFrame)
        {
            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            ScrollToFrameHorizontally(newlySelectedFrame);

            _interactionLogger.LogInteraction(EventCategory.Browsing, EventType.VideoSummary, $"{selectedFrame.VideoId}|{selectedFrame.FrameNumber}|SampleLayer");
        }
    }
}
