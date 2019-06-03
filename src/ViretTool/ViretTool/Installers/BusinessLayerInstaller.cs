using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.ExternalDescriptors;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Fusion;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.ColorSignatureModel;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNFeatures;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNKeywords;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Fusion;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Submission;
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
                Component.For<IThumbnailService<Thumbnail<byte[]>>>().ImplementedBy<JpegThumbnailService>().LifestyleBoundTo<DatasetServices>().IsDefault(),
                Component.For<IThumbnailService<Thumbnail<byte[]>>>().ImplementedBy<CachedThumbnailService<Thumbnail<byte[]>>>().LifestyleBoundTo<DatasetServices>(),
                Component.For<IDatasetService>().ImplementedBy<DatasetService>().LifestyleBoundTo<DatasetServices>(),
                Component.For<IDescriptorProvider<float[]>>()
                         .UsingFactoryMethod((_, context) => SemanticVectorDescriptorProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
                         .LifestyleBoundTo<DatasetServices>(),
                Component.For<IDescriptorProvider<byte[]>>()
                         .UsingFactoryMethod((_, context) => ColorSignatureDescriptorProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
                         .LifestyleBoundTo<DatasetServices>(),

                Component.For<IDescriptorProvider<LifelogFrameMetadata>>().ImplementedBy<LifelogDescriptorProvider>().LifestyleBoundTo<DatasetServices>(),
                Component.For<ILifelogFilter>().ImplementedBy<LifelogFilter>().LifestyleBoundTo<DatasetServices>(),
                Component.For<IInitialDisplayProvider>().ImplementedBy<InitialDisplayProvider>().LifestyleBoundTo<DatasetServices>(),

                Component.For<IFaceSignatureDescriptorProvider>()
                         .UsingFactoryMethod((_, context) => BoolSignatureDescriptorProvider
                         .FromDirectory((string)context.AdditionalArguments["datasetDirectory"], ".faces"))
                         .LifestyleBoundTo<DatasetServices>(),
                Component.For<ITextSignatureDescriptorProvider>()
                         .UsingFactoryMethod((_, context) => BoolSignatureDescriptorProvider
                         .FromDirectory((string)context.AdditionalArguments["datasetDirectory"], ".text"))
                         .LifestyleBoundTo<DatasetServices>(),

                //dataset parameters
                Component.For<IDatasetParameters>().ImplementedBy<DatasetParameters>()
                         //.UsingFactoryMethod((_, context) => new DatasetParameters((string)context.AdditionalArguments["datasetDirectory"]))
                         .LifestyleBoundTo<DatasetServices>(),

                Component.For<IBiTemporalRankingService>()
                         .ImplementedBy<BiTemporalRankingService>()
                         .LifestyleBoundTo<DatasetServices>()
            );
                

            //singleton services
            container.Register(
                Component.For<ISubmissionService>().ImplementedBy<SubmissionService>(),
                Component.For<IInteractionLogger>().ImplementedBy<InteractionLogger>(),
                Component.For<NasNetScorer>()
                         .Instance(
                             new NasNetScorer(
                                 "Data\\NasNetMobile-retrained.pb",
                                 "Data\\VBS2019_NasNetMobile-128PCA.pca_components",
                                 "Data\\VBS2019_NasNetMobile-128PCA.pca_mean")),
                Component.For<ExternalImageProvider>(),
                Component.For<IQueryPersistingService>().ImplementedBy<QueryPersistingService>());

            //transient services
            container.Register(
                //Component.For<IRankFusion>().ImplementedBy<RankFusionSum>().LifestyleTransient(),

                // similarity used by IBiTemporalSimilarityModel
                Component.For<IKeywordModel>() //TODO this is both model and provider - data are loaded each time a ranking module is instantiated
                         .UsingFactoryMethod((_, context) => KeywordSubModel.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
                         .LifestyleTransient(),
                Component.For<IColorSketchModel>().ImplementedBy<ColorSignatureModel>().LifestyleTransient(),
                //Component.For<IFaceSketchModel>().ImplementedBy<FaceSketchModel>().LifestyleTransient(),
                Component.For<IFaceSketchModel>().ImplementedBy<FaceSketchModelSkeleton>().LifestyleTransient(),
                Component.For<ITextSketchModel>().ImplementedBy<TextSketchModel>().LifestyleTransient(),
                Component.For<ISemanticExampleModel>().ImplementedBy<FloatVectorModel>().LifestyleTransient(),

                // fusion used by IBiTemporalSimilarityModel
                Component.For<IBiTemporalRankFusionSum>()
                    .ImplementedBy<BiTemporalRankFusionSum>().LifestyleTransient(),
                Component.For<IBiTemporalRankFusionProduct>()
                    .ImplementedBy<BiTemporalRankFusionProduct>().LifestyleTransient(),
                Component.For<IBiTemporalRankFusionFilters>()
                    .ImplementedBy<BiTemporalRankFusionFilters>().LifestyleTransient(),

                // used by IBiTemporalSimilarityModule
                Component.For<IBiTemporalSimilarityModel<
                    KeywordQuery, IKeywordModel, IBiTemporalRankFusionProduct>>()
                    .ImplementedBy<BiTemporalSimilarityModel<KeywordQuery, IKeywordModel, IBiTemporalRankFusionProduct>>()
                    .LifestyleTransient(),
                Component.For<IBiTemporalSimilarityModel<
                    ColorSketchQuery, IColorSketchModel, IBiTemporalRankFusionSum>>()
                    .ImplementedBy<BiTemporalSimilarityModel<ColorSketchQuery, IColorSketchModel, IBiTemporalRankFusionSum>>()
                    .LifestyleTransient(),
                Component.For<IBiTemporalSimilarityModel<
                    ColorSketchQuery, IFaceSketchModel, IBiTemporalRankFusionFilters>>()
                    .ImplementedBy<BiTemporalSimilarityModel<ColorSketchQuery, IFaceSketchModel, IBiTemporalRankFusionFilters>>()
                    .LifestyleTransient(),
                Component.For<IBiTemporalSimilarityModel<
                    ColorSketchQuery, ITextSketchModel, IBiTemporalRankFusionFilters>>()
                    .ImplementedBy<BiTemporalSimilarityModel<ColorSketchQuery, ITextSketchModel, IBiTemporalRankFusionFilters>>()
                    .LifestyleTransient(),
                Component.For<IBiTemporalSimilarityModel<
                    SemanticExampleQuery, ISemanticExampleModel, IBiTemporalRankFusionSum>>()
                    .ImplementedBy<BiTemporalSimilarityModel<SemanticExampleQuery, ISemanticExampleModel, IBiTemporalRankFusionSum>>()
                    .LifestyleTransient(),

                // used by IFusionModule
                Component.For<IRankFilteringModule>().ImplementedBy<RankFilteringModule>()
                .LifestyleTransient(),

                // used by IFilteringModule
                Component.For<IColorSaturationFilter>()
                    .UsingFactoryMethod((_, context) => ThresholdFilter.FromDirectory(
                        (string)context.AdditionalArguments["datasetDirectory"], ".bwfilter"))
                    .LifestyleTransient(),
                Component.For<IPercentOfBlackFilter>()
                    .UsingFactoryMethod((_, context) => ThresholdFilter.FromDirectory(
                        (string)context.AdditionalArguments["datasetDirectory"], ".pbcfilter"))
                    .LifestyleTransient(),
                Component.For<ICountRestrictionFilter>()
                    .ImplementedBy<CountRestrictionFilter>().LifestyleTransient(),

                // used by IBiTemporalRankingModule
                Component.For<IBiTemporalSimilarityModule>().ImplementedBy<BiTemporalSimilarityModule>().LifestyleTransient(),
                Component.For<IFusionModule>().ImplementedBy<FusionModule>().LifestyleTransient(),
                Component.For<IFilteringModule>().ImplementedBy<FilteringModule>().LifestyleTransient(),

                //Component.For<IRankingModule>().ImplementedBy<RankingModule>().LifestyleTransient(),
                Component.For<IBiTemporalRankingModule>().ImplementedBy<BiTemporalRankingModule>().LifestyleTransient()
            );
        }
    }
}
