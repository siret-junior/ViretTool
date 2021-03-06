using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;
using Caliburn.Micro;
using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Proxy;
using Castle.MicroKernel.Registration;
using Castle.Services.Logging.NLogIntegration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Viret;
//using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.PresentationLayer.Windows.ViewModels;

namespace ViretTool
{
    public class AppBootstrapper : BootstrapperBase
    {
        private WindsorContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            //Probably not needed now
            //InitializeCulture();

            DisplayRootViewFor<MainWindowViewModel>();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            _container.Resolve<ViretCore>().Dispose();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _container.Resolve<ILogger>().Fatal("Unhandled Exception: ", e.Exception);
            MessageBox.Show(e.Exception.Message, "Fatal error");

            e.Handled = true;
            //I'm not sure if it's always possible to recover so we better terminate the app
            Environment.Exit(-1);
        }

        protected override void Configure()
        {
            _container =
              new WindsorContainer(
                new DefaultKernel(new ArgumentPassingDependencyResolver(), new NotSupportedProxyFactory()),
                new DefaultComponentInstaller());

            _container.AddFacility<TypedFactoryFacility>();
            _container.AddFacility<LoggingFacility>(x => x.LogUsing<NLogFactory>().WithAppConfig());

            _container.Register(Component.For<IEventAggregator>().ImplementedBy<EventAggregator>());
            _container.Register(Component.For<IWindowManager>().ImplementedBy<WindowManager>());

            //this tells Caliburn where to look for Views connected to ViewModels
            AssemblySource.Instance.Add(typeof(MainWindowViewModel).Assembly);

            _container.Install(FromAssembly.This());
        }

        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ? _container.Resolve(service) : _container.Resolve(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.ResolveAll(service).Cast<object>();
        }

        //protected override void BuildUp(object instance)
        //{
        //	//castle set all public properties
        //	instance.GetType().GetProperties()
        //	        .Where(property => property.CanWrite && property.PropertyType.IsPublic)
        //	        .Where(property => _container.Kernel.HasComponent(property.PropertyType))
        //	        .ForEach(property => property.SetValue(instance, _container.Resolve(property.PropertyType), null));
        //}

        /// <summary>
        /// For manual culture setup
        /// </summary>
        private void InitializeCulture()
        {
            try
            {
                string culture = "en-US";

                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
                var lang = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(lang));
                FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(TextElement), new FrameworkPropertyMetadata(lang));
            }
            catch (Exception ex)
            {
                var logger = _container.Resolve<ILogger>();
                logger.Fatal("Initialization error", ex);
            }
        }
    }
}
