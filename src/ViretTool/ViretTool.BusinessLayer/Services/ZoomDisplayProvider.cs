using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.DataLayer.DataIO.ZoomDisplayIO;

namespace ViretTool.BusinessLayer.Services
{
    public class ZoomDisplayProvider : IZoomDisplayProvider
    {
        private const string ZOOM_DISPLAY_FILENAME = "zoomDisplay.txt";

        /// <summary>
        /// Each element of List is a 2D array, each array represents SOM layer
        /// </summary>
        public List<int[][]> LayersIds;

        /// <summary>
        /// Each element of List is a 2D float array, each array represents border values between each neighbour
        /// </summary>
        public List<float[][]> BorderSimilarities;

        /// <summary>
        /// position of current centered frame
        /// </summary>
        private int _centerPositionInLayerCol;
        /// <summary>
        /// position of current centered frame
        /// </summary>
        private int _centerPositionInLayerRow;
        public ZoomDisplayProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            string filePath = Path.Combine(datasetDirectory, ZOOM_DISPLAY_FILENAME);
            ZoomDisplayReader zoomDisplayReader = new ZoomDisplayReader(filePath);

            // Read the SOM map from file
            (LayersIds, BorderSimilarities) = zoomDisplayReader.ReadLayersIdsFromFile();
        }


        public (float BottomBorder, float RightBorder) GetBorderSimilarity(int layerIndex, int frameID)
        {
            float[][] layerSimilarities = BorderSimilarities[layerIndex];
            (int Col, int Row) = GetArrayItemPosition(frameID, LayersIds[layerIndex]);
            return (layerSimilarities[Row][Col * 2], layerSimilarities[Row][Col * 2 + 1]);
        }
        public float[] GetBorderSimilarities(int layerIndex, int rowCount, int columnCount)
        {

            float[][] layer = BorderSimilarities[layerIndex];
            int layerHeight = layer.Length;
            int layerWidth = layer[0].Length;

            // check if row/column length is not bigger than layer
            if (rowCount > layerHeight || columnCount * 2 > layerWidth)
            {
                throw new ArgumentOutOfRangeException($"Display {columnCount}x{rowCount} is bigger than SOM layer {layerWidth}x{layerHeight}.");
            }

            // Compute boundaries
            (int rowStart, int rowEnd) = ComputeRowBoundaries(layerHeight, _centerPositionInLayerRow, rowCount);
            (int colStart, int colEnd) = ComputeColBoundaries(layerWidth, _centerPositionInLayerCol, columnCount);

            // Extract items from boundaries
            List<float> borders = ExtractDisplayItems(layer, rowStart, rowEnd, colStart*2, colEnd*2);

            return borders.ToArray();
        }
        public int GetMaxDepth()
        {
            return LayersIds.Count - 1;
        }
        public virtual int[] GetInitialLayer(int rowCount, int columnCount, IList<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            return GetInitialLayer(rowCount, columnCount);
        }

        public int[] GetInitialLayer(int rowCount, int columnCount)
        {
            // if any IO exception occured while reading file, then LayersIds could be null
            if (LayersIds.Count() > 0)
            {
                // Zoom into first layer at top-left item
                return ZoomIntoLayer(0, LayersIds[0][0][0], rowCount, columnCount);
            }
            return null;
        }

        public (int[] Array, int Width, int Height) GetSmallLayer(int layerIndex, int rowCount, int columnCount)
        {
            int layerHeight = LayersIds[layerIndex].Length;
            int layerWidth = LayersIds[layerIndex][0].Length;

            int Height = Math.Min(rowCount, layerHeight);
            int Width = Math.Min(columnCount, layerWidth);

            int[] Array = LayersIds[layerIndex].Take(layerHeight).SelectMany(x => x.Take(Width).ToArray()).ToArray();
            return (Array, Width, Height);
        }
        public int[] ZoomIntoLayer(int layerIndex, int frameId, int rowCount, int columnCount)
        {
            // if any IO exception occured while reading file, then LayersIds could be null
            if (layerIndex < LayersIds.Count())
            {
                return ZoomIntoLayer(LayersIds[layerIndex], frameId, rowCount, columnCount);
            }
            return null;
        }
        public int[] MoveLeft(int layerIndex, int frameId, int rowCount, int columnCount)
        {
            if (layerIndex < LayersIds.Count())
            {
                if(--_centerPositionInLayerCol < 0)
                {
                    _centerPositionInLayerCol = LayersIds[layerIndex][0].Length - 1;
                }
                return ComputeSectorAfterMoving(LayersIds[layerIndex], rowCount, columnCount);
            }
            return null;
        }
        public int[] MoveRight(int layerIndex, int frameId, int rowCount, int columnCount)
        {
            if (layerIndex < LayersIds.Count())
            {
                if (++_centerPositionInLayerCol >= LayersIds[layerIndex][0].Length)
                {
                    _centerPositionInLayerCol = 0;
                }
                return ComputeSectorAfterMoving(LayersIds[layerIndex], rowCount, columnCount);
            }
            return null;
        }
        public int[] MoveUp(int layerIndex, int frameId, int rowCount, int columnCount)
        {
            if (layerIndex < LayersIds.Count())
            {
                if (--_centerPositionInLayerRow < 0)
                {
                    _centerPositionInLayerRow = LayersIds[layerIndex].Length - 1;
                }
                return ComputeSectorAfterMoving(LayersIds[layerIndex], rowCount, columnCount);
            }
            return null;
        }
        public int[] MoveDown(int layerIndex, int frameId, int rowCount, int columnCount)
        {
            if (layerIndex < LayersIds.Count())
            {
                if (++_centerPositionInLayerRow >= LayersIds[layerIndex].Length)
                {
                    _centerPositionInLayerRow = 0;
                }
                return ComputeSectorAfterMoving(LayersIds[layerIndex], rowCount, columnCount);
            }
            return null;
        }
        private int[] ComputeSectorAfterMoving(int[][] layer, int rowCount, int columnCount)
        {
            if (layer.Length != 0)
            {

                // check display fits layer size
                int layerHeight = layer.Length;
                int layerWidth = layer[0].Length;
                if (rowCount > layerHeight || columnCount > layerWidth)
                {
                    throw new ArgumentOutOfRangeException($"Display {columnCount}x{rowCount} is bigger than SOM layer {layerWidth}x{layerHeight}.");
                }

                // Compute boundaries
                (int rowStart, int rowEnd) = ComputeRowBoundaries(layerHeight, _centerPositionInLayerRow, rowCount);
                (int colStart, int colEnd) = ComputeColBoundaries(layerWidth, _centerPositionInLayerCol, columnCount);

                List<int> resultDisplay = ExtractDisplayItems(layer, rowStart, rowEnd, colStart, colEnd);

                return resultDisplay.ToArray();
            }
            return null;
        }
        public int[] ZoomIntoLayer(int[][] layer, int frameId, int rowCount, int columnCount)
        {
            // if any IO exception occured while reading file, then LayersIds could be null
            if (layer.Length != 0)
            {
                // check display fits layer size
                int layerHeight = layer.Length;
                int layerWidth = layer[0].Length;
                if (rowCount > layerHeight || columnCount > layerWidth)
                {
                    throw new ArgumentOutOfRangeException($"Display {columnCount}x{rowCount} is bigger than SOM layer {layerWidth}x{layerHeight}.");
                }

                // (positionInLayerCol, positionInLayerRow) is tuple determining position of frameId in SOM layer
                (int positionInLayerCol, int positionInLayerRow) = GetArrayItemPosition(frameId, layer);

                // if frame wasn't found
                if (positionInLayerCol == -1)
                {
                    throw new ArgumentOutOfRangeException($"FrameID: {frameId} was not found in the SOM layer.");
                }
                (_centerPositionInLayerCol, _centerPositionInLayerRow) = (positionInLayerCol, positionInLayerRow);
                
                // get display boundaries in the SOM layer
                (int rowStart, int rowEnd) = ComputeRowBoundaries(layerHeight, positionInLayerRow, rowCount);
                (int colStart, int colEnd) = ComputeColBoundaries(layerWidth, positionInLayerCol, columnCount);

                List<int> resultDisplay = ExtractDisplayItems(layer, rowStart, rowEnd, colStart, colEnd);

                return resultDisplay.ToArray();
            }
            else return null;
        }

        public int[] Resize(int layerIndex, int frameId, int rowCount, int columnCount)
        {
            // if any IO exception occured while reading file, then LayersIds could be null
            if (layerIndex < LayersIds.Count())
            {

                int layerHeight = LayersIds[layerIndex].Length;
                int layerWidth = LayersIds[layerIndex][0].Length;
                if (rowCount > layerHeight || columnCount > layerWidth)
                {
                    throw new ArgumentOutOfRangeException($"Display {columnCount}x{rowCount} is bigger than SOM layer {layerWidth}x{layerHeight}.");
                }

                // Compute top-left position of frame
                (int colStart, int rowStart) = GetArrayItemPosition(frameId, LayersIds[layerIndex]);

                // if frame wasn't found
                if (colStart == -1 || rowStart == -1)
                {
                    throw new ArgumentOutOfRangeException($"FrameID: {frameId} was not found in the SOM layer.");
                }
               
                // Compute boundaries
                int rowEnd = rowStart + rowCount;
                
                int columnEnd = colStart + columnCount;
                
                // adjust current centered position
                _centerPositionInLayerCol = colStart + columnCount / 2 - 1;
                _centerPositionInLayerRow = rowStart + rowCount / 2 - 1;
                List<int> resultDisplay = ExtractDisplayItems(LayersIds[layerIndex], rowStart, rowEnd, colStart, columnEnd);

                return resultDisplay.ToArray();

            }
            else return null;
        }
        public int[] ZoomOutOfLayer(int layerIndex, int frameId, int rowCount, int columnCount)
        {

            int[][] higherLayer = LayersIds[layerIndex];
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

                // Breadth-first search until we find item that is present in both lower and higher layer
                for (int distance = 1; distance <= Math.Max(lowerLayerHeight, lowerLayerWidth); distance++)
                {

                    (positionInLayerCol, positionInLayerRow) = SearchVerticalMargin(distance, lowerLayer, higherLayer, centerCol, centerRow);
                    if (positionInLayerCol != -1)
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

            (_centerPositionInLayerCol, _centerPositionInLayerRow) = (positionInLayerCol, positionInLayerRow);


            // Compute boundaries
            (int rowStart, int rowEnd) = ComputeRowBoundaries(higherLayerHeight, positionInLayerRow, rowCount);
            (int colStart, int colEnd) = ComputeColBoundaries(higherLayerWidth, positionInLayerCol, columnCount);

            List<int> resultDisplay = ExtractDisplayItems(higherLayer, rowStart, rowEnd, colStart, colEnd);

            return resultDisplay.ToArray();
        }

        /// <summary>
        /// Searches vertical margin in Breadth-first search
        /// </summary>
        /// <param name="distanceFromCenter"></param>
        /// <param name="layer"></param>
        /// <param name="higherLayer"></param>
        /// <param name="centerCol">position of original frame</param>
        /// <param name="centerRow">position of original frame</param>
        /// <returns></returns>
        private (int positionInLayerCol, int positionInLayerRow) SearchVerticalMargin(int distanceFromCenter, int[][] layer, int[][] higherLayer, int centerCol, int centerRow)
        {
            (int ColumnToBeSearched1, int ColumnToBeSearched2) = (centerCol - distanceFromCenter, centerCol + distanceFromCenter);
            int StartOfColumnIterator = centerRow - distanceFromCenter;

            // search both colums parallely
            for (int i = 0; i < 2 * distanceFromCenter + 1; i++)
            {
                int ColumnIterator = StartOfColumnIterator + i;
                if (ColumnIterator >= 0 && ColumnIterator < layer.Length)
                {
                    if (ColumnToBeSearched1 >= 0)
                    {
                        (int resultCol, int resultRow) = GetArrayItemPosition(layer[ColumnIterator][ColumnToBeSearched1], higherLayer);
                        if (resultCol != -1)
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

        /// <summary>
        /// Searches horizontal margin in Breadth-first search
        /// </summary>
        /// <param name="distanceFromCenter"></param>
        /// <param name="layer"></param>
        /// <param name="higherLayer"></param>
        /// <param name="centerCol">position of original frame</param>
        /// <param name="centerRow">position of original frame</param>
        /// <returns></returns>
        private (int positionInLayerCol, int positionInLayerRow) SearchHorizontalMargin(int distanceFromCenter, int[][] layer, int[][] higherLayer, int centerCol, int centerRow)
        {
            // TODO: review unused parameter centerCol
            (int RowToBeSearched1, int RowToBeSearched2) = (centerRow - distanceFromCenter, centerRow + distanceFromCenter);
            int StartOfRowIterator = centerRow - distanceFromCenter + 1;

            // search both colums parallely
            for (int i = 0; i < 2 * (distanceFromCenter - 1) + 1; i++)
            {
                int RowIterator = StartOfRowIterator + i;
                if (RowIterator >= 0 && RowIterator < layer[0].Length)
                {
                    if (RowToBeSearched1 >= 0)
                    {
                        (int resultCol, int resultRow) = GetArrayItemPosition(layer[RowToBeSearched1][RowIterator], higherLayer);
                        if (resultCol != -1)
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
        /// <summary>
        /// Gets position of specified item in 2D array
        /// </summary>
        /// <param name="arrayItem"></param>
        /// <param name="inputArray"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Computes row boundaries of frame neighbours
        /// </summary>
        /// <param name="layerHeight"></param>
        /// <param name="positionInLayerRow"></param>
        /// <param name="targetRowCount"></param>
        /// <returns></returns>
        private static (int rowStart, int rowEnd) ComputeRowBoundaries(int layerHeight, int positionInLayerRow, int targetRowCount)
        {
            // TODO: remove layerHeight parameter or check whether it's required for boundary checking
            int rowStart = positionInLayerRow - (targetRowCount / 2) + 1;
            int rowEnd = rowStart + targetRowCount;

            return (rowStart, rowEnd);
        }

        /// <summary>
        /// Computes column boundaries of frame neighbours
        /// </summary>
        /// <param name="layerHeight"></param>
        /// <param name="positionInLayerRow"></param>
        /// <param name="targetRowCount"></param>
        /// <returns></returns>
        private static (int colStart, int colEnd) ComputeColBoundaries(int layerWidth, int positionInLayerCol, int targetColumnCount)
        {
            // TODO: remove layerWidth parameter or check whether it's required for boundary checking
            int colStart = positionInLayerCol - (targetColumnCount / 2) + 1;
            int colEnd = colStart + targetColumnCount;

            return (colStart, colEnd);
        }


        /// <summary>
        /// Extracts items from layer. Items are specified by row/column boundaries
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layer"></param>
        /// <param name="rowStart"></param>
        /// <param name="rowEnd"></param>
        /// <param name="colStart"></param>
        /// <param name="colEnd"></param>
        /// <returns></returns>
        private List<T> ExtractDisplayItems<T>(T[][] layer, int rowStart, int rowEnd, int colStart, int colEnd)
        {
            List<T> resultList = new List<T>();
            for (int iRow = rowStart; iRow < rowEnd; iRow++)
            {
                int i;
                // resolve wrap around the edges (2D torus)
                if (iRow < 0)
                {
                    i = iRow + layer.Length;
                }
                else if (iRow < layer.Length)
                {
                    i = iRow;
                }
                else
                {
                    i = iRow - layer.Length;
                }
                for (int iCol = colStart; iCol < colEnd; iCol++)
                {
                    int j;
                    // resolve wrap around the edges (2D torus)
                    if (iCol < 0)
                    {
                        j = iCol + layer[i].Length;
                    }
                    else if (iCol < layer[i].Length)
                    {
                        j = iCol;
                    }
                    else
                    {
                        j = iCol - layer[i].Length;
                    }
                    resultList.Add(layer[i][j]);
                }
            }
            return resultList;
        }
    }
}
