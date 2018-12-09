using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ViretTool.DataLayer.DataProviders.Dataset;
using ViretTool.DataLayer.DataProviders.Thumbnails;

namespace ViretTool.Installers
{
    public class DataLayerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //factory
            container.Register(
                Component.For<ThumbnailProvider>().LifestyleSingleton(),
                Component.For<DatasetProvider>().LifestyleSingleton());
        }
    }
}
