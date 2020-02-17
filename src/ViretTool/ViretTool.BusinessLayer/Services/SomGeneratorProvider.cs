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

        private IDescriptorProvider<float[]> deepFeaturesProvider;
        private bool _isSomLoaded = false;
        public bool IsSomLoaded {
            get => _isSomLoaded;
            set => _isSomLoaded = value;
        }
        public SomGeneratorProvider(IDatasetParameters datasetParameters, string datasetDirectory) : base(datasetParameters, datasetDirectory)
        {
            
        }

        // current column boundaries of int[] somIds, which are currently shown in display
        private int colStart;
        private int colEnd;

        // current row boundaries of int[] somIds, which are currently shown in display
        private int rowStart;
        private int rowEnd;

        // Dimensions of SOM layers
        private const int xDim = 10;
        private const int yDim = 10;


        public override int[] GetInitialLayer(int rowCount, int columnCount, IEnumerable<int> inputFrameIds, IDescriptorProvider<float[]> deepFeaturesProvider)
        {
            LayersIds.Clear();
            ColorSimilarity.Clear();
            // Convert int[] => long[]
            IEnumerable<long> inputFrameIds_long = inputFrameIds.Select(x => (long)x);
            int [] somResult = SOMWrapper.CreateSOMRepresentants(inputFrameIds_long.ToArray(), null, xDim, xDim, 15, deepFeaturesProvider);

            // compute borders! for now add just 1.0 for every border
            float[][] colorSimilarities = new float[yDim][];
            for(int i = 0; i < rowCount; i ++)
            {
                float[] tmp = new float[xDim*2];
                for(int j = 0; j < tmp.Length; j++)
                {
                    tmp[j] = 1.0f;
                }
                colorSimilarities[i] = tmp;
            }

            // maybe move ReshapeTo2DArray function somewhere else, create some kind of Coverter/Helper static class (ask Gregor where)
            
            // add more layers - discuss with Gregor
            LayersIds.Add(ZoomDisplayReader.ReshapeTo2DArray<int>(somResult, yDim, xDim));
            ColorSimilarity.Add(colorSimilarities);
            return GetInitialLayer(rowCount, columnCount);
        }
    }
}
