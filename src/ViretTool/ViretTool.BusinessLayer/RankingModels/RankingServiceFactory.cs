﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Fusion;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.ColorSignatureModel;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNFeatures;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNKeywords;
using ViretTool.BusinessLayer.Services;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ViretTool.BusinessLayer.RankingModels
{
    //TODO not needed - will be provided by castle

    public class RankingServiceFactory
    {

        public static IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet>
            Build(IDatasetServicesManager datasetServicesManager)
        {
            string directory = datasetServicesManager.CurrentDatasetFolder;

            //load dataset(TODO: verify all files against the dataset header)
            // load descriptor providers
            // TODO: keyword, filtering...

            // load similarity models
            ColorSignatureModel colorSignatureModel = new ColorSignatureModel(new RankFusionSum(), datasetServicesManager.CurrentDataset.ColorSignatureProvider);
            FloatVectorModel floatVectorModel = new FloatVectorModel(new RankFusionSum(), datasetServicesManager.CurrentDataset.SemanticVectorProvider);
            KeywordSubModel keywordModel = KeywordSubModel.FromDirectory(directory);

            // load modules
            SimilarityModule similarityModule = new SimilarityModule(keywordModel, colorSignatureModel, floatVectorModel, new RankFusionSum());
            FilteringModule filteringModule = new FilteringModule();

            // TODO: implement filtering

            // load service
            RankingModule primaryRankingModule = new RankingModule(similarityModule, filteringModule);
            BiTemporalRankingService rankingService = new BiTemporalRankingService(
                datasetServicesManager.CurrentDataset.DatasetService,
                primaryRankingModule,
                null,
                filteringModule);
            // TODO: secondary service
            
            return rankingService;
        }



        //public static IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet> 
        //    Build(IDatasetServicesManager datasetServicesManager)
        //{
        //    string directory = datasetServicesManager.CurrentDatasetFolder;

        //    // load dataset (TODO: verify all files against the dataset header)
        //    Dataset dataset = new DatasetProvider().FromDirectory(directory);
        //    int frameCount = dataset.Frames.Count;

        //    // load descriptor providers
        //    IDescriptorProvider<byte[]> colorSignatureProvider = datasetServicesManager.CurrentDataset.ColorSignatureProvider;
        //    IDescriptorProvider<float[]> semanticVectorProvider = datasetServicesManager.CurrentDataset.SemanticVectorProvider;
        //    // TODO: keyword, filtering...

        //    // load similarity models
        //    ColorSignatureModel colorSignatureModel 
        //        = new ColorSignatureModel(colorSignatureProvider.Descriptors);
        //    FloatVectorModel floatVectorModel
        //        = new FloatVectorModel(semanticVectorProvider.Descriptors);
        //    KeywordSubModel keywordModel = KeywordSubModel.FromDirectory(directory);

        //    // load modules
        //    SimilarityModule similarityModule = new SimilarityModule();
        //    FilteringModule filteringModule = new FilteringModule();
        //    RankingModule rankingModule = new RankingModule();

        //    // fill module dependencies
        //    colorSignatureModel.RankFusion = new RankFusionSum();
        //    floatVectorModel.RankFusion = new RankFusionSum();
        //    similarityModule.ColorSketchModel = colorSignatureModel;
        //    similarityModule.SemanticExampleModel = floatVectorModel;
        //    similarityModule.KeywordModel = keywordModel;
        //    similarityModule.RankFusion = new RankFusionSum();
        //    // TODO: implement filtering

        //    // load service
        //    BiTemporalRankingService rankingService = new BiTemporalRankingService();
        //    RankingModule primaryRankingModule = new RankingModule();
        //    primaryRankingModule.SimilarityModule = similarityModule;
        //    primaryRankingModule.FilteringModule = filteringModule;
        //    rankingService.PrimaryRankingModule = primaryRankingModule;
        //    // TODO: secondary service

        //    // link modules (module rankings)

        //    // layer 1 - initial ranking (for dataset split between multiple tools)
        //    RankingBuffer initialRanking = RankingBuffer.Zeros(frameCount);
        //    rankingService.PrimaryRankingModule.InputRanking = initialRanking;
        //    rankingService.PrimaryRankingModule.SimilarityModule.InputRanking = initialRanking;
        //    rankingService.PrimaryRankingModule.SimilarityModule.KeywordModel.InputRanking = initialRanking;
        //    rankingService.PrimaryRankingModule.SimilarityModule.ColorSketchModel.InputRanking = initialRanking;
        //    rankingService.PrimaryRankingModule.SimilarityModule.SemanticExampleModel.InputRanking = initialRanking;

        //    // layer 2 - similarity models into similarity fusion
        //    RankingBuffer keywordModelToSimilarityFusion = RankingBuffer.Zeros(frameCount);
        //    RankingBuffer colorModelToSimilarityFusion = RankingBuffer.Zeros(frameCount);
        //    RankingBuffer semanticExampleModelToSimilarityFusion = RankingBuffer.Zeros(frameCount);
        //    keywordModel.OutputRanking = keywordModelToSimilarityFusion;
        //    colorSignatureModel.OutputRanking = colorModelToSimilarityFusion;
        //    colorSignatureModel.RankFusion.OutputRanking = colorModelToSimilarityFusion;
        //    floatVectorModel.OutputRanking = semanticExampleModelToSimilarityFusion;
        //    floatVectorModel.RankFusion.OutputRanking = semanticExampleModelToSimilarityFusion;

        //    // layer 3 - similarity fusion into filtering
        //    RankingBuffer similarityFusionToFiltering = RankingBuffer.Zeros(frameCount);
        //    similarityModule.OutputRanking = similarityFusionToFiltering;
        //    similarityModule.RankFusion.OutputRanking = similarityFusionToFiltering;
        //    filteringModule.InputRanking = similarityFusionToFiltering;

        //    // layer 4 - filtering into temporal fusion
        //    RankingBuffer filteringToTemporalFusion = RankingBuffer.Zeros(frameCount);
        //    filteringModule.OutputRanking = filteringToTemporalFusion;
        //    // TODO: second temporal model
        //    rankingService.PrimaryRankingModule.OutputRanking = filteringToTemporalFusion;

        //    // layer 5 - temporal fusion to the final ranking result
        //    //Ranking temporalFusionToResult = Ranking.Zeros(frameCount);

        //    return rankingService;
        //}
    }
}
