using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ViretTool.PresentationLayer.Controls.Common.Sketches
{
    public class SketchColorPoint
    {
        public SketchColorPoint(Point position, Color fillColor, Point ellipseAxis, SketchType sketchType, bool area)
        {
            Position = position;
            FillColor = fillColor;
            EllipseAxis = ellipseAxis;
            SketchType = sketchType;
            Area = area;
        }

        public Point Position { get; }
        public Color FillColor { get; }
        public Point EllipseAxis { get; }
        public SketchType SketchType { get; }
        public bool Area { get; }

        public override string ToString()
        {
            return "[P(" +
                Position.X.ToString("0.00", CultureInfo.InvariantCulture) +
                "; " +
                Position.Y.ToString("0.00", CultureInfo.InvariantCulture) +
                "), " +
                "C(" +
                FillColor.R +
                "; " +
                FillColor.G +
                "; " +
                FillColor.B +
                "), " +
                "E(" +
                EllipseAxis.X.ToString("0.00", CultureInfo.InvariantCulture) +
                "; " +
                EllipseAxis.Y.ToString("0.00", CultureInfo.InvariantCulture) +
                "), " +
                (Area ? "all" : "any") +
                "]";
        }
    }
}
