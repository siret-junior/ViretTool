using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Proxy;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ViretTool.BusinessLayer.Services;

namespace KisQueryResultExtraction
{
    class Program
    {
        static void Main(string[] args)
        {
            // string inputDirectory = args[0];
            // want to use IBiTemporalRankingService here, initialized by inputDirectory

            using (WindsorContainer container = new WindsorContainer(new DefaultKernel(new ArgumentPassingDependencyResolver(), new NotSupportedProxyFactory()), new DefaultComponentInstaller()))
            {
                container.AddFacility<TypedFactoryFacility>();
                container.Install(FromAssembly.This());

                IDatasetServicesManager datasetServiceManager = container.Resolve<IDatasetServicesManager>();
                datasetServiceManager.OpenDataset("...");   //TODO
                //compute result
                //datasetServiceManager.CurrentDataset.RankingService.ComputeRankedResultSet()
            }
        }
    }
}
