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
        public Ranking InputRanking { get; set; }
        public Ranking OutputRanking { get; set; }
        
        
        public void ComputeRanking(SimilarityQuery query)
        {
            if ((query == null && CachedQuery == null) || query.Equals(CachedQuery) && !InputRanking.IsUpdated)
            {
                OutputRanking.IsUpdated = false;
                return;
            }

            if (query != null)
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
            else
            {
                // TODO just reassign array reference (also in submodels)
                // null query, set to 0 rank
                for (int i = 0; i < OutputRanking.Ranks.Length; i++)
                {
                    if (InputRanking.Ranks[i] == float.MinValue)
                    {
                        OutputRanking.Ranks[i] = float.MinValue;
                    }
                    else
                    {
                        OutputRanking.Ranks[i] = 0;
                    }
                }
            }
            OutputRanking.IsUpdated = true;
        }
    }
}
