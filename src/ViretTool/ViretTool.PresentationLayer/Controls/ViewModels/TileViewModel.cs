using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace ViretTool.PresentationLayer.Controls.ViewModels
{
    public class TileViewModel : PropertyChangedBase
    {
        private bool _isSelected;
        private bool _isMouseOver;

        public TileViewModel(BitmapImage imageSource)
        {
            ImageSource = imageSource;
        }


        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        public BitmapImage ImageSource { get; }

        public bool IsMouseOver
        {
            get => _isMouseOver;
            set
            {
                if (_isMouseOver == value)
                {
                    return;
                }

                _isMouseOver = value;
                NotifyOfPropertyChange();
            }
        }

        public void MouseDown(MouseButtonEventArgs e)
        {
        }

        public void MouseMove(MouseEventArgs e)
        {
            IsMouseOver = true;
        }

        public void MouseLeave(MouseEventArgs e)
        {
            IsMouseOver = false;
        }

        public void MouseWheel(MouseWheelEventArgs e)
        {
        }
    }
}
