using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.ImageAnalytics;

namespace ViretTool.BusinessLayer.ExternalDescriptors
{
    public class NasNetScorer
    {
        private readonly MLContext MLContext = new MLContext();
        private readonly PredictionFunction<NasNetInputData, NasNetPrediction> Model;
        private readonly float[][] PCAComponents;
        private readonly float[] PCAMean;


        public NasNetScorer(string modelLocation, string pcaComponetsLocation, string pcaMeanLocation)
        {
            try
            {
                Model = LoadModel(modelLocation);
                PCAMean = LoadPCAMean(pcaMeanLocation);
                PCAComponents = LoadPCAComponents(pcaComponetsLocation);
            }
            catch (Exception)
            {
                // if the model could not be loaded, fail transparently
            }
        }

        // TODO: add option to set root directory for relative input file location
        public float[] GetReducedFeatures(string inputFileLocation)
        {
            // if the model could not be loaded, fail transparently
            if (Model == null) return new float[128];

            var f = GetFullFeatures(inputFileLocation);

            for (int i = 0; i < 1056; i++)
            {
                f[i] -= PCAMean[i];
            }

            var r = new float[128];
            for (int i = 0; i < 128; i++)
            {
                float sum = 0;
                for (int j = 0; j < 1056; j++)
                {
                    sum += f[j] * PCAComponents[i][j];
                }

                r[i] = sum;
            }

            r = NormalizeVector(r);
            return r;
        }

        private float[] GetFullFeatures(string inputFileLocation)
        {
            return Model.Predict(new NasNetInputData() { ImagePath = inputFileLocation }).Features;
        }

        private static float[] NormalizeVector(float[] originalVector)
        {
            float sum = (float)Math.Sqrt(originalVector.Sum(v => v * v));

            for (int i = 0; i < 128; i++)
            {
                originalVector[i] /= sum;
            }

            return originalVector;
        }

        private PredictionFunction<NasNetInputData, NasNetPrediction> LoadModel(string modelLocation)
        {
            var loader = new TextLoader(
                MLContext,
                new TextLoader.Arguments
                {
                    Column = new[]
                             {
                                 new TextLoader.Column("ImagePath", DataKind.Text, 0),
                             }
                });

            var pipeline = MLContext.Transforms.LoadImages(imageFolder: null, columns: ("ImagePath", "ImageReal"))
                                    .Append(
                                        MLContext.Transforms.Resize("ImageReal", "ImageReal", NasNetSettings.ImageHeight, NasNetSettings.ImageWidth)
                                    )
                                    .Append(
                                        MLContext.Transforms.ExtractPixels(
                                            new[]
                                            {
                                                new ImagePixelExtractorTransform.ColumnInfo(
                                                    "ImageReal",
                                                    NasNetSettings.InputTensorName,
                                                    interleave: NasNetSettings.ChannelsLast,
                                                    scale: NasNetSettings.Scale,
                                                    offset: NasNetSettings.Offset)
                                            })
                                    )
                                    .Append(
                                        MLContext.Transforms.ScoreTensorFlowModel(
                                            modelLocation,
                                            new[] { NasNetSettings.InputTensorName },
                                            new[] { NasNetSettings.OutputTensorName })
                                    );

            // fake input data
            var data = loader.Read(new MultiFileSource(null));
            // fit on empty input data
            var modeld = pipeline.Fit(data);

            return modeld.MakePredictionFunction<NasNetInputData, NasNetPrediction>(MLContext);
        }

        private float[][] LoadPCAComponents(string pcaComponetsLocation)
        {
            float[][] comp = new float[128][];
            using (BinaryReader reader = new BinaryReader(File.Open(pcaComponetsLocation, FileMode.Open)))
            {
                if (reader.ReadInt64() != 0x706d6f632d414350)
                {
                    // b'PCA-comp'
                    throw new Exception("Incorrect PCA components file.");
                }

                for (int i = 0; i < comp.Length; i++)
                {
                    comp[i] = new float[1056];
                    for (int j = 0; j < comp[i].Length; j++)
                    {
                        comp[i][j] = reader.ReadSingle();
                    }
                }
            }

            return comp;
        }

        private float[] LoadPCAMean(string pcaMeanLocation)
        {
            var mean = new float[1056];
            using (BinaryReader reader = new BinaryReader(File.Open(pcaMeanLocation, FileMode.Open)))
            {
                if (reader.ReadInt64() != 0x6e61656d2d414350)
                {
                    // b'PCA-mean'
                    throw new Exception("Incorrect PCA mean file.");
                }

                for (int i = 0; i < mean.Length; i++)
                {
                    mean[i] = reader.ReadSingle();
                }
            }

            return mean;
        }

        public struct NasNetSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const bool ChannelsLast = true;
            public const float Scale = 1 / 127.5f;
            public const float Offset = 127.5f;

            public const string InputTensorName = "Placeholder";
            public const string OutputTensorName = "final_layer/Mean";
        }

        public class NasNetPrediction
        {
            [ColumnName(NasNetSettings.OutputTensorName)]
            public float[] Features;
        }

        public class NasNetInputData
        {
            public string ImagePath;
        }
    }
}
