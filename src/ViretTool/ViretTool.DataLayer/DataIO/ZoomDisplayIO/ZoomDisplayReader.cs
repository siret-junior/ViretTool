using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;
using ViretTool.Core;

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
        /// <returns>two Lists of 2D arrays, each array in first list represents one layer in SOM map, 
        /// each array in second list represents right & bottom similarity with surrounding elements.</returns>
        public (List<int[][]>, List<float[][]>) ReadLayersIdsFromFile()
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

                resultLayers.Add(layerItems.As2DArray(layerWidth, layerHeight));
                colorSimilarity.Add(layerSimilarities.As2DArray(layerWidth * 2, layerHeight));
            }

            return (resultLayers, colorSimilarity);
        }

    }
}
