using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ShotsDetector.PresentationLayer;

namespace ShotsDetector.Installers
{
    public class PresentationLayerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<MainWindowView>(), //default lifestyle is singleton
                Component.For<MainWindowViewModel>());
        }
    }
}
