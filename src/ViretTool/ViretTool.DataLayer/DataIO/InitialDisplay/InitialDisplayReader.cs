using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ViretTool.DataLayer.DataIO.InitialDisplay
{
    public class InitialDisplayReader
    {
        public IEnumerable<int> ReadInitialIds(string filePath)
        {
            return File.ReadAllText(filePath).Split(new[] { '\t', ';', ',', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        }
    }
}
