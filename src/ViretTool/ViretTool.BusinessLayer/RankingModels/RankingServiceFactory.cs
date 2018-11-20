using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class RankingServiceFactory
    {
        public static IRankingService<RankedFrame[]> Build(string directory)
        {
            // load dataset
            Dataset dataset = DatasetProvider.FromDirectory(directory);
            int frameCount = dataset.Frames.Count;

            // load data model


            // load similarity models


            // load modules

            SimilarityModule similarityModule = new SimilarityModule();
            FilteringModule filteringModule = new FilteringModule();
            RankingModule rankingModule = new RankingModule();
            
            // load service
            RankingService rankingService = new RankingService();

            
            // link modules
            Ranking rankingToSimilarity = Ranking.Zeros(frameCount);
            
            return rankingService;
        }
    }
}
