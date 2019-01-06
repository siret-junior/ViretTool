using System;
using System.Windows;
using System.Windows.Threading;
using Castle.Services.Logging.NLogIntegration;
using ILogger = Castle.Core.Logging.ILogger;

namespace ViretTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILogger _logger;

        public App()
        {
            NLogFactory nLogFactory = new NLogFactory(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            _logger = nLogFactory.Create(GetType());
            DispatcherUnhandledException += CurrentDomainOnUnhandledException;
        }

        private void CurrentDomainOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal("Unhandled Exception: ", e.Exception);
            MessageBox.Show(e.Exception.Message, "Fatal error");
            
            e.Handled = true;
            Environment.Exit(-1);
        }
    }
}
