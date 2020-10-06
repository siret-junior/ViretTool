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
        /// <summary>
        /// Creates SOM from input data. 
        /// </summary>
        /// <param name="data">deep features of frames in 1D array. Array has to be sorted as input points (if dimension is equal to 128, then 0-127 is deep feature of first frame in input_points, 128-255 is deep feature of second frame, etc.)</param>
        /// <param name="datasetSize">number of frames</param>
        /// <param name="dims">dimension of deep features</param>
        /// <param name="xdim">SOM width</param>
        /// <param name="ydim">SOM height</param>
        /// <param name="rlen">Number of learning epochs</param>
        /// <param name="input_points">IDs of each frame</param>
        /// <returns></returns>
        [DllImport("CEmbedSomDLL.dll", EntryPoint = "GetRepresentants", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetRepresentants(float[] data, int datasetSize, int dims, int xdim, int ydim, int rlen, int[] input_points);
        

        /// <summary>
        /// Dealocates array
        /// </summary>
        /// <param name="toDel"></param>
        [DllImport("CEmbedSomDLL.dll", EntryPoint = "deleteArray", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deleteArray(IntPtr toDel);

        public static int[] GetSomRepresentants(float[] data, int datasetSize, int dims, int xdim, int ydim, int rlen, int[] input_points)
        {
            IntPtr ptr = GetRepresentants(data, datasetSize, dims, xdim, ydim, rlen, input_points);
            
            int[] result = new int[datasetSize];
            Marshal.Copy(ptr, result, 0, datasetSize);
            
            deleteArray(ptr);
            return result;
        }

    }
}
