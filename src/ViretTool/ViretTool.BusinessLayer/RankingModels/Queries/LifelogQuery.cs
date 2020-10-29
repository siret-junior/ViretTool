using System;
using System.Linq;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class LifelogFilteringQuery : IEquatable<LifelogFilteringQuery>
    {
        public const int HEARTRATE_MAX = 250;
        //empty
        public LifelogFilteringQuery()
        {
            DaysOfWeek = new DayOfWeek[0];
            MonthsOfYear = new int[0];
            Years = new int[0];
        }

        public LifelogFilteringQuery(
            DayOfWeek[] daysOfWeek,
            int[] monthsOfYear,
            int[] years,
            TimeSpan timeFrom,
            TimeSpan timeTo,
            int heartRateLow,
            int heartRateHigh,
            string gpsLocation,
            double? gpsLatitude,
            double? gpsLongitude)
        {
            DaysOfWeek = daysOfWeek;
            MonthsOfYear = monthsOfYear;
            Years = years;
            TimeFrom = timeFrom;
            TimeTo = timeTo;
            HeartRateLow = heartRateLow;
            HeartRateHigh = heartRateHigh;
            GpsLocation = gpsLocation;
            GpsLatitude = gpsLatitude;
            GpsLongitude = gpsLongitude;
        }

        public DayOfWeek[] DaysOfWeek { get; }
        public int[] MonthsOfYear { get; }
        public int[] Years { get; }
        public TimeSpan TimeFrom { get; }
        public TimeSpan TimeTo { get; }
        public int HeartRateLow { get; }
        public int HeartRateHigh { get; }

        public string GpsLocation { get; }
        public double? GpsLatitude { get; }
        public double? GpsLongitude { get; }

        public bool Equals(LifelogFilteringQuery other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return DaysOfWeek.SequenceEqual(other.DaysOfWeek) &&
                   MonthsOfYear.SequenceEqual(other.MonthsOfYear) &&
                   Years.SequenceEqual(other.Years) &&
                   TimeFrom.Equals(other.TimeFrom) &&
                   TimeTo.Equals(other.TimeTo) &&
                   HeartRateLow == other.HeartRateLow &&
                   HeartRateHigh == other.HeartRateHigh &&
                   string.Equals(GpsLocation, other.GpsLocation, StringComparison.InvariantCultureIgnoreCase) &&
                   GpsLatitude.Equals(other.GpsLatitude) &&
                   GpsLongitude.Equals(other.GpsLongitude);
        }

        public bool IsGpsDefinedByString => !string.IsNullOrWhiteSpace(GpsLocation);
        public bool IsGpsDefinedByCoordinates => GpsLatitude.HasValue && GpsLongitude.HasValue;

        public bool IsEmpty()
        {
            // TODO: this is dataset dependent
            return (DaysOfWeek == null || DaysOfWeek.Length == 0 || DaysOfWeek.Length == 7)
                && (MonthsOfYear == null || MonthsOfYear.Length == 0 || MonthsOfYear.Length == 12)
                && (Years == null || Years.Length == 0 || Years.Length == 4)
                && TimeFrom.Equals(new TimeSpan(0, 0, 0))
                && TimeTo.Equals(new TimeSpan(24, 0, 0))
                && HeartRateLow == 0
                && HeartRateHigh == HEARTRATE_MAX
                && (GpsLocation == null || GpsLocation.Equals(""))
                && GpsLatitude == null
                && GpsLongitude == null
                ;
        }
    }
}
