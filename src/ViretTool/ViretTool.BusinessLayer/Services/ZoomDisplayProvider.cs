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
        public List<float[][]> ColorSimilarity;


        public ZoomDisplayProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            string filePath = Path.Combine(datasetDirectory, ZOOM_DISPLAY_FILENAME);
            ZoomDisplayReader zoomDisplayReader = new ZoomDisplayReader(filePath);

            // Read the SOM map from file
            (LayersIds, ColorSimilarity) = zoomDisplayReader.ReadLayersIdsFromFile();
        }

        public (float BottomBorder,float RightBorder) GetColorSimilarity(int layerIndex, int frameID)
        {
            float[][] layerSimilarities = ColorSimilarity[layerIndex];
            (int Col, int Row) = GetArrayItemPosition(frameID, LayersIds[layerIndex]);
            return (layerSimilarities[Row][Col * 2], layerSimilarities[Row][Col * 2 + 1]);
        }
        public int GetMaxDepth()
        {
            return LayersIds.Count - 1;
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

            // if frame wasn't found
            if(positionInLayerCol == -1)
            {
                throw new ArgumentOutOfRangeException($"FrameID: {frameId} was not found in the SOM layer.");
            }

            // get display boundaries in the SOM layer
            (int rowStart, int rowEnd) = ComputeRowBoundaries(layerHeight, positionInLayerRow, rowCount);
            (int colStart, int colEnd) = ComputeColBoundaries(layerWidth, positionInLayerCol, columnCount);

            List<int> resultDisplay = ExtractDisplayItems(layer, rowStart, rowEnd, colStart, colEnd);

            return resultDisplay.ToArray();
        }

        public int[] ZoomOutOfLayer(int layerIndex, int frameId, int rowCount, int columnCount)
        {

            int [][] higherLayer = LayersIds[layerIndex];
            int higherLayerHeight = higherLayer.Length;
            int higherLayerWidth = higherLayer[0].Length;

            int[][] lowerLayer = LayersIds[layerIndex + 1];
            int lowerLayerHeight = lowerLayer.Length;
            int lowerLayerWidth = lowerLayer[0].Length;


            if (rowCount > higherLayerHeight || columnCount > higherLayerWidth)
            {
                throw new ArgumentOutOfRangeException($"Display {columnCount}x{rowCount} is bigger than SOM layer {higherLayerWidth}x{higherLayerHeight}.");
            }


            (int positionInLayerCol, int positionInLayerRow) = GetArrayItemPosition(frameId, higherLayer);

            // if original frame is not in the higher layer
            if (positionInLayerCol == -1)
            {
                (int centerCol, int centerRow) = GetArrayItemPosition(frameId, lowerLayer);
                for (int distance = 1; distance <= Math.Max(lowerLayerHeight, lowerLayerWidth); distance++)
                {
                    (positionInLayerCol, positionInLayerRow) = SearchVerticalMargin(distance, lowerLayer, higherLayer, centerCol, centerRow);
                    if(positionInLayerCol != -1)
                    {
                        break;
                    }
                    (positionInLayerCol, positionInLayerRow) = SearchHorizontalMargin(distance, lowerLayer, higherLayer, centerCol, centerRow);
                    if (positionInLayerCol != -1)
                    {
                        break;
                    } 
                }
            }

            if (positionInLayerCol == -1)
            {
                throw new ArgumentOutOfRangeException($"FrameID: {frameId} was not found in the higher SOM layer.");
            }

            (int rowStart, int rowEnd) = ComputeRowBoundaries(higherLayerHeight, positionInLayerRow, rowCount);
            (int colStart, int colEnd) = ComputeColBoundaries(higherLayerWidth, positionInLayerCol, columnCount);

            List<int> resultDisplay = ExtractDisplayItems(higherLayer, rowStart, rowEnd, colStart, colEnd);

            return resultDisplay.ToArray();
        }

        private (int positionInLayerCol, int positionInLayerRow) SearchVerticalMargin(int distanceFromCenter, int[][] layer, int[][] higherLayer, int centerCol, int centerRow)
        {
            (int ColumnToBeSearched1, int ColumnToBeSearched2) = (centerCol - distanceFromCenter, centerCol + distanceFromCenter);
            int StartOfColumnIterator = centerRow - distanceFromCenter;
            for(int i = 0; i < 2 * distanceFromCenter + 1; i++)
            {
                int ColumnIterator = StartOfColumnIterator + i;
                if(ColumnIterator >= 0 && ColumnIterator < layer.Length)
                {
                    if(ColumnToBeSearched1 >= 0)
                    {
                        (int resultCol, int resultRow) = GetArrayItemPosition(layer[ColumnIterator][ColumnToBeSearched1], higherLayer);
                        if(resultCol != -1)
                        {
                            return (resultCol, resultRow);
                        }
                    }
                    if (ColumnToBeSearched2 < layer[0].Length)
                    {
                        (int resultCol, int resultRow) = GetArrayItemPosition(layer[ColumnIterator][ColumnToBeSearched2], higherLayer);
                        if (resultCol != -1)
                        {
                            return (resultCol, resultRow);
                        }
                    }
                }
            }
            return (-1, -1);
        }

        private (int positionInLayerCol, int positionInLayerRow) SearchHorizontalMargin(int distanceFromCenter, int[][] layer, int[][] higherLayer, int centerCol, int centerRow)
        {
            (int RowToBeSearched1, int RowToBeSearched2) = (centerRow - distanceFromCenter, centerRow + distanceFromCenter);
            int StartOfRowIterator = centerRow - distanceFromCenter + 1;
            for(int i = 0;i < 2 * (distanceFromCenter - 1) + 1; i++)
            {
                int RowIterator = StartOfRowIterator + i;
                if(RowIterator >= 0 && RowIterator < layer[0].Length)
                {
                    if(RowToBeSearched1 >= 0)
                    {
                        (int resultCol, int resultRow) = GetArrayItemPosition(layer[RowToBeSearched1][RowIterator], higherLayer);
                        if(resultCol != -1)
                        {
                            return (resultCol, resultRow);
                        }
                    }
                    if (RowToBeSearched2 < layer.Length)
                    {
                        (int resultCol, int resultRow) = GetArrayItemPosition(layer[RowToBeSearched2][RowIterator], higherLayer);
                        if (resultCol != -1)
                        {
                            return (resultCol, resultRow);
                        }
                    }
                }
            }
            return (-1, -1);
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

            return (-1, -1);
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
