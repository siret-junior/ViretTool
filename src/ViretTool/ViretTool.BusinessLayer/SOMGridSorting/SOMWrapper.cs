using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;


namespace ViretTool.BusinessLayer.SOMGridSorting
{
    public class SOMWrapper
    {
        private static object SOMSync = new object();
        private static IntPtr SOMPtr;

        [StructLayout(LayoutKind.Sequential)]
        public struct PointWithId
        {
            public double first;
            public double second;
            public int clustId;
            public int imageId;
            public double sigma;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public double first;
            public double second;
            public int clustId;
        }

        [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "getSOM", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr getSOM(long[] list, int size, double[] probabilities, IntPtr SOMPtr);


        [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "getSOMRepresentants", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr getSOMRepresentants(long[] list, ref int size, double[] probabilities, IntPtr SOMPtr, bool mostProbab, long xdim, long ydim, long rlen);

        [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "initSOM", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr initSOM(double[,] data, int datasetSize, int dims);

        [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "deletePointWithIdArray", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deletePointWithIdArray(IntPtr toDel);

        [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "deletePointArray", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deletePointArray(IntPtr toDel);

        private static void InitSOM_(IDescriptorProvider<float[]> descriptorProvider)
        {
            if (SOMPtr == default(IntPtr))
            {
                float[][] data = descriptorProvider.Descriptors;
                double[,] dataInDouble = new double[data.Length, data[0].Length];
                for(int i = 0; i < data.Length; i ++)
                {
                    for(int j = 0; j < data[i].Length; j++)
                    {
                        dataInDouble[i,j] = (double)data[i][j];
                    }
                }
                SOMPtr = initSOM(dataInDouble, data.Length, data[0].Length);
            }
        }

        public static int[] CreateSOMRepresentants(long[] list, double[] probablities, long xdim, long ydim, long rlen, IDescriptorProvider<float[]> descriptorProvider)
        {
            if (probablities == null)
            {
                probablities = new double[list.Length];
                for (int i = 0; i < probablities.Length; i++)
                {
                    probablities[i] = 1.0 / probablities.Length;
                }
            }

            InitSOM_(descriptorProvider);

            int length = list.Length;
            IntPtr ptr = getSOMRepresentants(list, ref length, probablities, SOMPtr, true, xdim, ydim, rlen);
            MarshalUnmananagedArray2Struct(ptr, length, out PointWithId[] coords);
            deletePointWithIdArray(ptr);
            return coords.Select(x => x.imageId).ToArray();
        }
        public static PointWithId[] CreateWeightedSOMRepresentants(long[] list, double[] probablities, long xdim, long ydim, long rlen, IDescriptorProvider<float[]> descriptorProvider)
        {
            if (probablities == null)
            {
                probablities = new double[list.Length];
                for (int i = 0; i < probablities.Length; i++)
                {
                    probablities[i] = 1.0 / probablities.Length;
                }
            }

            InitSOM_(descriptorProvider);

            int length = list.Length;
            IntPtr ptr = getSOMRepresentants(list, ref length, probablities, SOMPtr, false, xdim, ydim, rlen);
            MarshalUnmananagedArray2Struct(ptr, length, out PointWithId[] coords);
            deletePointWithIdArray(ptr);

            return coords;
        }

        public static Tuple<long, double, double, long>[] CreateSOM(long[] list, IDescriptorProvider<float[]> descriptorProvider)
        {
            InitSOM_(descriptorProvider);
            Tuple<long, double, double, long>[] result = new Tuple<long, double, double, long>[list.Length];
            IntPtr ptr = getSOM(list, list.Length, null, SOMPtr);
            MarshalUnmananagedArray2Struct<Point>(ptr, list.Length, out Point[] coords);
            deletePointArray(ptr);

            for (int i = 0; i < list.Length; i++)
            {
                result[i] = new Tuple<long, double, double, long>(list[i], coords[i].first, coords[i].second, coords[i].clustId);
            }

            return result;
        }

        public static void MarshalUnmananagedArray2Struct<T>(IntPtr unmanagedArray, int length, out T[] mangagedArray)
        {
            var size = Marshal.SizeOf(typeof(T));
            mangagedArray = new T[length];

            for (int i = 0; i < length; i++)
            {
                IntPtr ins = new IntPtr(unmanagedArray.ToInt64() + i * size);
                mangagedArray[i] = Marshal.PtrToStructure<T>(ins);
            }
        }
    }
}
