using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Fusion;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.ColorSignatureModel;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNFeatures;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNKeywords;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.Installers
{
    public class BusinessLayerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //factory
            container.Register(
                Component.For<IDatabaseServicesFactory>().AsFactory(),
                Component.For<DatasetServices>().LifestyleTransient());

            //services bound to dataset
            container.Register(
                Component.For<IDatasetServicesManager>().ImplementedBy<DatasetServicesManager>().LifestyleSingleton(),

                //lifestyleBound is very important - current instance is released when data services are no longer used (new dataset is opened)
                Component.For<IThumbnailService<Thumbnail<byte[]>>>().ImplementedBy<JpegThumbnailService>().LifestyleBoundTo<DatasetServices>(),
                Component.For<IDatasetService>().ImplementedBy<DatasetService>().LifestyleBoundTo<DatasetServices>(),
                Component.For<IDescriptorProvider<float[]>>()
                         .UsingFactoryMethod((_, context) => SemanticVectorDescriptorProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
                         .LifestyleBoundTo<DatasetServices>(),
                Component.For<IDescriptorProvider<byte[]>>()
                         .UsingFactoryMethod((_, context) => ColorSignatureDescriptorProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
                         .LifestyleBoundTo<DatasetServices>(),
                Component.For<IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet>>()
                         .ImplementedBy<BiTemporalRankingService>()
                         .LifestyleBoundTo<DatasetServices>());

            //singleton services
            //container.Register(
            //    Component.For<IRankFusion>().ImplementedBy<RankFusionSum>());

            //transient services
            container.Register(
                Component.For<IRankFusion>().ImplementedBy<RankFusionSum>().LifestyleTransient(),
                Component.For<IColorSketchModel<ColorSketchQuery>>().ImplementedBy<ColorSignatureModel>().LifestyleTransient(),
                Component.For<ISemanticExampleModel<SemanticExampleQuery>>().ImplementedBy<FloatVectorModel>().LifestyleTransient(),
                Component.For<IKeywordModel<KeywordQuery>>() //TODO this is both model and provider - data are loaded each time a ranking module is instantiated
                         .UsingFactoryMethod((_, context) => KeywordSubModel.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
                         .LifestyleTransient(),
                Component.For<ISimilarityModule>().ImplementedBy<SimilarityModule>().LifestyleTransient(),
                Component.For<IFilteringModule>().ImplementedBy<FilteringModule>().LifestyleTransient(),
                Component.For<IRankingModule>().ImplementedBy<RankingModule>().LifestyleTransient());
        }
    }
}
