using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;

namespace ViretTool.DataLayer.DataIO.ZoomDisplayIO
{
    /// <summary>
    /// 
    /// Format of zoomDisplay.txt file:
    ///
    /// {height} of 1st layer 
    /// {width} of 1st layer 
    /// 1D array of 1st layer (has to be tranformed to 2D)
    /// 1D float array of similarities between bottom element and right element of 1st layer (bottom is first, right is second)
    /// 
    /// {height} of 2nd layer
    /// {width} of 2nd layer
    /// 1D array of 2nd layer (has to be tranformed to 2D)
    /// 1D float array of similarities between bottom element and right element of 2nd layer (bottom is first, right is second)
    /// 
    /// {height} of 3rd layer
    /// {width} of 3rd layer
    /// 1D array of 3rd layer (has to be tranformed to 2D)
    /// 1D float array of similarities between bottom element and right element of 3rd layer (bottom is first, right is second)
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
        /// <returns>two Lists of 2D arrays, each array in first list represents one layer in SOM map, each array in second list represents right & bottom similarity with surrounding elements, returns </returns>
        public (List<int[][]>,List<float[][]>) ReadLayersIdsFromFile()
        {
            // Read content of textfile to array
            string[] lines = null;

            List<int[][]> resultLayers = new List<int[][]>();
            List<float[][]> colorSimilarity = new List<float[][]>();

            try
            {
                lines = File.ReadAllLines(_filePath);
            }
            catch(IOException)
            {
                return (resultLayers, colorSimilarity);
            }

            for (int i = 0; i < lines.Length; i += 4)
            {
                // parse height, width, 1D array (of indexes) and 1D array (of similarities) and transform them to 2D arrays, then add them to lists
                int layerHeight = int.Parse(lines[i]);
                int layerWidth = int.Parse(lines[i + 1]);
                int[] layerItems = lines[i + 2].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                float[] layerSimilarities = lines[i + 3].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToArray();

                resultLayers.Add(ReshapeTo2DArray(layerItems, layerHeight, layerWidth));
                colorSimilarity.Add(ReshapeTo2DArray(layerSimilarities, layerHeight, layerWidth * 2));
            }

            return (resultLayers,colorSimilarity);
        }


        /// <summary>
        /// Transforms 1D array to 2D array depending on height/width parameters
        /// </summary>
        /// <param name="input1DArray"></param>
        /// <param name="outputHeight"></param>
        /// <param name="outputWidth"></param>
        /// <returns></returns>
        private T[][] ReshapeTo2DArray<T>(T[] input1DArray, int outputHeight, int outputWidth)
        {
            // argument check
            if (outputWidth * outputHeight != input1DArray.Length)
            {
                throw new InvalidDataException(
                    $"Input array length = {input1DArray.Length}" +
                    $" does not match output 2D layer dimensions {outputWidth}x{outputHeight} = {outputWidth * outputHeight}.");
            }

            // reshaping
            T[][] output2DArray = new T [outputHeight][];
            for (int iCol = 0; iCol < outputHeight; iCol++)
            {
                T[] row = new T[outputWidth];
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
