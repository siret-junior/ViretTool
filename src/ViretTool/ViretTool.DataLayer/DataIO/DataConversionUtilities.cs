using System;
using System.IO;

namespace ViretTool.DataLayer.DataIO
{
    /// <summary>
    /// Provides helper routines for conversion between data formats (mainly between array types).
    /// </summary>
    internal static class DataConversionUtilities
    {
        #region --[ Conversion from byte array ]--

        public static float[] ConvertToFloatArray(byte[] byteArray)
        {
            if (byteArray.Length % sizeof(float) != 0)
            {
                throw new ArgumentException($"Length of byte array ({byteArray.Length}) is not divisible by float size ({sizeof(float)}).");
            }

            float[] floats = new float[byteArray.Length / sizeof(float)];
            Buffer.BlockCopy(byteArray, 0, floats, 0, byteArray.Length);
            return floats;
        }

        public static int[] ConvertToIntArray(byte[] byteArray)
        {
            if (byteArray.Length % sizeof(int) != 0)
            {
                throw new ArgumentException($"Length of byte array ({byteArray.Length}) is not divisible by int size ({sizeof(int)}).");
            }

            int[] integers = new int[byteArray.Length / sizeof(int)];
            Buffer.BlockCopy(byteArray, 0, integers, 0, byteArray.Length);
            return integers;
        }

        public static long[] ConvertToLongArray(byte[] byteArray)
        {
            if (byteArray.Length % sizeof(long) != 0)
            {
                throw new ArgumentException($"Length of byte array ({byteArray.Length}) is not divisible by long size ({sizeof(long)}).");
            }

            long[] longs = new long[byteArray.Length / sizeof(long)];
            Buffer.BlockCopy(byteArray, 0, longs, 0, byteArray.Length);
            return longs;
        }

        public static bool[] ConvertToBoolArray(byte[] byteArray)
        {
            if (byteArray.Length % sizeof(bool) != 0)
            {
                throw new ArgumentException($"Length of byte array ({byteArray.Length}) is not divisible by bool size ({sizeof(bool)}).");
            }
            
            bool[] bools = new bool[byteArray.Length / sizeof(bool)];
            Buffer.BlockCopy(byteArray, 0, bools, 0, byteArray.Length);
            return bools;
        }

        #endregion --[ Conversion from byte array ]--


        #region --[ Conversion to byte array ]--

        public static byte[] ConvertToByteArray(float[] floatArray)
        {
            byte[] bytes = new byte[floatArray.Length * sizeof(float)];
            Buffer.BlockCopy(floatArray, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static byte[] ConvertToByteArray(bool[] boolArray)
        {
            byte[] bytes = new byte[boolArray.Length * sizeof(bool)];
            Buffer.BlockCopy(boolArray, 0, bytes, 0, bytes.Length);
            return bytes;
        }
        
        // TODO: find an efficient solution (also Buffer.BlockCopy()?)
        public static byte[] ConvertToByteArray((int synsetId, float rank)[] synsetRanks)
        {
            using (MemoryStream stream = new MemoryStream(synsetRanks.Length * (sizeof(int) + sizeof(float))))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach ((int synsetId, float rank) in synsetRanks)
                {
                    writer.Write(synsetId);
                    writer.Write(rank);
                }
                
                return stream.ToArray();
            }
        }

        #endregion --[ Conversion to byte array ]--
    }
}
