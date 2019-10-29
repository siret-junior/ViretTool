using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.ZoomDisplayIO;

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

    public class ZoomDisplayProvider : IZoomDisplayProvider
    {
        private readonly string ZOOM_DISPLAY_FILENAME = "zoomDisplay.txt";

        /// <summary>
        /// Each element of List is a 2D array, each array represents SOM layer
        /// </summary>
        public List<int[][]> LayersIds;


        private ZoomDisplayReader _zoomDisplayReader;

        public ZoomDisplayProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            
            string filePath = Path.Combine(datasetDirectory, ZOOM_DISPLAY_FILENAME);

            
            ZoomDisplayReader zoomDisplayReader = new ZoomDisplayReader(filePath);
            _zoomDisplayReader = zoomDisplayReader;
            //Read the SOM map from file
            LayersIds = _zoomDisplayReader.ReadLayersIdsFromFile();
        }

        public int[][] GetFirstLayerOfSOM()
        {
            return LayersIds[0];
        }
        
        
        public int[] ZoomIntoLastLayer(int frameId, int rowCount, int columnCount)
        {
            //result buffer
            int[] result = new int[rowCount*columnCount]; int resultIterator = 0;
            //pointer to last SOM layer
            int[][] LastLayer = LayersIds.Last();
            //positionInArray[XY] is tuple determining position of frameId in last SOM layer
            int positionInArrayX = 0, positionInArrayY = 0, length = LastLayer.Length; //height of last SOM layer is stored here
            //For-loop finds 'frameId' in last SOM layer
            for (int i = 0; i < length; i++)
            {
                if ((positionInArrayX = Array.IndexOf(LastLayer[i],frameId)) != -1)
                {
                    positionInArrayY = i;
                    break;
                }
            }
            //now we have position of 'frameId' in the layer

            //start of the surounding area in the layer - Y-axis
            int Y_start = Math.Max(0, positionInArrayY - (rowCount / 2)), Y_end;
            //if the surounding would overstep the SOM layer borders, then we have to adjust the Y_start,Y_end
            if (Y_start + rowCount > length)
            {
                Y_end = length;
                Y_start = Math.Max(0, length - rowCount);
            }
            else
                Y_end = Y_start + rowCount;
            length = LastLayer[0].Length; //now it stores WIDTH of last layer (NOT height)
            while(Y_start != Y_end)
            {
                //start of the surounding area in the layer - X-axis
                int X_start = Math.Max(0, positionInArrayX - (columnCount / 2)), X_end;
                //if the surounding would overstep the layer borders, then we have to adjust the X_start,X_end
                if (X_start + columnCount > length)
                {
                    X_end = length;
                    X_start = Math.Max(0, length - columnCount);
                }
                else
                    X_end = X_start + columnCount;
                //move in the current row until edge of surrounding area is reached
                while(X_start != X_end)
                {
                    result[resultIterator] = LastLayer[Y_start][X_start];
                    resultIterator++;
                    X_start++;
                }
                Y_start++;
            }
            return result;
        }
    }
}
