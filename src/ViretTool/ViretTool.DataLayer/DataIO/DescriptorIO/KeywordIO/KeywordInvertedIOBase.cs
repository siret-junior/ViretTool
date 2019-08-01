using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public abstract class KeywordInvertedIOBase : IDisposable
    {
        public const string KEYWORD_EXTENSION = ".keyword-inv";
        
        public abstract void Dispose();
    }
}
