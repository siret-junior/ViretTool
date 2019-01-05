using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ViretTool.BusinessLayer.OutputGridSorting;
using ViretTool.PresentationLayer.Controls.DisplayControl;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Controls.DisplayControl.Views;
using ViretTool.PresentationLayer.Controls.Query.ViewModels;
using ViretTool.PresentationLayer.Controls.Query.Views;
using ViretTool.PresentationLayer.Helpers;
using ViretTool.PresentationLayer.Windows.ViewModels;
using ViretTool.PresentationLayer.Windows.Views;

namespace ViretTool.Installers
{
    public class PresentationLayerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IGridSorter>().ImplementedBy<GridSorterFast>());

            container.Register(
                Component.For<MainWindowView>(), //default lifestyle is singleton
                Component.For<MainWindowViewModel>(),

                Component.For<QueryBuilder>(),

                Component.For<QueryView>().LifestyleTransient(),
                Component.For<QueryViewModel>().LifestyleTransient(),
                Component.For<PageDisplayControlView>().LifestyleTransient(),
                Component.For<ScrollDisplayControlView>().LifestyleTransient(),
                Component.For<ScrollDisplayControlViewModel>().LifestyleTransient(),
                Component.For<PageDisplayControlViewModel>().LifestyleTransient(),
                Component.For<SubmitControlView>().LifestyleTransient(),
                Component.For<SubmitControlViewModel>().LifestyleTransient(),
                Component.For<DetailView>().LifestyleTransient(),
                Component.For<DetailViewModel>().LifestyleTransient(),
                Component.For<TestControlView>().LifestyleTransient(),
                Component.For<TestControlViewModel>().LifestyleTransient());
        }
    }
}
