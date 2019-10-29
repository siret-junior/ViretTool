using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ViretTool.DataLayer.DataIO.ZoomDisplayIO
{
    public class ZoomDisplayReader
    {
        private string _filePath;

        public ZoomDisplayReader(string FilePath)
        {
            _filePath = FilePath;
        }

        /*
         * 
         * Format of zoomDisplay.txt file:
         * 
         * {height} of 1st layer 
         * {width} of 1st layer 
         * 1D array of 1st layer (has to be tranformed to 2D)
         * {height} of 2nd layer
         * {width} of 2nd layer
         * 1D array of 2nd layer (has to be tranformed to 2D)
         * {height} of 3th layer
         * {width} of 3th layer
         * 1D array of 3th layer (has to be tranformed to 2D)
         * etc.
         * 
         */



        /// <summary>
        /// Reads content of each layer from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of 2D arrays, each array represents one layer in SOM map</returns>
        public List<int[][]> ReadLayersIdsFromFile()
        {
            //Read content of textfile to array
            string[] lines = File.ReadAllLines(_filePath);
            List<int[][]> result = new List<int[][]>();
            for (int i = 0; i < lines.Length; i += 3)
            {
                //parse height,width and 1D array and transform it to 2D array, then add it to results
                result.Add(Make2DArray(lines[i + 2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray(), int.Parse(lines[i]), int.Parse(lines[i + 1])));
            }
            return result;
        }
        /// <summary>
        /// Transforms 1D array to 2D array depending on height/width parameters
        /// </summary>
        /// <param name="input"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private int[][] Make2DArray(int[] input, int height, int width)
        {
            int[][] output = new int [height][];
            for (int i = 0; i < height; i++)
            {
                int[] tmp = new int[width];
                for (int j = 0; j < width; j++)
                {
                    tmp[j] = input[i * width + j];
                }
                output[i] = tmp;
            }
            return output;
        }

    }
}
