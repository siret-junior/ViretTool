using System.Collections.Generic;
using ViretTool.BusinessLayer.Descriptors;

namespace ViretTool.BusinessLayer.Services
{
    public interface IZoomDisplayProvider
    {
        /// <summary>
        /// Returns the first layer of SOM needed for initialization
        /// </summary>
        /// <returns></returns>
        int[] GetInitialLayer(int RowCount, int ColumnCount);
        int[] GetInitialLayer(int rowCount, int columnCount, IEnumerable<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider);
        /// <summary>
        /// Calculate number of layers
        /// </summary>
        /// <returns>Returns index of the last layer</returns>
        int GetMaxDepth();
        (float BottomBorder, float RightBorder) GetColorSimilarity(int layerIndex, int frameIndex);
        /// <summary>
        /// Calculates grid from layer. This grid is specified by the most top-left frame of the grid, then by row length and column length
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="frameId">frameID of the most tope-left frame in grid</param>
        /// <param name="rowCount">Row length</param>
        /// <param name="columnCount">Column length</param>
        /// <returns></returns>
        int[] Resize(int layerIndex, int frameId, int rowCount, int columnCount);
        int[] MoveDown(int layerIndex, int frameId, int rowCount, int columnCount);
        int[] MoveUp(int layerIndex, int frameId, int rowCount, int columnCount);
        int[] MoveLeft(int layerIndex, int frameId, int rowCount, int columnCount);
        int[] MoveRight(int layerIndex, int frameId, int rowCount, int columnCount);
        int[] ZoomIntoLayer(int layerIndex, int frameId, int rowCount, int columnCount);
        int[] ZoomOutOfLayer(int layerIndex, int frameId, int rowCount, int columnCount);
        (int[] Array, int Width, int Height) GetSmallLayer(int layerIndex, int rowCount, int columnCount);
    }
}
