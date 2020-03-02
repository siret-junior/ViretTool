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

        public static TElement[][] As2DArray<TElement>(this TElement[] source, int nColumns, int nRows)
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
