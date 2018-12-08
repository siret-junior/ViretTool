using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Controls.DisplayControl.Views;
using ViretTool.PresentationLayer.Controls.Query.ViewModels;
using ViretTool.PresentationLayer.Controls.Query.Views;
using ViretTool.PresentationLayer.ViewModels;
using ViretTool.PresentationLayer.Views;

namespace ViretTool.Installers
{
    public class PresentationLayerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<MainWindowView>(), //default lifestyle is singleton
                Component.For<MainWindowViewModel>(),

                Component.For<QueryView>().LifestyleTransient(),
                Component.For<QueryViewModel>().LifestyleTransient(),
                Component.For<DisplayControlView>().LifestyleTransient(),
                Component.For<DisplayControlViewModel>().LifestyleTransient());
        }
    }
}
