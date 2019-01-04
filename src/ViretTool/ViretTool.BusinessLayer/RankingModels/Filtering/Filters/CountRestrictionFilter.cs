using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public class CountRestrictionFilter : ICountRestrictionFilter
    {
        // dataset dependency
        public IDatasetService DatasetService { get; private set; }

        // cache
        public CountFilteringQuery CachedQuery { get; private set; }
        
        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }


        public CountRestrictionFilter(IDatasetService datasetService)
        {
            DatasetService = datasetService;
        }


        public void ComputeFiltering(CountFilteringQuery query, 
            RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

            if (!HasQueryOrInputChanged(query, inputRanking))
            {
                // nothing changed, OutputRanking contains cached data from previous computation
                OutputRanking.IsUpdated = false;
                return;
            }
            else
            {
                CachedQuery = query;
                OutputRanking.IsUpdated = true;
            }

            if (IsQueryEmpty(query))
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
                return;
            }

            // prepare data
            int[] indexes = new int[inputRanking.Ranks.Length];
            float[] ranks = new float[inputRanking.Ranks.Length];
            Parallel.For(0, indexes.Length, index =>
            {
                indexes[index] = index;
                ranks[index] = inputRanking.Ranks[index];
            });
            // sort ranks in descending order
            Array.Sort(ranks, indexes, Comparer<float>.Create((x, y) => y.CompareTo(x)));
            
            int[] groupHitCounter = new int[DatasetService.Dataset.Groups.Count];
            int[] shotHitCounter = new int[DatasetService.Dataset.Shots.Count];
            int[] videoHitCounter = new int[DatasetService.Dataset.Videos.Count];
            
            // single thread execution is required
            for (int index = 0; index < indexes.Length; index++)
            {
                int i = indexes[index];
                if (inputRanking.Ranks[i] == float.MinValue)
                {
                    // ignore already filtered items
                    continue;
                }

                Frame frame = DatasetService.Dataset.Frames[i];
                int groupId = frame.ParentGroup.Id;
                int shotId = frame.ParentShot.Id;
                int videoId = frame.ParentVideo.Id;

                // video
                if (query.MaxPerVideo > 0 && videoHitCounter[videoId] >= query.MaxPerVideo)
                {
                    outputRanking.Ranks[i] = float.MinValue;
                    continue;
                }
                else
                {
                    videoHitCounter[videoId]++;
                }

                // shot
                if (query.MaxPerShot > 0 && shotHitCounter[shotId] >= query.MaxPerShot)
                {
                    outputRanking.Ranks[i] = float.MinValue;
                    continue;
                }
                else
                {
                    shotHitCounter[shotId]++;
                }

                // group
                if (query.MaxPerGroup > 0 && groupHitCounter[groupId] >= query.MaxPerGroup)
                {
                    outputRanking.Ranks[i] = float.MinValue;
                    continue;
                }
                else
                {
                    groupHitCounter[shotId]++;
                }

                outputRanking.Ranks[i] = inputRanking.Ranks[i];
            }
        }

        
        private bool HasQueryOrInputChanged(CountFilteringQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(CountFilteringQuery query)
        {
            return query == null ||
                ( 
                    query.MaxPerVideo <= 0
                    && query.MaxPerShot <= 0
                    && query.MaxPerGroup <= 0
                );
        }


    }
}
