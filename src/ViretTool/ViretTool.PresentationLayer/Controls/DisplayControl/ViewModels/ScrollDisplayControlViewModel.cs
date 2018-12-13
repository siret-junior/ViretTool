using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class ScrollDisplayControlViewModel : DisplayControlViewModelBase
    {
        private int _rowCount;
        private int _columnCount;

        public ScrollDisplayControlViewModel(IDatasetServicesManager datasetServicesManager) : base(datasetServicesManager)
        {
        }

        public int RowCount
        {
            get => _rowCount;
            set
            {
                if (_rowCount == value)
                {
                    return;
                }

                _rowCount = value;
                NotifyOfPropertyChange();
            }
        }

        public int ColumnCount
        {
            get => _columnCount;
            set
            {
                if (_columnCount == value)
                {
                    return;
                }

                _columnCount = value;
                NotifyOfPropertyChange();
            }
        }

        public Action<int> ScrollToColumn { private get; set; }

        public override async Task LoadVideoForFrame(FrameViewModel frameViewModel)
        {
            await base.LoadVideoForFrame(frameViewModel);

            int indexOfFrame = _loadedFrames.FindIndex(f => f.FrameNumber == frameViewModel.FrameNumber);
            int columnWithFrame = indexOfFrame / RowCount;
            int columnNumberToScroll = Math.Max(0, columnWithFrame - ColumnCount / 2); //frame should be in the middle
            ScrollToColumn(columnNumberToScroll);
        }

        protected override void UpdateVisibleFrames()
        {
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            VisibleFrames.Clear();
            VisibleFrames.AddRange(_loadedFrames);
        }
    }
}
