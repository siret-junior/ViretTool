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
    public class SomResultDisplayControlViewModel : ZoomDisplayControlViewModel
    {
        //private FrameViewModel _gpsFrame;

        public SomResultDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger iterationLogger)
            : base(logger, datasetServicesManager, iterationLogger)
        {
            datasetServicesManager.DatasetOpened += (_, services) =>
            {
                _zoomDisplayProvider = datasetServicesManager.CurrentDataset.SomGeneratorProvider;
            };
        }


        /// <summary>
        /// Computes SOM structure from given framIDs
        /// </summary>
        /// <param name="inputFrameIds">Frame IDs</param>
        /// <returns></returns>
        public override async Task LoadFramesForIds(IList<int> inputFrameIds)
        {
            // update row and column count before usage
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;


            int[] ids;
            try
            {
                // compute ids
                ids = _zoomDisplayProvider.GetInitialLayer(RowCount, ColumnCount, inputFrameIds, _datasetServicesManager.CurrentDataset.SemanticVectorProvider);
            }
            // if user screen is bigger than top layer then compute smaller layer and resize image height/width 
            catch (ArgumentOutOfRangeException)
            {
                (ids, ColumnCount, RowCount) = _zoomDisplayProvider.GetSmallLayer(0, RowCount, ColumnCount);
                ImageWidth = DisplayWidth / ColumnCount;
                ImageHeight = DisplayHeight / RowCount;
            }
            if (ids != null)
            {
                _currentLayer = 0;
                   
                _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());

                UpdateBorderColors();
            }
            else
            {
                await RandomGridDisplay();
                _currentLayer = 0;
            }
            IsInitialDisplayShown = false;

           
            this.NotifyOfPropertyChange("ShowZoomOutButton");
            this.NotifyOfPropertyChange("ShowZoomIntoButton");

            UpdateVisibleFrames();
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

    }
}
