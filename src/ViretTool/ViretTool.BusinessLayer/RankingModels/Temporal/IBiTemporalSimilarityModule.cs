using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalSimilarityModule
    {
        IBiTemporalKeywordModel BiTemporalKeywordModel { get; }
        IBiTemporalColorSignatureModel BiTemporalColorSignatureModel { get; }
        IBiTemporalSemanticExampleModel BiTemporalSemanticExampleModel { get; }
        IRankFusion RankFusion { get; }

        BiTemporalSimilarityQuery CachedQuery { get; }
        RankingBuffer InputRanking { get; }
        BiTemporalRankingBuffer KeywordIntermediateRanking { get; }
        BiTemporalRankingBuffer ColorSketchIntermediateRanking { get; }
        BiTemporalRankingBuffer SemanticExampleIntermediateRanking { get; }
        BiTemporalRankingBuffer OutputRanking { get; }


        void ComputeRanking(BiTemporalSimilarityQuery query, 
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking,
            BiTemporalRankingBuffer colorSketchIntermediateRanking,
            BiTemporalRankingBuffer keywordIntermediateRanking,
            BiTemporalRankingBuffer semanticExampleIntermediateRanking);
    }
}
