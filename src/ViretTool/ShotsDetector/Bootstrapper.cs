using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Castle.Core.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Proxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ShotsDetector.PresentationLayer;
using ViretTool;

namespace ShotsDetector
{
    class Bootstrapper : BootstrapperBase
    {
        private WindsorContainer _container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainWindowViewModel>();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
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
            //_container.AddFacility<LoggingFacility>(x => x.LogUsing<NLogFactory>().WithAppConfig());

            _container.Register(Component.For<IEventAggregator>().ImplementedBy<EventAggregator>());
            _container.Register(Component.For<IWindowManager>().ImplementedBy<WindowManager>());

            //this tells Caliburn where to look for Views connected to ViewModels
            AssemblySource.Instance.Add(typeof(ViretTool.PresentationLayer.Windows.ViewModels.MainWindowViewModel).Assembly);

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
    }
}
