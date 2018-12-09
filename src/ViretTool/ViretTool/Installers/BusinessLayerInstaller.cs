using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.Thumbnails;
using ViretTool.DataLayer.DataIO.ThumbnailIO;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Thumbnails;

namespace ViretTool.Installers
{
    public class BusinessLayerInstaller :IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IBiTemporalRankingService<Query, RankedFrame[], TemporalQuery, TemporalRankedFrame[]>>().ImplementedBy<BiTemporalRankingService>());
        }
    }


    public class BusinessLayerInstallerManual
    {
        public Dataset Dataset { get; private set; }
        public JpegThumbnailService ThumbnailService { get; private set; }

        public void Install(string dataDirectory)
        {
            LoadDataset(dataDirectory);
            LoadThumbnailService(dataDirectory);

            LoadDescriptorService(dataDirectory);

            LoadRankingService(dataDirectory);

            LoadSubmissionService(dataDirectory);


        }

        private void LoadDataset(string dataDirectory)
        {
            Dataset = DatasetProvider.FromDirectory(dataDirectory);
        }

        private void LoadThumbnailService(string dataDirectory)
        {
            ThumbnailReader thumbnailReader = ThumbnailProvider.FromDirectory(dataDirectory);
            ThumbnailService = new JpegThumbnailService(thumbnailReader);
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
