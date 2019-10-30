namespace ViretTool.BusinessLayer.Services
{
    public interface IZoomDisplayProvider
    {
        /// <summary>
        /// Returns the first layer of SOM needed for initialization
        /// </summary>
        /// <returns></returns>
        int[][] GetFirstLayerOfSOM();

        /// <summary>
        /// Finds particular frame in lowest SOM layer (determined by "frameId" parameter) and returns its surounding frames 
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <returns>2D array concatenated into 1D array (dimension is rowCount*columnCount)</returns>
        int[] ZoomIntoLastLayer(int frameId, int rowCount, int columnCount);
    }
}
