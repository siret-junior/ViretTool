using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ViretTool.DataLayer.DataIO.InitialDisplay
{
    public class InitialDisplayReader
    {
        private const int RowCountLineNumber = 0;
        private const int ColumnCountLineNumber = 1;
        private const int IdsLineNumber = 2;

        public IEnumerable<int> ReadInitialIds(string filePath)
        {
            return File.ReadLines(filePath).ElementAt(IdsLineNumber).Split(new[] { '\t', ';', ',', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        }

        public int ReadRowCount(string filePath)
        {
            return int.Parse(File.ReadLines(filePath).ElementAt(RowCountLineNumber));
        }

        public int ReadColumnCount(string filePath)
        {
            return int.Parse(File.ReadLines(filePath).ElementAt(ColumnCountLineNumber));
        }
    }
}
