using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity.Models
{
    public interface IBiTemporalColorSignatureModel
        : IBiTemporalSimilarityModel<IColorSignatureModel<ColorSketchQuery>, ColorSketchQuery>
    {
    }
}
