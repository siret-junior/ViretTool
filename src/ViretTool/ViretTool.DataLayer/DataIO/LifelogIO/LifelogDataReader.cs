using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.DataLayer.DataIO.LifelogIO
{
    /// <summary>
    /// Reads data that is specific to Lifelog Search Challenge (LSC).
    /// </summary>
    public class LifelogDataReader
    {
        public IEnumerable<LifelogFrameInfo> Read(string lifelogDataFileName)
        {
            dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(lifelogDataFileName));
            foreach (dynamic item in data)
            {
                string fileName = item.id;
                
                // backwards compatibility LSC2018-2019
                TimeSpan time;
                try
                {
                    string timeString = item.time;
                    time = TimeSpan.ParseExact(timeString, "hhmmss", CultureInfo.InvariantCulture);
                }
                catch
                {
                    // TODO: throwing an exception is very inefficient
                    time = TimeSpan.ParseExact(fileName.Split('_')[1], "hhmmss", CultureInfo.InvariantCulture);
                }

                bool fromVideo = item.fromVideo;

                yield return new LifelogFrameInfo(
                    fileName,
                    fromVideo,
                    DateTime.ParseExact((string)item.date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    (DayOfWeek)((int)item.weekday % 7),
                    time,
                    (int?)item.heartrate,
                    (string)item.loc_name,
                    GetGpsCoordinate((string)item.loc_coord[0]),
                    GetGpsCoordinate((string)item.loc_coord[1]));
            }
        }

        private double? GetGpsCoordinate(string gpsCoordinate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gpsCoordinate) || gpsCoordinate.Equals("NULL"))
                {
                    return null;
                }
                else 
                { 
                    return double.Parse(gpsCoordinate, CultureInfo.InvariantCulture); 
                }
            }
            catch 
            {
                return null;
            }
        }
    }
}
