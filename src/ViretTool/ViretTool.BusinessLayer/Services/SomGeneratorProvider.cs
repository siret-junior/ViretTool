using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.SOMGridSorting;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.DataLayer.DataIO.ZoomDisplayIO;
using ViretTool.Core;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ViretTool.BusinessLayer.Services
{
    public class SomGeneratorProvider : ZoomDisplayProvider
    {
        IDatasetServicesManager _datasetServicesManager;
        
        public SomGeneratorProvider(IDatasetServicesManager datasetServicesManager, string datasetDirectory) 
            : base(datasetServicesManager, datasetDirectory)
        {
            // TODO: set SOM initialization here whenever dataset is loaded (using datasetServicesManager.DatasetOpened event).
            // TODO: access IDescriptorProvider<float[]> through passed datasetServicesManager
            _datasetServicesManager = datasetServicesManager;
        }
        private const int BASE_WIDTH = 20;
        private const int BASE_HEIGHT = 20;

        private const int DISPLAY_WIDTH_TOLERANCE = 2;
        private const int DISPLAY_HEIGHT_TOLERANCE = 2;

        private const int SOM_EPOCHS = 20;
        private const int DECREASE_BASE_THRESHOLD = 100;

        private Random _random = new Random();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override int[] GetInitialLayer(int rowCount, int columnCount, IList<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            // compute output size
            int outputWidth = BASE_WIDTH;
            int outputHeight = BASE_HEIGHT;
            int outputSize = outputWidth * outputHeight;

            int[] inputFrameIdsArray;
            if (inputFrameIds.Count < DECREASE_BASE_THRESHOLD)
            {
                // set SOM to display dimensions
                outputWidth = columnCount;
                outputHeight = rowCount;
                outputSize = outputWidth * outputHeight;
            }

            // preprocess input frames
            if (inputFrameIds.Count >= outputSize)
            {
                inputFrameIdsArray = inputFrameIds.Take(outputSize).ToArray();
                //inputFrameIdsArray = inputFrameIds.ToArray();
                //Array.Resize(ref inputFrameIdsArray, outputSize);
            }
            else
            {
                inputFrameIdsArray = ExtendBySampledFrames(inputFrameIds.ToList(), outputSize).ToArray();
                //int[] tmp = ExtendBySampledFrames(inputFrameIds.ToList(), outputSize).ToArray();
                //inputFrameIdsArray = new int[tmp.Length];
                //Array.Copy(tmp, inputFrameIdsArray, tmp.Length);
            }

            return GetInitialLayerUnconstrained(rowCount, columnCount, outputSize, outputWidth, outputHeight, inputFrameIdsArray, deepFeaturesProvider);
        }

        // TODO: refactor, this is just a hotfix
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override int[] GetInitialLayerUnconstrained(int displayHeight, int displayWidth, int outputSize, int outputWidth, int outputHeight, int[] inputFrameIdsArray, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            (float[] framesData, int dimension) = ExtractDataFromSemanticVectors(inputFrameIdsArray, deepFeaturesProvider);

            int[] somResult1D = SOMWrapper.GetSomRepresentants(framesData, outputSize, dimension, outputWidth, outputHeight, SOM_EPOCHS, inputFrameIdsArray);


            int[][] somBaseLayer = somResult1D.To2DArray(outputWidth, outputHeight);

            // Compute border similarities for base layer
            float[][] somBaseBorderSimilarities = ComputeBorderSimilarities(somBaseLayer, deepFeaturesProvider);

            // compute the additional layer sizes based on the base layer dimensions
            List<(int layerWidth, int layerHeight)> additionalLayerSizes = new List<(int layerWidth, int layerHeight)>();
            if (outputWidth > displayWidth || outputHeight > displayHeight)
            {
                additionalLayerSizes = ComputeAdditionalLayerSizesBottomUp(outputWidth, outputHeight, displayWidth, displayHeight);
            }

            // compute additional layers from bottom up to fulfill predicate
            // that each layer above is a subset of the layer below
            Stack<int[][]> layerStack = new Stack<int[][]>();
            Stack<float[][]> borderStack = new Stack<float[][]>();

            // add base layer to stack
            layerStack.Push(somBaseLayer);
            borderStack.Push(somBaseBorderSimilarities);

            (int frameIDs, int ranks)[][] rankedSomBaseLayer = assignRankToEachElement(somBaseLayer, inputFrameIdsArray);

            foreach ((int layerWidth, int layerHeight) in additionalLayerSizes)
            {
                int[][] layer = SubsampleLayer(rankedSomBaseLayer, layerWidth, layerHeight);
                layerStack.Push(layer);
                borderStack.Push(ComputeBorderSimilarities(layer, deepFeaturesProvider));
            }

            // store layers
            LayersIds.Clear();
            BorderSimilarities.Clear();
            while (layerStack.Count > 0)
            {
                LayersIds.Add(layerStack.Pop());
                BorderSimilarities.Add(borderStack.Pop());
            }

            // TODO: consider using a single structure for both: frameIds and also its borders

            // zoom into base layer
            return ZoomIntoLayer(0, LayersIds[0][0][0], displayHeight, displayWidth);
        }

        /// <summary>
        /// Duplicate array elements to get particular length of array
        /// </summary>
        /// <param name="originalInputFrameIds"></param>
        /// <param name="intendedSize"></param>
        private List<int> ExtendBySampledFrames(List<int> inputList, int intendedSize)
        {
            // empty input list fallback
            if (inputList.Count == 0)
            {
                return Enumerable.Repeat(0, intendedSize).ToList();
            }

            // sample randomly
            int nMissingFrames = intendedSize - inputList.Count;
            List<int> randomSamples = new List<int>();
            while (randomSamples.Count < nMissingFrames)
            {
                randomSamples.AddRange(inputList
                    .OrderBy(x => _random.Next())
                    .Take(nMissingFrames - randomSamples.Count)
                );
            }

            // extend the input list
            inputList.AddRange(randomSamples);
            return inputList;
        }

        /// <summary>
        /// Computes sizes of upper layers
        /// </summary>
        /// <param name="baseLayerWidth"></param>
        /// <param name="baseLayerHeight"></param>
        /// <returns></returns>
        private List<(int layerWidth, int layerHeight)> ComputeAdditionalLayerSizesBottomUp(
            int baseLayerWidth, int baseLayerHeight, 
            int outputWidth, int outputHeight)
        {
            List<(int layerWidth, int layerHeight)> result = new List<(int layerWidth, int layerHeight)>();

            // add intermediate layers
            int layerWidth = baseLayerWidth / 2;
            int layerHeight = baseLayerHeight / 2;
            while (layerWidth > outputWidth + DISPLAY_WIDTH_TOLERANCE 
                && layerHeight > outputHeight + DISPLAY_HEIGHT_TOLERANCE)
            {
                result.Add((outputWidth, outputHeight));

                layerWidth /= 2;
                layerHeight /= 2;
            }

            // add the top layer matching display size
            result.Add((outputWidth, outputHeight));

            return result;
        }


        /// <summary>
        /// Creates new layer based on original layer 
        /// </summary>
        /// <param name="somBaseLayer">original layer</param>
        /// <param name="layerWidth">new layer width</param>
        /// <param name="layerHeight">new layer height</param>
        /// <returns></returns>
        private int[][] SubsampleLayer((int frameIDs, int ranks)[][] somBaseLayer, int layerWidth, int layerHeight)
        {
            if (layerWidth >= somBaseLayer[0].Length || layerHeight >= somBaseLayer.Length)
            {
                throw new ArgumentOutOfRangeException($"Trying to supersample in a subsampling method " 
                    + $"([{somBaseLayer[0].Length}, {somBaseLayer.Length}] layer, [{layerWidth}, {layerHeight}] subsampling).");
            }

            // how big radius should we search in base layer to get one element in sublayer
            int heightRadius = somBaseLayer.Length / layerHeight;
            int widthRadius = somBaseLayer[0].Length / layerWidth;

            // result
            int[][] subLayer = new int[layerHeight][];


            // Calculate each element of subLayer
            for(int iRow = 0; iRow < layerHeight; iRow++)
            {
                int[] subLayerRow = new int[layerWidth];

                for(int iCol = 0; iCol < layerWidth; iCol++)
                {
                    // Calculate element at (iRow, iCol) index
                    subLayerRow[iCol] = GetElementOfSubLayer(somBaseLayer, heightRadius, widthRadius, iRow * heightRadius, iCol * widthRadius);
                }

                subLayer[iRow] = subLayerRow;
            }

            return subLayer;

        }
        /// <summary>
        /// Search for top-ranked element in particular radius inside of base layer
        /// </summary>
        /// <param name="somBaseLayer"></param>
        /// <param name="heightRadius"></param>
        /// <param name="widthRadius"></param>
        /// <param name="rowStart"></param>
        /// <param name="colStart"></param>
        /// <returns></returns>
        private int GetElementOfSubLayer((int frameIDs, int ranks)[][] somBaseLayer, int heightRadius, int widthRadius, int rowStart, int colStart)
        {
            (int rowResult, int colResult) = (rowStart, colStart);
            int resultRank = Int32.MaxValue;


            for(int iRow = rowStart; iRow < rowStart + heightRadius; iRow++)
            {
                for(int iCol = colStart; iCol < colStart + widthRadius; iCol++)
                {
                    if(somBaseLayer[iRow][iCol].Item2 < resultRank)
                    {
                        rowResult = iRow;
                        colResult = iCol;
                        resultRank = somBaseLayer[iRow][iCol].ranks;
                    }
                }
            }
            return somBaseLayer[rowResult][colResult].frameIDs;
        }

        /// <summary>
        /// Assigns rank to each element in SOM base layer
        /// </summary>
        /// <param name="somBaseLayer"></param>
        /// <param name="inputFrameIdsArray">Ranks from ranking pipeline</param>
        /// <returns></returns>
        private (int frameIDs, int ranks)[][] assignRankToEachElement(int[][] somBaseLayer, int[] inputFrameIdsArray)
        {
            (int frameIDs, int ranks)[][] result = new (int frameIDs, int ranks)[somBaseLayer.Length][];


            for (int iRow = 0; iRow < somBaseLayer.Length; iRow ++)
            {
                (int frameIDs, int ranks)[] rowResult = new (int frameIDs, int ranks)[somBaseLayer[0].Length];


                for (int iCol = 0; iCol < somBaseLayer[0].Length; iCol++)
                {
                    int frameID = somBaseLayer[iRow][iCol];
                    int rank = Array.IndexOf(inputFrameIdsArray, frameID);

                    if (rank < 0 || rank >= inputFrameIdsArray.Length)
                    {
                        throw new ArgumentOutOfRangeException("Rank " + rank + " is not found in SOM datastructure!");
                    }

                    rowResult[iCol] = (frameID, rank);
                }

                result[iRow] = rowResult;
            }

            return result;
        }

        /// <summary>
        /// Compute right/bottom border for each element in layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="deepFeaturesProvider"></param>
        /// <returns></returns>
        private float[][] ComputeBorderSimilarities(int [][] layer, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            float[][] deepFeatures = deepFeaturesProvider.Descriptors;
            float[][] result = new float[layer.Length][];
            for(int iRow = 0; iRow < layer.Length; iRow++)
            {
                float[] rowResult = new float[layer[iRow].Length * 2];
                for(int iCol = 0; iCol < layer[iRow].Length; iCol ++)
                {
                    // get index of right and bottom neighbour (dealing with edge cases)
                    int bottomBorderIndex = (iRow + 1 < layer.Length) ? iRow + 1 : 0;
                    int rightBorderIndex = (iCol + 1 < layer[iRow].Length) ? iCol + 1 : 0;

                    // get deep features of current frame and his right and bottom neighbour
                    float [] bottomDeepFeature = deepFeatures[layer[bottomBorderIndex][iCol]];
                    float[] rightDeepFeature = deepFeatures[layer[iRow][rightBorderIndex]];
                    float[] currentDeepFeature = deepFeatures[layer[iRow][iCol]];

                    // compute the similarity using cosine similarity
                    float bottomSimilarity = CosineSimilarityHelper.CosineSimilarityNormalized01(bottomDeepFeature, currentDeepFeature);
                    float rightSimilarity = CosineSimilarityHelper.CosineSimilarityNormalized01(rightDeepFeature, currentDeepFeature);

                    // save the results;
                    rowResult[iCol * 2] = bottomSimilarity;
                    rowResult[(iCol * 2) + 1] = rightSimilarity;
                }
                result[iRow] = rowResult;
            }
            return result;
        }

        /// <summary>
        /// Extracts deep features for each frameID from array
        /// </summary>
        /// <param name="inputFrameIdsArray">array of frameIDs</param>
        /// <param name="deepFeaturesProvider"></param>
        /// <returns></returns>
        private (float[] framesData, int dimension) ExtractDataFromSemanticVectors(int[] inputFrameIdsArray, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            int dimension = deepFeaturesProvider.Descriptors[0].Length;
            float[] framesData = new float[inputFrameIdsArray.Length * dimension];
            for (int iFrame = 0; iFrame < inputFrameIdsArray.Length; iFrame++)
            {
                deepFeaturesProvider.Descriptors[inputFrameIdsArray[iFrame]].CopyTo(framesData, iFrame * dimension);
            }

            return (framesData, dimension);
        }
    }
}
