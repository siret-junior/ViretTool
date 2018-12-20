using System;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public abstract class ScrollableDisplayControlViewModel : DisplayControlViewModelBase
    {
        private int _columnCount;
        private int _rowCount;

        protected ScrollableDisplayControlViewModel(
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

        public Action<int> ScrollToRow { private get; set; }

        protected void ScrollToFrameHorizontally(FrameViewModel frameViewModel)
        {
            if (frameViewModel == null || ColumnCount == 0)
            {
                ScrollToRow(0);
                return;
            }

            int indexOfFrame = _loadedFrames.IndexOf(frameViewModel);
            int rowWithFrame = indexOfFrame / ColumnCount;
            int columnNumberToScroll = Math.Max(0, rowWithFrame - RowCount / 2); //frame should be in the middle
            ScrollToRow(columnNumberToScroll);
        }

        protected void ScrollToFrameVertically(FrameViewModel frameViewModel)
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
