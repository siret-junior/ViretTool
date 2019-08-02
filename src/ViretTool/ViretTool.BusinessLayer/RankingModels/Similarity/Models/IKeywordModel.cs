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
        Dictionary<int, float> GetRankForOneSynsetGroup(List<int> listOfIds);
    }
}
