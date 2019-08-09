using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public interface IKeywordModel : ISimilarityModel<KeywordQuery>
    {
        float[] GetScoring(int synsetId);
        float[] GetScoring(Synset synsetLiteral);
        float[] GetScoring(SynsetClause synsetClause);
        float[] GetScoring(SynsetClause[] synsetFormula);
    }
}
