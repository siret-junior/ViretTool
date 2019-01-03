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
    }
}
