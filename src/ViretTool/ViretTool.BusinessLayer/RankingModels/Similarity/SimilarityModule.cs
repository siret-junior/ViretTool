using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity
{
    public class SimilarityModule : ISimilarityModule
    {
        public IKeywordModel<KeywordQuery> KeywordModel { get; set; }
        public IColorSketchModel<ColorSketchQuery> ColorSketchModel { get; set; }
        public ISemanticExampleModel<SemanticExampleQuery> SemanticExampleModel { get; set; }

        public IRankFusion RankFusion { get; set; }

        public SimilarityQuery CachedQuery { get; private set; }
        public Ranking InputRanking { get; private set; }
        public Ranking OutputRanking { get; private set; }
        
        
        public void ComputeRanking(SimilarityQuery query)
        {
            if (query.Equals(CachedQuery) && !InputRanking.IsUpdated)
            {
                OutputRanking.IsUpdated = false;
            }
            else
            {
                // compute partial rankings (if neccessary)
                KeywordModel.ComputeRanking(query.KeywordQuery);
                ColorSketchModel.ComputeRanking(query.ColorSketchQuery);
                SemanticExampleModel.ComputeRanking(query.SemanticExampleQuery);

                // perform rank fusion
                RankFusion.ComputeRanking(new Ranking[] {
                    KeywordModel.OutputRanking,
                    ColorSketchModel.OutputRanking,
                    SemanticExampleModel.OutputRanking
                });
                // TODO: RankFusion should share the ranking object with SimilarityModule

                // cache the query
                CachedQuery = query;
            }
        }
    }
}
