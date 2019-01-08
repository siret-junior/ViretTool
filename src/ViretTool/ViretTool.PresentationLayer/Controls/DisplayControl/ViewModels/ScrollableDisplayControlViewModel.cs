using System;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public abstract class ScrollableDisplayControlViewModel : DisplayControlViewModelBase
    {
        protected readonly IInteractionLogger _interactionLogger;
        private int _columnCount;
        private int _rowCount;

        protected ScrollableDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger interactionLogger)
            : base(logger, datasetServicesManager)
        {
            _interactionLogger = interactionLogger;
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

        public void OnGridScrollChanged(LogType logType, string detailDescription = null)
        {
            _interactionLogger.LogInteraction(LogCategory.Browsing, logType, "ScrollChanged", detailDescription);
        }

        protected void ScrollToFrameHorizontally(FrameViewModel frameViewModel)
        {
            if (ScrollToRow == null)
            {
                return;
            }

            if (frameViewModel == null || ColumnCount == 0)
            {
                ScrollToRow(0);
                return;
            }

            int indexOfFrame = _loadedFrames.IndexOf(frameViewModel);
            int rowWithFrame = indexOfFrame / ColumnCount;
            int columnNumberToScroll = Math.Max(0, rowWithFrame - RowCount / 2); //frame should be in the middle
            ScrollToRow.Invoke(columnNumberToScroll);
        }

        protected void ScrollToFrameVertically(FrameViewModel frameViewModel)
        {
            if (ScrollToColumn == null)
            {
                return;
            }

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
