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
        private int _baseWidth = 20;
        private int _baseHeight = 20;

        public override int[] GetInitialLayer(int rowCount, int columnCount, IList<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            LayersIds.Clear();
            BorderSimilarities.Clear();

            // compute base layer
            int datasetSize = _baseHeight * _baseWidth;
            int rlen = 20;
            int[] inputFrameIdsArray = inputFrameIds.Take(datasetSize).ToArray();
            (float[] framesData, int dimension) = ExtractDataFromSemanticVectors(inputFrameIdsArray, deepFeaturesProvider);

            int[] somResult1D = SOMWrapper.GetSomRepresentants(framesData, datasetSize, dimension, _baseWidth, _baseHeight, rlen, inputFrameIdsArray);
            
            int[][] somBaseLayer = somResult1D.To2DArray(_baseWidth, _baseHeight);

            // TODO: code prepared for a multilayer solution
            //// compute the additional layer sizes based on the base layer dimensions
            //int baseLayerWidth = columnCount;   // TODO: rename/reassign
            //int baseLayerHeight = rowCount;
            //List<(int layerWidth, int layerHeight)> layerSizes = ComputeAdditionalLayerSizesBottomUp(baseLayerWidth, baseLayerHeight);

            //// compute additional layers from bottom up to fulfill predicate
            //// that each layer above is a subset of the layer below
            //Stack<int[][]> layerStack = new Stack<int[][]>();
            //Stack<float[][]> borderStack = new Stack<float[][]>();
            //foreach ((int layerWidth, int layerHeight) in layerSizes)
            //{
            //    int[][] layer = SubsampleLayer(somBaseLayer, layerWidth, layerHeight);
            //    layerStack.Push(layer);
            //    borderStack.Push(ComputeBorderSimilarities(layer, deepFeaturesProvider));
            //}

            //// store layers
            //while (layerStack.Count > 0)
            //{
            //    LayersIds.Add(layerStack.Pop());
            //    BorderSimilarities.Add(borderStack.Pop());
            //}

            // TODO: temporary using just the base layer
            LayersIds.Add(somBaseLayer);

            // Compute border similarities
            float [][] borderSimilarities = ComputeBorderSimilarities(somBaseLayer, deepFeaturesProvider);
            BorderSimilarities.Add(borderSimilarities);

            // TODO: consider using a single structure for both: frameIds and also its borders
            return GetInitialLayer(rowCount, columnCount);
        }

        private List<(int layerWidth, int layerHeight)> ComputeAdditionalLayerSizesBottomUp(int baseLayerWidth, int baseLayerHeight)
        {
            // TODO: temporary solution
            List<(int layerWidth, int layerHeight)> result = new List<(int layerWidth, int layerHeight)>();
            
            // and a single (30x30) mid layer if there is more than 1000 items
            if (baseLayerWidth * baseLayerHeight > 1000)
            {
                result.Add((30, 30));
            }
            
            // add 10x10 top layer (TODO: dynamic size based on user display)
            result.Add((10, 10));
            
            return result;
        }

        private int[][] SubsampleLayer(int[][] somBaseLayer, int layerWidth, int layerHeight)
        {
            if (layerWidth >= somBaseLayer[0].Length || layerHeight >= somBaseLayer.Length)
            {
                throw new ArgumentOutOfRangeException($"Trying to supersample in a subsampling method " 
                    + $"([{somBaseLayer[0].Length}, {somBaseLayer.Length}] layer, [{layerWidth}, {layerHeight}] subsampling).");
            }

            // TODO: subsample 
            throw new NotImplementedException();
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
        (float[] framesData, int dimension) ExtractDataFromSemanticVectors(int[] inputFrameIdsArray, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            int dimension = deepFeaturesProvider.Descriptors[0].Length;
            float[] framesData = new float[inputFrameIdsArray.Length * dimension];
            for (int iFrame = 0; iFrame < inputFrameIdsArray.Length; iFrame++)
            {
                deepFeaturesProvider.Descriptors[iFrame].CopyTo(framesData, iFrame * dimension);
            }

            return (framesData, dimension);
        }
    }
}
