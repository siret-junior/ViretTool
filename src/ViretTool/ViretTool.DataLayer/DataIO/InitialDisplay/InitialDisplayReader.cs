using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ViretTool.DataLayer.DataIO.InitialDisplay
{
    /// <summary>
    /// Reads keyframes for initial display that is designed for easy query example selection.
    /// </summary>
    public class InitialDisplayReader
    {
        private const int ROW_COUNT_LINE_NUMBER = 0;
        private const int COLUMN_COUNT_LINE_NUMBER = 1;
        private const int FRAME_IDS_LINE_NUMBER = 2;

        public IEnumerable<int> ReadInitialIds(string filePath)
        {
            return File.ReadLines(filePath)
                .ElementAt(FRAME_IDS_LINE_NUMBER)
                .Split(new[] { '\t', ';', ',', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse);
        }

        public int ReadRowCount(string filePath)
        {
            return int.Parse(File.ReadLines(filePath).ElementAt(ROW_COUNT_LINE_NUMBER));
        }

        public int ReadColumnCount(string filePath)
        {
            return int.Parse(File.ReadLines(filePath).ElementAt(COLUMN_COUNT_LINE_NUMBER));
        }
    }
}
