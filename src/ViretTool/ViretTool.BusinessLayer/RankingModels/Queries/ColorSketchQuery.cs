using System.Collections.Generic;
using System.Linq;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class ColorSketchQuery : ISimilarityQuery
    {
        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }
        public Ellipse[] ColorSketchEllipses { get; private set; }


        public ColorSketchQuery(int canvasWidth, int canvasHeight, Ellipse[] colorSketchEllipses)
        {
            CanvasWidth = canvasWidth;
            CanvasHeight = canvasHeight;
            ColorSketchEllipses = colorSketchEllipses;
        }


        public override bool Equals(object obj)
        {
            return obj is ColorSketchQuery query &&
                   CanvasWidth == query.CanvasWidth &&
                   CanvasHeight == query.CanvasHeight &&
                   ColorSketchEllipses.SequenceEqual(query.ColorSketchEllipses);
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
