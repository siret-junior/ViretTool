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

namespace ViretTool.BusinessLayer.Services
{
    public class SomGeneratorProvider : ZoomDisplayProvider
    {
        public SomGeneratorProvider(IDatasetParameters datasetParameters, string datasetDirectory) 
            : base(datasetParameters, datasetDirectory)
        {
            // TODO: set SOM initialization here whenever dataset is loaded (using datasetServicesManager.DatasetOpened event).
            // TODO: access IDescriptorProvider<float[]> through passed datasetServicesManager
        }
        private const int _baseWidth = 20;
        private const int _baseHeight = 20;

        private const int _minimalLayerWidth = 10;
        private const int _minimalLayerHeight = 10;

        public override int[] GetInitialLayer(int rowCount, int columnCount, IList<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            LayersIds.Clear();
            BorderSimilarities.Clear();

            // compute base layer
            int datasetSize = _baseHeight * _baseWidth;
            int rlen = 20;
            int[] inputFrameIdsArray = inputFrameIds.Take(datasetSize).ToArray();

            if (datasetSize > inputFrameIdsArray.Length)
            {
                AddCopiesToInputFrameIds(ref inputFrameIdsArray, datasetSize);
            }

            (float[] framesData, int dimension) = ExtractDataFromSemanticVectors(inputFrameIdsArray, deepFeaturesProvider);

            int[] somResult1D = SOMWrapper.GetSomRepresentants(framesData, datasetSize, dimension, _baseWidth, _baseHeight, rlen, inputFrameIdsArray);
            
            
            int[][] somBaseLayer = somResult1D.To2DArray(_baseWidth, _baseHeight);
 
            // Compute border similarities for base layer
            float[][] somBaseBorderSimilarities = ComputeBorderSimilarities(somBaseLayer, deepFeaturesProvider);

            //// compute the additional layer sizes based on the base layer dimensions
            List<(int layerWidth, int layerHeight)> layerSizes = ComputeAdditionalLayerSizesBottomUp(_baseWidth, _baseHeight);

            // compute additional layers from bottom up to fulfill predicate
            // that each layer above is a subset of the layer below
            Stack<int[][]> layerStack = new Stack<int[][]>();
            Stack<float[][]> borderStack = new Stack<float[][]>();

            // add base layer to stack
            layerStack.Push(somBaseLayer);
            borderStack.Push(somBaseBorderSimilarities);

            (int frameIDs, int ranks)[][] rankedSomBaseLayer = assignRankToEachElement(somBaseLayer, inputFrameIdsArray);

            foreach ((int layerWidth, int layerHeight) in layerSizes)
            {
                int[][] layer = SubsampleLayer(rankedSomBaseLayer, layerWidth, layerHeight);
                layerStack.Push(layer);
                borderStack.Push(ComputeBorderSimilarities(layer, deepFeaturesProvider));
            }

            
            // store layers
            while (layerStack.Count > 0)
            {
                LayersIds.Add(layerStack.Pop());
                BorderSimilarities.Add(borderStack.Pop());
            }

            // TODO: consider using a single structure for both: frameIds and also its borders

            // zoom into base layer
            return ZoomIntoLayer(0, LayersIds[0][0][0], rowCount, columnCount);
        }

        /// <summary>
        /// Duplicate array elements to get particular length of array
        /// </summary>
        /// <param name="originalInputFrameIds"></param>
        /// <param name="intendedSize"></param>
        private void AddCopiesToInputFrameIds(ref int[] originalInputFrameIds, int intendedSize)
        {

            int startIndex = originalInputFrameIds.Length;
            Array.Resize(ref originalInputFrameIds, intendedSize);


            int originalArrayIterator = 0;
            for(int i = startIndex; i < intendedSize; i++)
            {
                if(originalArrayIterator >= startIndex)
                {
                    originalArrayIterator = 0;
                }
                originalInputFrameIds[i] = originalInputFrameIds[originalArrayIterator++];
            }

        }
        private List<(int layerWidth, int layerHeight)> ComputeAdditionalLayerSizesBottomUp(int baseLayerWidth, int baseLayerHeight)
        {
            List<(int layerWidth, int layerHeight)> result = new List<(int layerWidth, int layerHeight)>();

            baseLayerWidth /= 2;
            baseLayerHeight /= 2;

            while (baseLayerHeight >= _minimalLayerHeight && baseLayerWidth >= _minimalLayerWidth)
            {
                
                result.Add((baseLayerWidth, baseLayerHeight));

                baseLayerWidth /= 2;
                baseLayerHeight /= 2;
            }

            
            return result;
        }

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

        (int frameIDs, int ranks)[][] assignRankToEachElement(int[][] somBaseLayer, int[] inputFrameIdsArray)
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


        // TODO: PROBLEM in GetColorSimilarity - Search is conducted using FrameID -> problem with repeating frameIDs
        // Need to be solved
        // next problem are multiple layer when there is none (we generated from dll, but new layer still appears in sidebar of frame-view)
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
