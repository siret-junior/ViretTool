using System.Collections.Generic;

namespace ViretTool.PresentationLayer.Controls.Common.Sketches
{
    public class SketchQueryResult
    {
        public SketchQueryResult(IReadOnlyList<SketchColorPoint> sketchColorPoints, SketchType[] changedSketchTypes)
        {
            SketchColorPoints = sketchColorPoints;
            ChangedSketchTypes = changedSketchTypes;
        }

        public IReadOnlyList<SketchColorPoint> SketchColorPoints { get; }
        public SketchType[] ChangedSketchTypes { get; }
    }
}
