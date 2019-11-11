using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.ZoomDisplayIO;

namespace ViretTool.BusinessLayer.Services
{
    public class ZoomDisplayProvider : IZoomDisplayProvider
    {
        private readonly string ZOOM_DISPLAY_FILENAME = "zoomDisplay.txt";

        /// <summary>
        /// Each element of List is a 2D array, each array represents SOM layer
        /// </summary>
        public List<int[][]> LayersIds;


        public ZoomDisplayProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            string filePath = Path.Combine(datasetDirectory, ZOOM_DISPLAY_FILENAME);
            ZoomDisplayReader zoomDisplayReader = new ZoomDisplayReader(filePath);

            // Read the SOM map from file
            LayersIds = zoomDisplayReader.ReadLayersIdsFromFile();
        }


        public int[][] GetInitialLayer()
        {
            return LayersIds[0];
        }
        public int[] ZoomIntoLayer(int layerIndex, int frameId, int rowCount, int columnCount)
        {
            return ZoomIntoLayer(LayersIds[layerIndex], frameId, rowCount, columnCount);
        }
        public int[] ZoomIntoLayer(int[][] layer, int frameId, int rowCount, int columnCount)
        {
            // check display fits layer size
            int layerHeight = layer.Length;
            int layerWidth = layer[0].Length;
            if (rowCount > layerHeight || columnCount > layerWidth)
            {
                throw new ArgumentOutOfRangeException($"Display {columnCount}x{rowCount} is bigger than SOM layer {layerWidth}x{layerHeight}.");
            }

            // positionInArray[XY] is tuple determining position of frameId in last SOM layer
            (int positionInLayerCol, int positionInLayerRow) = GetArrayItemPosition(frameId, layer);

            // get display boundaries in the SOM layer
            (int rowStart, int rowEnd) = ComputeRowBoundaries(layerHeight, positionInLayerRow, rowCount);
            (int colStart, int colEnd) = ComputeColBoundaries(layerWidth, positionInLayerCol, columnCount);

            List<int> resultDisplay = ExtractDisplayItems(layer, rowStart, rowEnd, colStart, colEnd);

            return resultDisplay.ToArray();
        }

        public int[] ZoomIntoLastLayer(int frameId, int rowCount, int columnCount)
        {
            // pointer to last SOM layer
            int[][] lastLayer = LayersIds.Last();

            return ZoomIntoLayer(lastLayer, frameId, rowCount, columnCount);
        }


        private static (int resultCol, int resultRow) GetArrayItemPosition(int arrayItem, int[][] inputArray)
        {
            int resultCol;
            int resultRow;

            // For-loop finds 'frameId' in lowest SOM layer
            for (int iRow = 0; iRow < inputArray.Length; iRow++)
            {
                if ((resultCol = Array.IndexOf(inputArray[iRow], arrayItem)) != -1)
                {
                    resultRow = iRow;
                    return (resultCol, resultRow);
                }
            }

            throw new ArgumentOutOfRangeException($"FrameID: {arrayItem} was not found in the SOM layer.");
        }

        private static (int rowStart, int rowEnd) ComputeRowBoundaries(int layerHeight, int positionInLayerRow, int targetRowCount)
        {
            // normal situation
            int rowStart = Math.Max(0, positionInLayerRow - (targetRowCount / 2) + 1);
            int rowEnd = rowStart + targetRowCount;

            // if the surounding would overstep the SOM layer borders, then we have to adjust the rowStart, rowEnd
            if (rowStart + targetRowCount > layerHeight)
            {
                rowEnd = layerHeight;
                rowStart = layerHeight - targetRowCount;
            }

            return (rowStart, rowEnd);
        }

        private static (int colStart, int colEnd) ComputeColBoundaries(int layerWidth, int positionInLayerCol, int targetColumnCount)
        {
            // normal situation
            int colStart = Math.Max(0, positionInLayerCol - (targetColumnCount / 2) + 1);
            int colEnd = colStart + targetColumnCount;

            // if the surounding would overstep the layer borders, then we have to adjust the X_start,X_end
            if (colStart + targetColumnCount > layerWidth)
            {
                colEnd = layerWidth;
                colStart = Math.Max(0, layerWidth - targetColumnCount);
            }

            return (colStart, colEnd);
        }

        private List<int> ExtractDisplayItems(int[][] layer, int rowStart, int rowEnd, int colStart, int colEnd)
        {
            List<int> resultList = new List<int>();
            for (int iRow = rowStart; iRow < rowEnd; iRow++)
            {
                for (int iCol = colStart; iCol < colEnd; iCol++)
                {
                    resultList.Add(layer[iRow][iCol]);
                }
            }
            return resultList;
        }
    }
}
