using System;

namespace ViretTool.BusinessLayer.Descriptors.Models
{
    public class LifelogFrameMetadata
    {
        public LifelogFrameMetadata(string fileName, bool fromVideo, DateTime date, DayOfWeek dayOfWeek, TimeSpan time, int? heartRate, string gpsLocation, double? gpsLatitude, double? gpsLongitude)
        {
            FileName = fileName;
            FromVideo = fromVideo;
            Day = date.Day;
            Month = date.Month;
            Year = date.Year;
            DayOfWeek = dayOfWeek;
            Time = time;
            HeartRate = heartRate;
            GpsLocation = gpsLocation;
            GpsLatitude = gpsLatitude;
            GpsLongitude = gpsLongitude;
        }

        public string FileName { get; }
        public bool FromVideo { get; }
        public int Day { get; }
        public int Month { get; }
        public int Year { get; }
        public DayOfWeek DayOfWeek { get; }
        public TimeSpan Time { get; }
        public int? HeartRate { get; }
        
        public string GpsLocation { get; }

        public double? GpsLatitude { get; }
        public double? GpsLongitude { get; }

    }
}
