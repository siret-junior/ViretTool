using System.Collections.Generic;

namespace ViretTool.PresentationLayer.Controls.Common.Sketches
{
    public class SketchQueryResult
    {
        public SketchQueryResult(IReadOnlyList<SketchColorPoint> sketchColorPoints)
        {
            SketchColorPoints = sketchColorPoints;
        }

        public IReadOnlyList<SketchColorPoint> SketchColorPoints { get; }
    }
}
