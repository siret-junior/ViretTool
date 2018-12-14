using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.Installers
{
    public class BusinessLayerInstaller :IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //factory
            container.Register(
                Component.For<IDatabaseServicesFactory>().AsFactory(),
                Component.For<DatasetServices>().LifestyleTransient());

            container.Register(
                Component.For<IDatasetServicesManager>().ImplementedBy<DatasetServicesManager>().LifestyleSingleton(),

                //lifestyleBound is very important - current instance is released when data services are no longer used (new dataset is opened)
                Component.For<IThumbnailService<Thumbnail<byte[]>>>().ImplementedBy<JpegThumbnailService>().LifestyleBoundTo<DatasetServices>(),
                Component.For<IDatasetService>().ImplementedBy<DatasetService>().LifestyleBoundTo<DatasetServices>(),

                Component.For<IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet>>().ImplementedBy<BiTemporalRankingService>());
        }
    }
}
