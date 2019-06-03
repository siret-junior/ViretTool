using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public abstract class ScrollableDisplayControlViewModel : DisplayControlViewModelBase
    {
        protected ScrollableDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger interactionLogger)
            : base(logger, datasetServicesManager, interactionLogger)
        {
        }

        public Action<int> ScrollToColumn { private get; set; }

        protected override void AddFramesToVisibleItems(BindableCollection<FrameViewModel> collectionToUpdate, IList<FrameViewModel> viewModelsToAdd)
        {
            collectionToUpdate.Clear();
            collectionToUpdate.AddRange(viewModelsToAdd);
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
