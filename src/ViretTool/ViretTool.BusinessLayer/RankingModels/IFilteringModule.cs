using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IFilteringModule
    {
        // color filters
        IColorSaturationFilter ColorSaturationFilter { get; }
        IPercentOfBlackFilter PercentOfBlackColorFilter { get; }
        // ranking model percent of dataset filters
        IColorSignatureRankedDatasetFilter ColorSignatureRankingFilter { get; }
        IKeywordRankedDatasetFilter KeywordRankingFilter { get; }
        ISemanticExampleRankedDatasetFilter SemanticExampleRankingFilter { get; }

        FilteringQuery CachedQuery { get; }
        RankingBuffer InputRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeRanking(FilteringQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking,
            RankingBuffer colorSignatureRanking,
            RankingBuffer keywordRanking,
            RankingBuffer semanticExampleRanking);
    }
}
