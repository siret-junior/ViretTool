using ViretTool.BusinessLayer.Services;

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

        protected override void UpdateVisibleFrames()
        {
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            VisibleFrames.Clear();
            VisibleFrames.AddRange(_loadedFrames);
        }
    }
}
