using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret
{
    public class Config
    {
        public string DresServer { get; set; } = "https://vbs.itec.aau.at:9443";
        public string SessionId { get; set; } = "";

        public int VideoSegmentLength { get; set; } = 10;
        public int SegmentsInResultDisplay { get; set; } = 200;
        public int FramesInSimilarWindow { get; set; } = 1000;
        public int DetailWindowColumns { get; set; } = 9;
        public int DetailWindowRows { get; set; } = 8;

        public int PresentationFilterMaxFromVideo { get; set; } = 3;
        public int PresentationFilterMaxFromShot { get; set; } = 1;


    }
}
