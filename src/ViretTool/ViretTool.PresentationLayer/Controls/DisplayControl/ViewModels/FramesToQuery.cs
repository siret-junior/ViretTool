using System.Collections.Generic;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class FramesToQuery
    {
        public FramesToQuery(IList<FrameViewModel> frames, bool addToFirst, bool supressResultChanges)
        {
            Frames = frames;
            AddToFirst = addToFirst;
            SupressResultChanges = supressResultChanges;
        }

        public IList<FrameViewModel> Frames { get; }
        public bool AddToFirst { get; }
        public bool SupressResultChanges { get; }
    }
}
