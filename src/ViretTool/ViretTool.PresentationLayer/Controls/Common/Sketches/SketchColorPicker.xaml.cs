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

namespace ViretTool.PresentationLayer.Controls.Common.Sketches
{
    /// <summary>
    /// Interaction logic for SketchColorPicker.xaml
    /// </summary>
    public partial class SketchColorPicker : Window
    {
        private readonly int mColorButtonWidth = 20;

        public SketchColorPicker()
        {
            InitializeComponent();

            //top B/W colors
            FillColorCanvas(CreateBwBrushes(), 8, BwCanvas);

            //bottom color picker
            SolidColorBrush[] colorBrushes = CreateColorBrushes();
            int nColorsInRow = 20;
            FillColorCanvas(colorBrushes, nColorsInRow, ColorPickerPanel);

            Width = nColorsInRow * mColorButtonWidth + 15;
            Height = (colorBrushes.Length / nColorsInRow) * mColorButtonWidth + 103;
        }

        private void FillColorCanvas(SolidColorBrush[] brushes, int nColorsInRow, Canvas canvas)
        {
            int nColorsInColumn = ((brushes.Length - 1) / nColorsInRow) + 1; // assumes brushes are not empty
            for (int i = 0; i < brushes.Length; i++)
            {
                int nthRow = i / nColorsInRow;
                int nthColumn = i % nColorsInRow;

                double cellBorderLightness = 1 - nthRow / (double)nColorsInColumn;
                cellBorderLightness *= cellBorderLightness;
                Canvas b = CreateColorCellCanvas(brushes[i], cellBorderLightness);
                b.MouseDown += LeftClick;

                canvas.Children.Add(b);
                Canvas.SetLeft(b, nthColumn * mColorButtonWidth);
                Canvas.SetTop(b, 6 + (int)Math.Floor(i / (double)nColorsInRow) * mColorButtonWidth);
            }
        }

        public Color SelectedColor { get; private set; }
        public Image SelectedImage { get; private set; }

        public static Color HSLToRGB(float h, float s, float l)
        {
            float r, g, b;

            if (s == 0f)
            {
                r = g = b = l;
            }
            else
            {
                float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                r = HueToRgb(p, q, h + 1f / 3f);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1f / 3f);
            }

            return Color.FromRgb(Convert.ToByte(r * 255), Convert.ToByte(g * 255), Convert.ToByte(b * 255));
        }

        public bool Show(Point p)
        {
            Left = p.X - 20;
            Top = p.Y - 20;

            return ShowDialog() == true;
        }

        private SolidColorBrush[] CreateColorBrushes()
        {
            List<SolidColorBrush> brushes = new List<SolidColorBrush>();

            for (float lightness = 0.1f; lightness <= 1; lightness += 0.1f)
            {
                for (float hue = 0; hue <= 1; hue += 0.05f)
                {
                    brushes.Add(new SolidColorBrush(HSLToRGB(hue, 1, lightness)));
                }
            }

            return brushes.ToArray();
        }

        private SolidColorBrush[] CreateBwBrushes()
        {
            List<SolidColorBrush> brushes = new List<SolidColorBrush>();

            for (double x = 0; x < 255; x += 17)
            {
                brushes.Add(new SolidColorBrush(Color.FromRgb((byte)x, (byte)x, (byte)x)));
            }

            brushes.Add(new SolidColorBrush(Color.FromRgb(255, 255, 255)));

            return brushes.ToArray();
        }

        private Canvas CreateColorCellCanvas(SolidColorBrush color, double borderLightness)
        {
            Canvas canvas = new Canvas();
            canvas.Width = mColorButtonWidth;
            canvas.Height = canvas.Width;
            canvas.Background = color;

            double minLightness = 0.2;
            double maxLightness = 0.5;
            double offset = minLightness;
            double denormalizer = maxLightness - minLightness;
            borderLightness *= denormalizer;
            borderLightness += offset;

            byte borderGray = (byte)(borderLightness * 255);


            Color borderColor = Color.FromRgb(borderGray, borderGray, borderGray);

            Rectangle rectangle = new Rectangle
                                  {
                                      Width = canvas.Width,
                                      Height = canvas.Height,
                                      Stroke = new SolidColorBrush(borderColor),
                                      StrokeThickness = 0.5,
                                      //Fill = new SolidColorBrush(Colors.Black),
                                  };

            Canvas.SetLeft(rectangle, 0);
            Canvas.SetTop(rectangle, 0);
            canvas.Children.Add(rectangle);

            return canvas;
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0f)
            {
                t += 1f;
            }

            if (t > 1f)
            {
                t -= 1f;
            }

            if (t < 1f / 6f)
            {
                return p + (q - p) * 6f * t;
            }

            if (t < 1f / 2f)
            {
                return q;
            }

            if (t < 2f / 3f)
            {
                return p + (q - p) * (2f / 3f - t) * 6f;
            }

            return p;
        }

        private void LeftClick(object sender, RoutedEventArgs e)
        {
            switch (sender)
            {
                case Canvas canvas:
                    SelectedColor = ((SolidColorBrush)canvas.Background).Color;
                    break;
                case Image image:
                    SelectedImage = image;
                    break;
                default:
                    throw new NotSupportedException("Unknown control");
            }

            DialogResult = true;
            Close();
        }
    }
}
