using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ViretTool.PresentationLayer.Controls.Common
{
    /// <summary>
    /// Interaction logic for SubmitFrameControl.xaml
    /// </summary>
    public partial class SubmitFrameControl : UserControl
    {
        public SubmitFrameControl()
        {
            InitializeComponent();
            DrawRectangle();
        }
        
        public double CanvasHeight
        {
            get { return frameControl.Height; }
        }
        public double CanvasWidth
        {
            get { return frameControl.Width; }
        }
        FrameControl _frameControl
        {
            get
            {
                return frameControl;
            }
            set
            {
                if(value != frameControl)
                {
                    frameControl = value;
                }
            }
        }
        
        public void DrawRectangle()
        {
            Ellipse el = new Ellipse();
            el.Width = 10;
            el.Height = 10;
            el.Fill = new SolidColorBrush(Colors.Red);
            Canvas.SetLeft(el, 10);
            Canvas.SetTop(el, 10);

            mainCanvas.Children.Add(el);

        }
    }
}
