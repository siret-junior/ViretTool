using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ViretTool.BusinessLayer.RankingModels
{
    //TODO not needed - will be provided by castle

    //public class RankingServiceFactory
    //{
    //    public static IRankingService<Query, RankedFrame[]> Build(string directory)
    //    {
    //        // load dataset
    //        Dataset dataset = DatasetProvider.FromDirectory(directory);
    //        int frameCount = dataset.Frames.Count;

    //        // load data model


    //        // load similarity models


    //        // load modules

    //        SimilarityModule similarityModule = new SimilarityModule();
    //        FilteringModule filteringModule = new FilteringModule();
    //        RankingModule rankingModule = new RankingModule();
            
    //        // load service
    //        RankingService rankingService = new RankingService();

            
    //        // link modules
    //        Ranking rankingToSimilarity = Ranking.Zeros(frameCount);
            
    //        return rankingService;
    //    }
    //}
}
