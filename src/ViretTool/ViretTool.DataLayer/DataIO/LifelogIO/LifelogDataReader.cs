using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.DataLayer.DataIO.LifelogIO
{
    public class LifelogDataReader
    {
        public IEnumerable<LifelogFrameInfo> Read(string lifelogDataFileName)
        {
            dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(lifelogDataFileName));
            foreach (dynamic item in data)
            {
                string fileName = item.id;
                TimeSpan time = TimeSpan.ParseExact(fileName.Split('_')[1], "hhmmss", CultureInfo.InvariantCulture);
                bool fromVideo = item.fromVideo;

                yield return new LifelogFrameInfo(
                    fileName,
                    fromVideo,
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
            return string.IsNullOrWhiteSpace(gpsCoordinate) ? (double?)null : double.Parse(gpsCoordinate, CultureInfo.InvariantCulture);
        }
    }
}
