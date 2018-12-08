using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public class CountRestrictionFilter
    {
        // dataset dependency
        public Dataset Dataset { get; private set; }

        // cache
        public CountFilteringQuery CachedQuery { get; private set; }
        public Ranking CachedResultRanking { get; private set; }


        public Ranking ComputeRanking(CountFilteringQuery query, Ranking initialRanking)
        {
            // TODO if all filters are off
            if (query.Equals(CachedQuery) && !initialRanking.IsUpdated)
            {
                // not query, nor ranking has changed, return cached ranking
                CachedResultRanking.IsUpdated = false; // just to be sure
                return CachedResultRanking;
            }
            
            int[] groupHitCounter = new int[Dataset.Groups.Count];
            int[] shotHitCounter = new int[Dataset.Shots.Count];
            int[] videoHitCounter = new int[Dataset.Videos.Count];

            // copy initial ranking
            Ranking resultRanking = new Ranking(initialRanking);

            // single thread execution is required
            for (int i = 0; i < resultRanking.Ranks.Length; i++)
            {
                if (resultRanking.Ranks[i] == float.MinValue)
                {
                    // ignore already filtered items
                    continue;
                }

                Frame frame = Dataset.Frames[i];
                int groupId = frame.ParentGroup.Id;
                int shotId = frame.ParentShot.Id;
                int videoId = frame.ParentVideo.Id;

                // video
                if (query.MaxPerVideo > 0 && videoHitCounter[videoId] >= query.MaxPerVideo)
                {
                    resultRanking.Ranks[i] = -1;
                    continue;
                }
                else
                {
                    videoHitCounter[videoId]++;
                }

                // shot
                if (query.MaxPerShot > 0 && shotHitCounter[shotId] >= query.MaxPerShot)
                {
                    resultRanking.Ranks[i] = -1;
                    continue;
                }
                else
                {
                    shotHitCounter[shotId]++;
                }

                // group
                if (query.MaxPerGroup > 0 && groupHitCounter[groupId] >= query.MaxPerGroup)
                {
                    resultRanking.Ranks[i] = -1;
                    continue;
                }
                else
                {
                    groupHitCounter[shotId]++;
                }
            }
            
            // cache result
            CachedQuery = query;
            CachedResultRanking = new Ranking(resultRanking.Ranks, false);
            return resultRanking;
        }


        //public void AddVideoToFilterList(int videoId)
        //{
        //    mVideoFilterHashset.Add(videoId);
        //}

        //public void AddVideoToFilterList(Video video)
        //{
        //    mVideoFilterHashset.Add(video.Id);
        //}

        //public void EnableVideoFilter()
        //{
        //    mVideoFilterEnabled = true;
        //}
        //public void DisableVideoFilter()
        //{
        //    mVideoFilterEnabled = false;
        //}

        //public void ResetVideoFilter()
        //{
        //    mVideoFilterHashset.Clear();
        //}

    }
}
