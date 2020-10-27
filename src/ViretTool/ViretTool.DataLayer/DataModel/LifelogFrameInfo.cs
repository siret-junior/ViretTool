using System;

namespace ViretTool.DataLayer.DataModel
{
    /// <summary>
    /// Additional Frame attributes used in Lifelog Search Challenge (LSC).
    /// </summary>
    public class LifelogFrameInfo
    {
        public LifelogFrameInfo(string fileName, bool fromVideo, DateTime date, DayOfWeek dayOfWeek, TimeSpan time, int? heartRate, string gpsLocation, double? gpsLatitude, double? gpsLongitude)
        {
            FileName = fileName;
            FromVideo = fromVideo;
            Date = date;
            DayOfWeek = dayOfWeek;
            Time = time;
            HeartRate = heartRate;
            GpsLocation = gpsLocation;
            GpsLatitude = gpsLatitude;
            GpsLongitude = gpsLongitude;
        }

        public string FileName { get; }
        public bool FromVideo { get; }
        public DateTime Date { get; }
        public DayOfWeek DayOfWeek { get; }
        public TimeSpan Time { get; }
        public int? HeartRate { get; }

        public string GpsLocation { get; }

        public double? GpsLatitude { get; }
        public double? GpsLongitude { get; }
    }
}
