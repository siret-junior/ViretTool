using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO
{
    internal static class DataConversionUtilities
    {
        public static float[] TranslateToFloatArray(byte[] byteBuffer)
        {
            float[] floats = new float[byteBuffer.Length / sizeof(float)];
            Buffer.BlockCopy(byteBuffer, 0, floats, 0, byteBuffer.Length);
            return floats;
        }

        public static int[] TranslateToIntArray(byte[] byteBuffer)
        {
            int[] integers = new int[byteBuffer.Length / sizeof(int)];
            Buffer.BlockCopy(byteBuffer, 0, integers, 0, byteBuffer.Length);
            return integers;
        }

        public static long[] TranslateToLongArray(byte[] byteBuffer)
        {
            long[] longs = new long[byteBuffer.Length / sizeof(long)];
            Buffer.BlockCopy(byteBuffer, 0, longs, 0, byteBuffer.Length);
            return longs;
        }

        public static byte[] TranslateToByteArray(float[] floatArray)
        {
            byte[] bytes = new byte[floatArray.Length * sizeof(float)];
            Buffer.BlockCopy(floatArray, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static bool[] TranslateToBoolArray(byte[] byteBuffer)
        {
            bool[] bools = new bool[byteBuffer.Length / sizeof(bool)];
            Buffer.BlockCopy(byteBuffer, 0, bools, 0, byteBuffer.Length);
            return bools;
        }

        public static byte[] TranslateToByteArray(bool[] boolArray)
        {
            byte[] bytes = new byte[boolArray.Length * sizeof(bool)];
            Buffer.BlockCopy(boolArray, 0, bytes, 0, boolArray.Length);
            return bytes;
        }
    }
}
