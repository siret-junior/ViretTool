using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    public class ThumbnailWriter : ThumbnailIOBase
    {
        private readonly BinaryWriter mWriter;

        // total counts
        protected readonly int FrameCount;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
