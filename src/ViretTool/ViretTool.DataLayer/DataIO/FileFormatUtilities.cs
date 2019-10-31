using System;
using System.IO;

namespace ViretTool.DataLayer.DataIO
{
    /// <summary>
    /// Helper routines used to check file formats.
    /// </summary>
    public class FileFormatUtilities
    {
        /// <summary>
        /// Checks whether the value is in defined range (including border values).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of the value, used in exception message.</param>
        /// <param name="value">The value that is being tested.</param>
        /// <param name="minValue">Lower bound for the tested value (including this value).</param>
        /// <param name="maxValue">Upper bound for the tested value (including this value).</param>
        public static void CheckValueInRange<T>(string name, T value, T minValue, T maxValue) where T : IComparable
        {
            if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
            {
                throw new InvalidDataException($"{name}: {value} is outside expected range [{minValue}, {maxValue}].");
            }
        }

        /// <summary>
        /// Checks every item value in the input array whether it is in the defined range (including border values).
        /// </summary>
        /// <typeparam name="T">IComparable type of the array item values.</typeparam>
        /// <param name="name">Name of the input array, used in exception message.</param>
        /// <param name="values">An array of values to be tested.</param>
        /// <param name="minValue">Lower bound for the tested array items (including this value).</param>
        /// <param name="maxValue">Upper bound for the tested array items (including this value).</param>
        public static void CheckValuesInRange<T>(string name, T[] values, T minValue, T maxValue) where T : IComparable
        {
            for (int i = 0; i < values.Length; i++)
            {
                T value = values[i];
                CheckValueInRange(name + $"[{i}]", value, minValue, maxValue);
            }
        }
        
        /// <summary>
        /// Checks whether the values in the input array are incrementing.
        /// Consecutive values can not be the same value, they have to increment.
        /// </summary>
        /// <typeparam name="T">IComparable type of the array items.</typeparam>
        /// <param name="name">Name of the input array, used in exception message.</param>
        /// <param name="values">Input array to be tested.</param>
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
