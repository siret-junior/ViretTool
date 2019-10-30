using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ViretTool.DataLayer.DataIO.ZoomDisplayIO
{
    /// <summary>
    /// 
    /// Format of zoomDisplay.txt file:
    ///
    /// {height} of 1st layer 
    /// {width} of 1st layer 
    /// 1D array of 1st layer (has to be tranformed to 2D)
    /// 
    /// {height} of 2nd layer
    /// {width} of 2nd layer
    /// 1D array of 2nd layer (has to be tranformed to 2D)
    /// 
    /// {height} of 3rd layer
    /// {width} of 3rd layer
    /// 1D array of 3rd layer (has to be tranformed to 2D)
    /// etc.
    /// </summary>
    public class ZoomDisplayReader
    {
        private string _filePath;

        public ZoomDisplayReader(string filePath)
        {
            _filePath = filePath;
        }


        /// <summary>
        /// Reads content of each layer from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of 2D arrays, each array represents one layer in SOM map</returns>
        public List<int[][]> ReadLayersIdsFromFile()
        {
            // Read content of textfile to array
            string[] lines = File.ReadAllLines(_filePath);

            List<int[][]> resultLayers = new List<int[][]>();
            for (int i = 0; i < lines.Length; i += 3)
            {
                // parse height, width and 1D array and transform it to 2D array, then add it to result layers
                int layerHeight = int.Parse(lines[i]);
                int layerWidth = int.Parse(lines[i + 1]);
                int[] layerItems = lines[i + 2].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                
                resultLayers.Add(ReshapeTo2DArray(layerItems, layerHeight, layerWidth));
            }

            return resultLayers;
        }


        /// <summary>
        /// Transforms 1D array to 2D array depending on height/width parameters
        /// </summary>
        /// <param name="input1DArray"></param>
        /// <param name="outputHeight"></param>
        /// <param name="outputWidth"></param>
        /// <returns></returns>
        private int[][] ReshapeTo2DArray(int[] input1DArray, int outputHeight, int outputWidth)
        {
            // argument check
            if (outputWidth * outputHeight != input1DArray.Length)
            {
                throw new InvalidDataException(
                    $"Input array length = {input1DArray.Length}" +
                    $" does not match output 2D layer dimensions {outputWidth}x{outputHeight} = {outputWidth * outputHeight}.");
            }

            // reshaping
            int[][] output2DArray = new int [outputHeight][];
            for (int iCol = 0; iCol < outputHeight; iCol++)
            {
                int[] row = new int[outputWidth];
                for (int iRow = 0; iRow < outputWidth; iRow++)
                {
                    row[iRow] = input1DArray[iCol * outputWidth + iRow];
                }
                output2DArray[iCol] = row;
            }
            return output2DArray;
        }

    }
}
