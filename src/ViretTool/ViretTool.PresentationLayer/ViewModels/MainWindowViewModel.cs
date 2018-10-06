using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Castle.Core.Logging;

namespace ViretTool.PresentationLayer.ViewModels
{
	public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive
	{
		private readonly ILogger _logger;

		public MainWindowViewModel(ILogger logger)
		{
			_logger = logger;
			
		}

		protected override void OnActivate()
		{
			_logger.Debug("Main window activated");
		}
	}
}
