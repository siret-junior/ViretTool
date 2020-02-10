using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace RFTesterBackend.ResourceManagers
{
  public class SOMWrapper
  {
    private static object SOMSync = new object();
    private static IntPtr SOMPtr;

    [StructLayout(LayoutKind.Sequential)]
    public struct PointWithId
    {
      public float first;
      public float second;
      public int clustId;
      //public float distCent;
      public int imageId;
      public float sigma;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
      public float first;
      public float second;
      public int clustId;
      //public double distCent;
    }

    [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "getSOM", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getSOM(long[] list, int size, double[] probabilities, IntPtr SOMPtr);


    [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "getSOMRepresentants", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getSOMRepresentants(long[] list, ref int size, double[] probabilities, IntPtr SOMPtr, bool mostProbab, long xdim, long ydim, long rlen);

    [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "initSOM", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr initSOM(string path, int datasetSize, int dims);

    private static void InitSOM_()
    {
      if (SOMPtr == default(IntPtr))
      {
        var config = Configuration.GetSourceConfig();
        SOMPtr = initSOM(config.PATH + Path.DirectorySeparatorChar + config.DFEATURES_NAME, VideoData.Data.Length(), 128);
      }
    }

    public static PointWithId[] CreateSOMRepresentants(long[] list, double[] probablities, long xdim, long ydim, long rlen)
    {
      //lock (SOMSync)
      if(probablities == null)
      {
        probablities = new double[list.Length];
        for(int i = 0; i < probablities.Length; i++)
        {
          probablities[i] = 1 / probablities.Length;
        }
      }

      {
        InitSOM_();

        int length = list.Length;
        IntPtr ptr = getSOMRepresentants(list, ref length, probablities, SOMPtr, true, xdim, ydim, rlen);
        MarshalUnmananagedArray2Struct(ptr, length, out PointWithId[] coords);

        return coords;
      }
    }
    public static PointWithId[] CreateWeightedSOMRepresentants(long[] list, double[] probablities, long xdim, long ydim, long rlen)
    {
      //lock (SOMSync)
      if (probablities == null)
      {
        probablities = new double[list.Length];
        for (int i = 0; i < probablities.Length; i++)
        {
          probablities[i] = 1 / probablities.Length;
        }
      }

      {
        InitSOM_();

        int length = list.Length;
        IntPtr ptr = getSOMRepresentants(list, ref length, probablities, SOMPtr, false, xdim, ydim, rlen);
        MarshalUnmananagedArray2Struct(ptr, length, out PointWithId[] coords);

        return coords;
      }
    }

    public static Tuple<long, double, double, long>[] CreateSOM(long[] list)
    {
      //lock (SOMSync)
      {
        InitSOM_();
        Tuple<long, double, double, long>[] result = new Tuple<long, double, double, long>[list.Length];
        IntPtr ptr = getSOM(list, list.Length, null, SOMPtr);
        MarshalUnmananagedArray2Struct<Point>(ptr, list.Length, out Point[] coords);

        for (int i = 0; i < list.Length; i++)
        {
          result[i] = new Tuple<long, double, double, long>(list[i], coords[i].first, coords[i].second, coords[i].clustId);
        }

        return result;
      }
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
