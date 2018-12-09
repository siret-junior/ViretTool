using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.ThumbnailIO;

namespace ViretTool.DataLayer.DataProviders.Thumbnails
{
    public class ThumbnailProvider
    {
        public const string FILE_EXTENSION = ".thumbs";

        public ThumbnailReader FromDirectory(string inputDirectory)
        {
            string[] files = Directory.GetFiles(inputDirectory);
            string inputFile = files.Single(path => path.EndsWith(FILE_EXTENSION));
            return new ThumbnailReader(inputFile);
        }
    }
}
