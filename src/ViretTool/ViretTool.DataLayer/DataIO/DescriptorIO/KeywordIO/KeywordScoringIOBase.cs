using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public abstract class KeywordScoringIOBase : IDisposable
    {
        public const string KEYWORD_SCORING_EXTENSION = ".kwscoring";

        public abstract void Dispose();
    }
}
