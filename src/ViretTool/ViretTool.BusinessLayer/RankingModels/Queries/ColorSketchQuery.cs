using System.Collections.Generic;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class ColorSketchQuery
    {
        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }
        public Ellipse[] ColorSketchEllipses { get; private set; }
        public bool UseForSorting { get; private set; }
        public bool UseForFiltering { get; private set; }


        public ColorSketchQuery(int canvasWidth, int canvasHeight, Ellipse[] colorSketchEllipses, 
            bool useForSorting = true, bool useForFiltering = false)
        {
            CanvasWidth = canvasWidth;
            CanvasHeight = canvasHeight;
            ColorSketchEllipses = colorSketchEllipses;
            UseForSorting = useForSorting;
            UseForFiltering = useForFiltering;
        }


        public override bool Equals(object obj)
        {
            return obj is ColorSketchQuery query &&
                   CanvasWidth == query.CanvasWidth &&
                   CanvasHeight == query.CanvasHeight &&
                   ColorSketchEllipses.Equals(query.ColorSketchEllipses);
        }

        public override int GetHashCode()
        {
            int hashCode = 1496491996;
            hashCode = hashCode * -1521134295 + CanvasWidth.GetHashCode();
            hashCode = hashCode * -1521134295 + CanvasHeight.GetHashCode();
            hashCode = hashCode * -1521134295 + ColorSketchEllipses.GetHashCode();
            return hashCode;
        }
    }
}
