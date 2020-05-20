using System.Collections.Generic;
using ViretTool.BusinessLayer.Descriptors;

namespace ViretTool.BusinessLayer.Services
{
    public interface IZoomDisplayProvider
    {
        /// <summary>
        /// Returns the first layer of SOM needed for initialization
        /// </summary>
        /// <param name="RowCount">row length</param>
        /// <param name="ColumnCount">column length</param>
        /// <returns></returns>
        int[] GetInitialLayer(int RowCount, int ColumnCount);
        /// <summary>
        /// Computes layers of SOM based on list of frame IDs and their descriptors
        /// </summary>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <param name="inputFrameIds">list of frame IDs</param>
        /// <param name="deepFeaturesProvider">Descriptors</param>
        /// <returns></returns>
        int[] GetInitialLayer(int rowCount, int columnCount, IList<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider);
        /// <summary>
        /// Compute number of layers
        /// </summary>
        /// <returns>Returns index of the last layer</returns>
        int GetMaxDepth();
        /// <summary>
        /// Loads border values of a frame at exact layer
        /// </summary>
        /// <param name="layerIndex">Index of layer</param>
        /// <param name="frameIndex">Index of frame</param>
        /// <returns></returns>
        (float BottomBorder, float RightBorder) GetBorderSimilarity(int layerIndex, int frameIndex);
        /// <summary>
        /// Computes borders of current centered frame and its surrounding neighborhoods
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <returns></returns>
        float[] GetBorderSimilarities(int layerIndex, int rowCount, int columnCount);
        /// <summary>
        /// Computes new grid from layer. This grid is specified by the top-left frame of the original grid, then by row length and column length
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="frameId">frameID of the most tope-left frame in grid</param>
        /// <param name="rowCount">Row length</param>
        /// <param name="columnCount">Column length</param>
        /// <returns></returns>
        int[] Resize(int layerIndex, int frameId, int rowCount, int columnCount);
        /// <summary>
        /// Move centered frame down and compute its surrounding neighborhoods
        /// </summary>
        /// <param name="layerIndex">Current layer</param>
        /// <param name="frameId">ID of the frame</param>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <returns></returns>
        int[] MoveDown(int layerIndex, int frameId, int rowCount, int columnCount);
        /// <summary>
        /// Move centered frame up and compute its surrounding neighborhoods
        /// </summary>
        /// <param name="layerIndex">Current layer</param>
        /// <param name="frameId">ID of the frame</param>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <returns></returns>
        int[] MoveUp(int layerIndex, int frameId, int rowCount, int columnCount);
        /// <summary>
        /// Move centered frame left and compute its surrounding neighborhoods
        /// </summary>
        /// <param name="layerIndex">Current layer</param>
        /// <param name="frameId">ID of the frame</param>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <returns></returns>
        int[] MoveLeft(int layerIndex, int frameId, int rowCount, int columnCount);
        /// <summary>
        /// Move centered frame right and compute its surrounding neighborhoods
        /// </summary>
        /// <param name="layerIndex">Current layer</param>
        /// <param name="frameId">ID of the frame</param>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <returns></returns>
        int[] MoveRight(int layerIndex, int frameId, int rowCount, int columnCount);
        /// <summary>
        /// Zoom into specified layer and center to specified frame
        /// </summary>
        /// <param name="layerIndex">ID of layer</param>
        /// <param name="frameId">ID of frame</param>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <returns></returns>
        int[] ZoomIntoLayer(int layerIndex, int frameId, int rowCount, int columnCount);
        /// <summary>
        /// Zoom out to specified layer and center to specified frame
        /// </summary>
        /// <param name="layerIndex">ID of layer</param>
        /// <param name="frameId">ID of frame</param>
        /// <param name="rowCount">row length</param>
        /// <param name="columnCount">column length</param>
        /// <returns></returns>
        int[] ZoomOutOfLayer(int layerIndex, int frameId, int rowCount, int columnCount);
        /// <summary>
        /// Computes smaller grid than rowCount x columnCount (in case user have bigger screen then we have to adjust the Width and height).
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        (int[] Array, int Width, int Height) GetSmallLayer(int layerIndex, int rowCount, int columnCount);
    }
}
