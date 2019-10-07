using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public abstract class KeywordIOBase : IDisposable
    {
        public const string KEYWORD_EXTENSION = ".framesynsets";

        public abstract void Dispose();
    }
}
