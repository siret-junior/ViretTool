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
    }
}
