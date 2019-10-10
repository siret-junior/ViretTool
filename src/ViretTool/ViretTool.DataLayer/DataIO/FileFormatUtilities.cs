using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO
{
    public class FileFormatUtilities
    {
        public static void CheckValueInRange<T>(string name, T value, T minValue, T maxValue) where T : IComparable
        {
            if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
            {
                throw new InvalidDataException($"{name}: {value} is outside expected range [{minValue}, {maxValue}].");
            }
        }

        public static void CheckValuesInRange<T>(string name, T[] values, T minValue, T maxValue) where T : IComparable
        {
            for (int i = 0; i < values.Length; i++)
            {
                T value = values[i];
                CheckValueInRange(name + $"[{i}]", value, minValue, maxValue);
            }
        }
        
        public static void CheckValuesIncrement<T>(string name, T[] values) where T : IComparable
        {
            if (values.Length > 0)
            {
                T lastValue = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i].CompareTo(lastValue) <= 0)
                    {
                        throw new InvalidDataException($"{name}[{i}]: {lastValue} -> {values[i]} does not increment.");
                    }
                    lastValue = values[i];
                }
            }
        }
    }
}
