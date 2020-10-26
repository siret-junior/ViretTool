﻿using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Descriptors.KeywordLabel;
using ViretTool.BusinessLayer.Descriptors.KeywordTopScoring;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.BusinessLayer.Services
{
    /// <summary>
    /// Class for storing services bound to a specific dataset
    /// </summary>
    public class DatasetServices
    {
        // All services that need disposing or closing functionality have to be passed here
        public DatasetServices(
            IThumbnailService<Thumbnail<byte[]>> thumbnailService,
            IDatasetService datasetService,
            IColorSignatureDescriptorProvider colorSignatureProvider,
            IFaceSignatureDescriptorProvider faceSignatureProvider,
            ITextSignatureDescriptorProvider textSignatureProvider,
            IDescriptorProvider<float[]> semanticVectorProvider,
            IDescriptorProvider<(int synsetId, float probability)[]> keywordSynsetProvider,
            IKeywordLabelProvider<string> keywordLabelProvider,
            IKeywordTopScoringProvider keywordScoringProvider,
            IBiTemporalRankingService rankingService,
            IDatasetParameters datasetParameters,
            ILifelogDescriptorProvider lifelogDescriptorProvider,
            IInitialDisplayProvider initialDisplayProvider,
            IZoomDisplayProvider zoomDisplayProvider,
            SomGeneratorProvider somGeneratorProvider)
        {
            ThumbnailService = thumbnailService;
            DatasetService = datasetService;
            ColorSignatureProvider = colorSignatureProvider;
            FaceSignatureProvider = faceSignatureProvider;
            TextSignatureProvider = textSignatureProvider;
            SemanticVectorProvider = semanticVectorProvider;
            KeywordSynsetProvider = keywordSynsetProvider;
            KeywordLabelProvider = keywordLabelProvider;
            KeywordScoringProvider = keywordScoringProvider;
            RankingService = rankingService;
            DatasetParameters = datasetParameters;
            LifelogDescriptorProvider = lifelogDescriptorProvider;
            InitialDisplayProvider = initialDisplayProvider;
            ZoomDisplayProvider = zoomDisplayProvider;
            SomGeneratorProvider = somGeneratorProvider;
        }

        public IThumbnailService<Thumbnail<byte[]>> ThumbnailService { get; }
        public IDatasetService DatasetService { get; }
        public IColorSignatureDescriptorProvider ColorSignatureProvider { get; }
        public IFaceSignatureDescriptorProvider FaceSignatureProvider { get; }
        public ITextSignatureDescriptorProvider TextSignatureProvider { get; }
        public IDescriptorProvider<float[]> SemanticVectorProvider { get; }
        public IDescriptorProvider<(int synsetId, float probability)[]> KeywordSynsetProvider { get; }
        public IKeywordLabelProvider<string> KeywordLabelProvider { get; }
        public IKeywordTopScoringProvider KeywordScoringProvider { get; }
        public IBiTemporalRankingService RankingService { get; }
        public IDatasetParameters DatasetParameters { get; }
        public ILifelogDescriptorProvider LifelogDescriptorProvider { get; }
        public IInitialDisplayProvider InitialDisplayProvider { get; }
        public IZoomDisplayProvider ZoomDisplayProvider { get; }
        public SomGeneratorProvider SomGeneratorProvider { get; }
    }
}
