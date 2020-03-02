using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.SOMGridSorting;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.DataLayer.DataIO.ZoomDisplayIO;

namespace ViretTool.BusinessLayer.Services
{
    public class SomGeneratorProvider : ZoomDisplayProvider
    {
        public SomGeneratorProvider(IDatasetParameters datasetParameters, string datasetDirectory) : base(datasetParameters, datasetDirectory)
        {
            
        }

        public override int[] GetInitialLayer(int rowCount, int columnCount, IEnumerable<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            LayersIds.Clear();
            ColorSimilarity.Clear();
            // Convert int[] => long[]
            IEnumerable<long> inputFrameIds_long = inputFrameIds.Select(x => (long)x);
            int [] somResult = SOMWrapper.CreateSOMRepresentants(inputFrameIds_long.ToArray(), null, columnCount, rowCount, 15, deepFeaturesProvider);

            // maybe move ReshapeTo2DArray function somewhere else, create some kind of Coverter/Helper static class (ask Gregor where)

            // add more layers - discuss with Gregor
            int [][] reshapeTo2D_SOMresult = ZoomDisplayReader.ReshapeTo2DArray<int>(somResult, rowCount, columnCount);
            LayersIds.Add(reshapeTo2D_SOMresult);

            // Compute color similarities
            float [][] colorSimilarities = calculateBorderColorSimilarities(reshapeTo2D_SOMresult, deepFeaturesProvider);
            ColorSimilarity.Add(colorSimilarities);

            return GetInitialLayer(rowCount, columnCount);
        }
        // PROBLEM in GetColorSimilarity - Search is conducted using FrameID -> problem with repeating frameIDs
        // Need to be solved
        // next problem are multiple layer when there is none (we generated from dll, but new layer still appears in sidebar of frame-view)
        private float[][] calculateBorderColorSimilarities(int [][] layer, IDescriptorProvider<float[]> deepFeaturesProvider)
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

                    // calculate the similarity using cosine similarity
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
    }
}
