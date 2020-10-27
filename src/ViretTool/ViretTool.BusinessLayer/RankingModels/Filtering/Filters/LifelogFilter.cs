using System;
using System.Linq;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public class LifelogFilter : ILifelogFilter
    {
        private readonly ILifelogDescriptorProvider _lifelogDescriptorProvider;
        private LifelogFilteringQuery _cacheQuery;

        public LifelogFilter(ILifelogDescriptorProvider lifelogDescriptorProvider)
        {
            _lifelogDescriptorProvider = lifelogDescriptorProvider;
        }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }

        public void ComputeFiltering(LifelogFilteringQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

            if (!HasQueryOrInputChanged(query, inputRanking))
            {
                // nothing changed, OutputRanking contains cached data from previous computation
                OutputRanking.IsUpdated = false;
                return;
            }

            _cacheQuery = query;
            OutputRanking.IsUpdated = true;

            ApplyLifelogFilters();
        }

        private bool HasQueryOrInputChanged(LifelogFilteringQuery query, RankingBuffer inputRanking)
        {
            return (query == null && _cacheQuery != null) 
                || (_cacheQuery == null && query != null) 
                || !Equals(query, _cacheQuery) 
                || inputRanking.IsUpdated;
        }

        private void ApplyLifelogFilters()
        {
            float[] inputRanks = InputRanking.Ranks;
            float[] outputRanks = OutputRanking.Ranks;
            LifelogFilteringQuery query = _cacheQuery;
            
            bool computeGpsDistances = !query.IsGpsDefinedByString && query.IsGpsDefinedByCoordinates;
            (float dist, int id)[] distancesByGps = computeGpsDistances ? new (float dist, int id)[inputRanks.Length] : null;
            const int numberOfClosestObjects = 100;

            Parallel.For(
                0,
                InputRanking.Ranks.Length,
                id =>
                {
                    if (inputRanks[id].Equals(float.MinValue))
                    {
                        outputRanks[id] = float.MinValue;
                        return;
                    }

                    LifelogFrameMetadata metadata = _lifelogDescriptorProvider[id];
                    if (!query.DaysOfWeek.Contains(metadata.DayOfWeek) ||
                        metadata.HeartRate < query.HeartRateLow ||
                        metadata.HeartRate > query.HeartRateHigh ||
                        metadata.Time < query.TimeFrom ||
                        metadata.Time > query.TimeTo ||
                        query.IsGpsDefinedByString && !query.GpsLocation.Equals(metadata.GpsLocation, StringComparison.InvariantCultureIgnoreCase))
                    {
                        outputRanks[id] = float.MinValue;
                        return;
                    }

                    if (computeGpsDistances)
                    {
                        //only top K will have the correct value
                        outputRanks[id] = float.MinValue;
                        distancesByGps[id] = ComputeGpsDistance(id);
                    }
                    else
                    {
                        outputRanks[id] = inputRanks[id];
                    }
                });

            if (computeGpsDistances)
            {
                foreach ((float _, int id) in distancesByGps.OrderBy(pair => pair.dist).Take(numberOfClosestObjects))
                {
                    outputRanks[id] = inputRanks[id];
                }
            }

            (float dist, int id) ComputeGpsDistance(int id)
            {
                double? latitudeDif = query.GpsLatitude - _lifelogDescriptorProvider[id].GpsLatitude;
                double? longitudeDif = query.GpsLongitude - _lifelogDescriptorProvider[id].GpsLongitude;
                double? distance = latitudeDif * latitudeDif + longitudeDif * longitudeDif;
                if (!distance.HasValue)
                {
                    return (float.MaxValue, id);
                }

                return ((float)Math.Sqrt(distance.Value), id);
            }
        }
    }
}
