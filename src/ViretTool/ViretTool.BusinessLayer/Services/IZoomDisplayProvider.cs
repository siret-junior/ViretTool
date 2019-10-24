using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.ZoomDisplayIO;

namespace ViretTool.BusinessLayer.Services
{
    public interface IZoomDisplayProvider
    {
        // TODO
    }

    public class ZoomDisplayProvider : IZoomDisplayProvider
    {
        private readonly string ZOOM_DISPLAY_FILENAME = "zoomDisplay.txt";

        public ZoomDisplayProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            string filePath = Path.Combine(datasetDirectory, ZOOM_DISPLAY_FILENAME);

            // TODO
            // ZoomDisplayReader zoomDisplayReader = new ...
        }

        // TODO
    }
}
