using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.DisplayControl;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class DetailViewModel : ScrollableDisplayControlViewModel
    {
        private bool _isBusy;

        public DetailViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager) : base(logger, datasetServicesManager)
        {
            ColumnCount = 10;
            RowCount = 10;
            DisplayName = Resources.Properties.Resources.DetailWindowTitle;
        }

        public BindableCollection<FrameViewModel> SampledFrames { get; } = new BindableCollection<FrameViewModel>();

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                NotifyOfPropertyChange();
            }
        }

        public override async Task LoadVideoForFrame(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            await base.LoadVideoForFrame(selectedFrame);

            HashSet<int> shotFrames = _datasetServicesManager.CurrentDataset.DatasetService.GetShotFrameNumbersForVideo(selectedFrame.VideoId)
                                                             .Select(t => t.StartFrame)
                                                             .ToHashSet();
            SampledFrames.Clear();
            SampledFrames.AddRange(
                _loadedFrames.Where(f => shotFrames.Contains(f.FrameNumber)).Append(_loadedFrames.Last()).Select(f => ConvertThumbnailToViewModel(f.VideoId, f.FrameNumber)));

            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            ScrollToFrameHorizontally(newlySelectedFrame);
            IsBusy = false;
        }

        public async Task LoadSortedDisplay(FrameViewModel selectedFrame, IList<FrameViewModel> topFrames)
        {
            IsBusy = true;
            FrameViewModel[] sortedFrameIds = await Task.Run(() => GetSortedFrameIds(topFrames));

            _loadedFrames = sortedFrameIds.Select(f => f.Clone()).ToList();
            UpdateVisibleFrames();

            const int sampledFramesCount = 10;
            SampledFrames.Clear();
            SampledFrames.AddRange(
                Enumerable.Range(0, sampledFramesCount)
                          .Select(i => _loadedFrames[_loadedFrames.Count / sampledFramesCount * i])
                          .Append(_loadedFrames.Last())
                          .Select(f => ConvertThumbnailToViewModel(f.VideoId, f.FrameNumber)));

            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            ScrollToFrameHorizontally(newlySelectedFrame);
            IsBusy = false;
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                TryClose(false);
            }
        }

        protected override void BeforeEventAction()
        {
            TryClose(true);
        }

        public void OnFrameSelectedSampled(FrameViewModel selectedFrame)
        {
            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            ScrollToFrameHorizontally(newlySelectedFrame);
        }

        private FrameViewModel[] GetSortedFrameIds(IList<FrameViewModel> topFrames)
        {
            List<float[]> data = topFrames.Select(GetFrameId)
                                          .Where(id => id.HasValue)
                                          .Select(id => _datasetServicesManager.CurrentDataset.SemanticVectorProvider.Descriptors[id.Value])
                                          .ToList();
            int width = ColumnCount;
            int height = data.Count / ColumnCount;
            //we ignore items out of the grid
            int[,] sortedFrames = new GridSorterFast().SortItems(data.Take(width * height).ToList(), width, height);

            FrameViewModel[] result = new FrameViewModel[width * height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i * width + j] = topFrames[sortedFrames[j, i]];
                }
            }

            return result;
        }

        protected override void UpdateVisibleFrames()
        {
            if (VisibleFrames.Count < _loadedFrames.Count)
            {
                VisibleFrames.AddRange(_loadedFrames.Skip(VisibleFrames.Count));
            }
            else if (VisibleFrames.Count > _loadedFrames.Count)
            {
                VisibleFrames.RemoveRange(VisibleFrames.Skip(_loadedFrames.Count).ToList());
            }

            for (int i = 0; i < _loadedFrames.Count; i++)
            {
                VisibleFrames[i] = _loadedFrames[i];
            }
        }
        
    }
}
