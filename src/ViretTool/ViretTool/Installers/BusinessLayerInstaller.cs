//#define NO_FACES
//#define NO_TEXT
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Viret;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.Installers
{
    public class BusinessLayerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                    Component.For<ViretCore>().ImplementedBy<ViretCore>().LifestyleSingleton());

            // factory
            container.Register(
                Component.For<IDatabaseServicesFactory>().AsFactory(),
                Component.For<DatasetServices>().LifestyleTransient());

            container.Register(
                Component.For<IDatasetServicesManager>().ImplementedBy<DatasetServicesManager>().LifestyleSingleton());


//            // factory
//            container.Register(
//                Component.For<IDatabaseServicesFactory>().AsFactory(),
//                Component.For<DatasetServices>().LifestyleTransient());

//            // services bound to dataset
//            container.Register(
//                Component.For<IDatasetServicesManager>().ImplementedBy<DatasetServicesManager>().LifestyleSingleton(),

//                //lifestyleBound is very important - current instance is released when data services are no longer used (new dataset is opened)
//                Component.For<IThumbnailService<Thumbnail<byte[]>>>().ImplementedBy<JpegThumbnailService>().LifestyleBoundTo<DatasetServices>().IsDefault(),
//                Component.For<IThumbnailService<Thumbnail<byte[]>>>().ImplementedBy<CachedThumbnailService<Thumbnail<byte[]>>>().LifestyleBoundTo<DatasetServices>(),
//                Component.For<IDatasetService>().ImplementedBy<DatasetService>().LifestyleBoundTo<DatasetServices>(),
//                Component.For<IDescriptorProvider<float[]>>()
//                         .UsingFactoryMethod((_, context) => SemanticVectorDescriptorProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),
//                Component.For<IColorSignatureDescriptorProvider>()
//                         .UsingFactoryMethod((_, context) => ColorSignatureDescriptorProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),
//                Component.For<IDescriptorProvider<(int synsetId, float probability)[]>>()
//                         .UsingFactoryMethod((_, context) => KeywordDescriptorFactory.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),
//                Component.For<IKeywordLabelProvider<string>>()
//                         .UsingFactoryMethod((_, context) => KeywordLabelFactory.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),
//                Component.For<IKeywordTopScoringProvider>()
//                         .UsingFactoryMethod((_, context) => KeywordTopScoringFactory.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),
//                Component.For<IW2VVQueryToVectorProvider>()
//                         .UsingFactoryMethod((_, context) => W2VVQueryToVectorProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),
//                Component.For<ITranscriptProvider>()
//                         .UsingFactoryMethod((_, context) => TranscriptProvider.FromDirectory((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),



//                Component.For<ILifelogDescriptorProvider>().ImplementedBy<LifelogDescriptorProvider>().LifestyleBoundTo<DatasetServices>(),
//                Component.For<ILifelogFilter>().ImplementedBy<LifelogFilter>().LifestyleBoundTo<DatasetServices>(),
//                Component.For<IInitialDisplayProvider>().ImplementedBy<InitialDisplayProvider>().LifestyleBoundTo<DatasetServices>(),
//                Component.For<IZoomDisplayProvider>().ImplementedBy<ZoomDisplayProvider>().LifestyleBoundTo<DatasetServices>(),
//                Component.For<SomGeneratorProvider>().ImplementedBy<SomGeneratorProvider>().LifestyleBoundTo<DatasetServices>(),

//                Component.For<IFaceSignatureDescriptorProvider>()
//                         .UsingFactoryMethod((_, context) => BoolSignatureDescriptorProvider
//                         .FromDirectory((string)context.AdditionalArguments["datasetDirectory"], ".faces"))
//                         .LifestyleBoundTo<DatasetServices>(),
//                Component.For<ITextSignatureDescriptorProvider>()
//                         .UsingFactoryMethod((_, context) => BoolSignatureDescriptorProvider
//                         .FromDirectory((string)context.AdditionalArguments["datasetDirectory"], ".text"))
//                         .LifestyleBoundTo<DatasetServices>(),

//                //dataset parameters
//                Component.For<IDatasetParameters>().ImplementedBy<DatasetParameters>()
//                         //.UsingFactoryMethod((_, context) => new DatasetParameters((string)context.AdditionalArguments["datasetDirectory"]))
//                         .LifestyleBoundTo<DatasetServices>(),

//                Component.For<IBiTemporalRankingService>()
//                         .ImplementedBy<BiTemporalRankingService>()
//                         .LifestyleBoundTo<DatasetServices>()
//            );
            

//            //singleton services
//            container.Register(
//                // TODO: set submission service dynamically (VBS/LSC) when dataset is opened
//                Component.For<ISubmissionService>().ImplementedBy<SubmissionServiceLifelog>(),
//                Component.For<IInteractionLogger>().ImplementedBy<InteractionLogger>(),
//                Component.For<IResultLogger>().ImplementedBy<ResultLogger>(),
//                Component.For<ITaskLogger>().ImplementedBy<TaskLogger>(),
//                //Component.For<NasNetScorer>()
//                //         .Instance(
//                //             new NasNetScorer(
//                //                 "Data\\NasNetMobile-retrained.pb",
//                //                 "Data\\VBS2019_NasNetMobile-128PCA.pca_components",
//                //                 "Data\\VBS2019_NasNetMobile-128PCA.pca_mean")),
//                Component.For<ExternalImageProvider>(),
//                Component.For<IQueryPersistingService>().ImplementedBy<QueryPersistingService>());


//            //transient services
//            container.Register(
//                //Component.For<IRankFusion>().ImplementedBy<RankFusionSum>().LifestyleTransient(),

//                // similarity used by IBiTemporalSimilarityModel
//                Component.For<IKeywordModel>().ImplementedBy<KeywordModel>().LifestyleTransient(),
//                Component.For<IColorSketchModel>().ImplementedBy<ColorSignatureModel>().LifestyleTransient(),
//                // FaceSketchModel disabled for LSC as it is not very useful there
//                // TODO: ignore missing data transparently
//#if NO_FACES
//                Component.For<IFaceSketchModel>().ImplementedBy<FaceSketchModelSkeleton>().LifestyleTransient(),
//#else
//                Component.For<IFaceSketchModel>().ImplementedBy<FaceSketchModel>().LifestyleTransient(),
//#endif

//#if NO_TEXT
//                Component.For<ITextSketchModel>().ImplementedBy<TextSketchModelSkeleton>().LifestyleTransient(),
//#else
//                Component.For<ITextSketchModel>().ImplementedBy<TextSketchModel>().LifestyleTransient(),
//#endif
//                Component.For<ISemanticExampleModel>().ImplementedBy<FloatVectorModel>().LifestyleTransient(),



//                // fusion used by IBiTemporalSimilarityModel
//                Component.For<IBiTemporalRankFusionSum>()
//                    .ImplementedBy<BiTemporalRankFusionSum>().LifestyleTransient(),
//                Component.For<IBiTemporalRankFusionProduct>()
//                    .ImplementedBy<BiTemporalRankFusionProduct>().LifestyleTransient(),
//                Component.For<IBiTemporalRankFusionFilters>()
//                    .ImplementedBy<BiTemporalRankFusionFilters>().LifestyleTransient(),

//                // used by IBiTemporalSimilarityModule
//                Component.For<IBiTemporalSimilarityModel<
//                    KeywordQuery, IKeywordModel, IBiTemporalRankFusionProduct>>()
//                    .ImplementedBy<BiTemporalSimilarityModel<KeywordQuery, IKeywordModel, IBiTemporalRankFusionProduct>>()
//                    .LifestyleTransient(),
//                Component.For<IBiTemporalSimilarityModel<
//                    ColorSketchQuery, IColorSketchModel, IBiTemporalRankFusionSum>>()
//                    .ImplementedBy<BiTemporalSimilarityModel<ColorSketchQuery, IColorSketchModel, IBiTemporalRankFusionSum>>()
//                    .LifestyleTransient(),
//                Component.For<IBiTemporalSimilarityModel<
//                    ColorSketchQuery, IFaceSketchModel, IBiTemporalRankFusionFilters>>()
//                    .ImplementedBy<BiTemporalSimilarityModel<ColorSketchQuery, IFaceSketchModel, IBiTemporalRankFusionFilters>>()
//                    .LifestyleTransient(),
//                Component.For<IBiTemporalSimilarityModel<
//                    ColorSketchQuery, ITextSketchModel, IBiTemporalRankFusionFilters>>()
//                    .ImplementedBy<BiTemporalSimilarityModel<ColorSketchQuery, ITextSketchModel, IBiTemporalRankFusionFilters>>()
//                    .LifestyleTransient(),
//                Component.For<IBiTemporalSimilarityModel<
//                    SemanticExampleQuery, ISemanticExampleModel, IBiTemporalRankFusionSum>>()
//                    .ImplementedBy<BiTemporalSimilarityModel<SemanticExampleQuery, ISemanticExampleModel, IBiTemporalRankFusionSum>>()
//                    .LifestyleTransient(),

//                // used by IFusionModule
//                Component.For<IRankFilteringModule>().ImplementedBy<RankFilteringModule>()
//                .LifestyleTransient(),

//                // used by IFilteringModule
//                Component.For<IColorSaturationFilter>()
//                    .UsingFactoryMethod((_, context) => ThresholdFilter.FromDirectory(
//                        (string)context.AdditionalArguments["datasetDirectory"], ".max_color_delta"))
//                    .LifestyleTransient(),
//                Component.For<IPercentOfBlackFilter>()
//                    .UsingFactoryMethod((_, context) => ThresholdFilter.FromDirectory(
//                        (string)context.AdditionalArguments["datasetDirectory"], ".black_pixel_percentage"))
//                    .LifestyleTransient(),
//                Component.For<ICountRestrictionFilter>()
//                    .ImplementedBy<CountRestrictionFilter>().LifestyleTransient(),
//                Component.For<ITranscriptFilter>()
//                    .ImplementedBy<TranscriptFilter>().LifestyleTransient(),

//                // used by IBiTemporalRankingModule
//                Component.For<IBiTemporalSimilarityModule>().ImplementedBy<BiTemporalSimilarityModule>().LifestyleTransient(),
//                Component.For<IFusionModule>().ImplementedBy<FusionModule>().LifestyleTransient(),
//                Component.For<IFilteringModule>().ImplementedBy<FilteringModule>().LifestyleTransient(),

//                //Component.For<IRankingModule>().ImplementedBy<RankingModule>().LifestyleTransient(),
//                Component.For<IBiTemporalRankingModule>().ImplementedBy<BiTemporalRankingModule>().LifestyleTransient()
//            );
        }
    }
}
