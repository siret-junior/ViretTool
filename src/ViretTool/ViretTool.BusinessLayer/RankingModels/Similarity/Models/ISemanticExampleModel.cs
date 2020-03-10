﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public interface ISemanticExampleModel : ISimilarityModel<SemanticExampleQuery>
    {
        float[] ComputeSimilarity(float[] queryData, float[] inputRanks);
        float[] ComputeSimilarity(float[] queryData);
    }
}
