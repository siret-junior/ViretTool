namespace ViretTool.BusinessLayer.Services
{
    public interface IZoomDisplayProvider
    {
        /// <summary>
        /// Returns the first layer of SOM needed for initialization
        /// </summary>
        /// <returns></returns>
        int[][] GetInitialLayer();

        /// <summary>
        /// Calculate number of layers
        /// </summary>
        /// <returns></returns>
        int GetMaxDepth();
        (float BottomBorder, float RightBorder) GetColorSimilarity(int layerIndex, int frameIndex);
        int[] ZoomIntoLayer(int layerIndex, int frameId, int rowCount, int columnCount);
        int[] ZoomOutOfLayer(int layerIndex, int frameId, int rowCount, int columnCount);
    }
}
