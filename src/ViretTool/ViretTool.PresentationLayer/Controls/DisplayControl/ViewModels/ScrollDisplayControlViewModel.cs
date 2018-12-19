using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Windows.ViewModels;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class ScrollDisplayControlViewModel : DisplayControlViewModelBase
    {
        private int _columnCount;
        private bool _isBusy;
        private int _rowCount;

        public ScrollDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager)
            : base(logger, datasetServicesManager)
        {
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

        public Action<int> ScrollToColumn { private get; set; }

        

        public override async Task LoadVideoForFrame(FrameViewModel selectedFrame)
        {
            await base.LoadVideoForFrame(selectedFrame);

            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            ScrollToFrame(newlySelectedFrame);
        }

        protected override void UpdateVisibleFrames()
        {
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            VisibleFrames.Clear();
            VisibleFrames.AddRange(_loadedFrames);
        }

        private void ScrollToFrame(FrameViewModel frameViewModel)
        {
            if (frameViewModel == null || RowCount == 0)
            {
                ScrollToColumn(0);
                return;
            }

            int indexOfFrame = _loadedFrames.IndexOf(frameViewModel);
            int columnWithFrame = indexOfFrame / RowCount;
            int columnNumberToScroll = Math.Max(0, columnWithFrame - ColumnCount / 2); //frame should be in the middle
            ScrollToColumn(columnNumberToScroll);
        }
    }
}
