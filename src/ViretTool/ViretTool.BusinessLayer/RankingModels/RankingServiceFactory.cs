using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Fusion;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.ColorSignatureModel;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNFeatures;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNKeywords;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.Services;
using ViretTool.DataLayer.DataIO.FilterIO;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ViretTool.BusinessLayer.RankingModels
{
    //TODO not needed - will be provided by castle

    public class RankingServiceFactory
    {
        public static IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet>
            //Build(IDatasetServicesManager datasetServicesManager)
            Build(string directory)
        {
            // load dataset(TODO: verify all files against the dataset header)
            //string directory = datasetServicesManager.CurrentDatasetFolder;
            Dataset dataset = new DatasetProvider().FromDirectory(directory);
            int frameCount = dataset.Frames.Count;

            // load descriptor providers (done in installer)
            ColorSignatureDescriptorProvider colorSignatureProvider
                = ColorSignatureDescriptorProvider.FromDirectory(directory);
            SemanticVectorDescriptorProvider semanticVectorProvider
                = SemanticVectorDescriptorProvider.FromDirectory(directory);


            // load similarity models
            //ColorSignatureModel colorSignatureModel = new ColorSignatureModel(new RankFusionSum(), datasetServicesManager.CurrentDataset.ColorSignatureProvider);
            //FloatVectorModel floatVectorModel = new FloatVectorModel(new RankFusionSum(), datasetServicesManager.CurrentDataset.SemanticVectorProvider);
            ColorSignatureModel colorSignatureModel = new ColorSignatureModel(new RankFusionSum(),colorSignatureProvider);
            FloatVectorModel floatVectorModel = new FloatVectorModel(new RankFusionSum(), semanticVectorProvider);
            KeywordSubModel keywordModel = KeywordSubModel.FromDirectory(directory);
            
            SimilarityModule similarityModule = new SimilarityModule(
                keywordModel, 
                colorSignatureModel, 
                floatVectorModel, 
                new RankFusionSum());


            // load filtering models
            // TODO: move to a provider?
            string colorSaturationFilterFilename = Directory.GetFiles(directory)
                .Where(file => file.EndsWith(".bwfilter")).First();
            float[] colorSaturationFrameAttributes = MaskFilterReader.ReadFilter(colorSaturationFilterFilename);
            ThresholdFilter colorSaturationFilter = new ThresholdFilter(colorSaturationFrameAttributes);

            string percentOfBlackFilterFilename = Directory.GetFiles(directory)
                .Where(file => file.EndsWith(".pbcfilter")).First();
            float[] percentOfBlackFrameAttributes = MaskFilterReader.ReadFilter(percentOfBlackFilterFilename);
            ThresholdFilter percentOfBlackFilter = new ThresholdFilter(percentOfBlackFrameAttributes);

            // TODO: temporary [similarity model - filter model] linking
            //int frameCount = datasetServicesManager.CurrentDataset.DatasetService.FrameCount;
            RankedDatasetFilter colorSketchFilter = new RankedDatasetFilter();
            RankedDatasetFilter keywordFilter = new RankedDatasetFilter();
            RankedDatasetFilter semanticExampleFilter = new RankedDatasetFilter();

            FilteringModule filteringModule = new FilteringModule(
                colorSaturationFilter,
                percentOfBlackFilter,
                colorSketchFilter,
                keywordFilter,
                semanticExampleFilter
                );
            

            // load ranking service
            RankingModule primaryRankingModule = new RankingModule(similarityModule, filteringModule);
            BiTemporalRankingService rankingService = new BiTemporalRankingService(
                //datasetServicesManager.CurrentDataset.DatasetService,
                //frameCount,
                null,
                primaryRankingModule,
                null,
                filteringModule);
            // TODO: secondary service
            

            return rankingService;
        }
    }
}
