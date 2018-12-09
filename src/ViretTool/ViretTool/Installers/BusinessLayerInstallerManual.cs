using System;
using ViretTool.BusinessLayer.Thumbnails;
using ViretTool.DataLayer.DataIO.ThumbnailIO;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;
using ViretTool.DataLayer.DataProviders.Thumbnails;

namespace ViretTool.Installers
{
    public class BusinessLayerInstallerManual
    {
        public Dataset Dataset { get; private set; }
        public JpegThumbnailService ThumbnailService { get; private set; }

        public void Install(string dataDirectory)
        {
            LoadDataset(dataDirectory);
            LoadThumbnailService(dataDirectory);



            // TODO
            LoadDescriptorService(dataDirectory);

            LoadRankingService(dataDirectory);

            LoadSubmissionService(dataDirectory);


        }

        private void LoadDataset(string dataDirectory)
        {
            Dataset = new DatasetProvider().FromDirectory(dataDirectory);
        }

        private void LoadThumbnailService(string dataDirectory)
        {
            ThumbnailService = new JpegThumbnailService(dataDirectory);
        }

        private void LoadDescriptorService(string dataDirectory)
        {
            throw new NotImplementedException();
        }

        private void LoadRankingService(string dataDirectory)
        {
            throw new NotImplementedException();
        }

        private void LoadSubmissionService(string dataDirectory)
        {
            throw new NotImplementedException();
        }
    }
}
