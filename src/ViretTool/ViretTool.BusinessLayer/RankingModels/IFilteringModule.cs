using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IFilteringModule
    {
        IColorSaturationFilter ColorSaturationFilter { get; }
        IPercentOfBlackFilter PercentOfBlackColorFilter { get; }
        ICountRestrictionFilter CountRestrictionFilter { get; }

        FilteringQuery CachedQuery { get; }
        RankingBuffer InputRanking { get; }
        RankingBuffer MaskIntermediateRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeRanking(FilteringQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
