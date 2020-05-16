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
        
        [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "GetRepresentants")]
        public static extern IntPtr GetRepresentants(float[] data, int datasetSize, int dims, int xdim, int ydim, int rlen, int[] input_points);
        
        [DllImport(@"CEmbedSomDLL.dll", EntryPoint = "deleteArray", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deleteArray(IntPtr toDel);

        public static int[] GetSomRepresentants(float[] data, int datasetSize, int dims, int xdim, int ydim, int rlen, int[] input_points)
        {
            IntPtr ptr = GetRepresentants(data, datasetSize, dims, xdim, ydim, 15, input_points);
            
            int[] result = new int[datasetSize];
            Marshal.Copy(ptr, result, 0, datasetSize);
            deleteArray(ptr);
            return result;
        }

    }
}
