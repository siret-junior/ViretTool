using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.Core
{
    public static class ArrayExtensions
    {
        public static long[] ToLongArray(this int[] inputArray)
        {
            long[] outputArray = new long[inputArray.Length];
            for (int i = 0; i < inputArray.Length; i++)
            {
                outputArray[i] = inputArray[i];
            }
            return outputArray;
        }

        public static long[] ToLongArray(this IList<int> inputList)
        {
            long[] outputArray = new long[inputList.Count];
            for (int i = 0; i < inputList.Count; i++)
            {
                outputArray[i] = inputList[i];
            }
            return outputArray;
        }

        public static long[] ToLongArray(this List<int> inputList)
        {
            return ((IList<int>)inputList).ToLongArray();
        }

        public static TElement[][] To2DArray<TElement>(this TElement[] source, int nColumns, int nRows)
        {
            // argument check
            if (nColumns * nRows != source.Count())
            {
                throw new InvalidDataException(
                    $"Input length = {source.Count()}" +
                    $" does not match output 2D array dimensions {nColumns}x{nRows} = {nColumns * nRows}.");
            }

            // reshaping
            TElement[][] output2DArray = new TElement[nRows][];
            for (int iCol = 0; iCol < nRows; iCol++)
            {
                TElement[] row = new TElement[nColumns];
                for (int iRow = 0; iRow < nColumns; iRow++)
                {
                    row[iRow] = source[iCol * nColumns + iRow];
                }
                output2DArray[iCol] = row;
            }
            return output2DArray;
        }
    }
}
