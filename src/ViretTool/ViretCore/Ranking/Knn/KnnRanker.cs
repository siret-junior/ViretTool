using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.Knn
{
    public class KnnRanker
    {
        public readonly float[][] Vectors;


        public KnnRanker(float[][] vectors)
        {
            Vectors = vectors;
        }

        public static KnnRanker FromFile(string inputFile, int maxKeyframes = -1)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // read blob metadata
                int blobCount = reader.ReadInt32();
                int blobLength = reader.ReadInt32();
                int vectorLength = blobLength / sizeof(float);

                // read filetype metadata
                int metadataLength = reader.ReadInt32();

                // trim dataset if required
                if (maxKeyframes > 0)
                {
                    blobCount = maxKeyframes;
                }

                // load vectors
                float[][] vectors = new float[blobCount][];
                for (int iBlob = 0; iBlob < blobCount; iBlob++)
                {
                    byte[] byteArray = reader.ReadBytes(vectorLength * sizeof(float));
                    float[] floatArray = new float[vectorLength];
                    Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
                    vectors[iBlob] = floatArray;
                }

                if (maxKeyframes < 0 && reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    throw new DataMisalignedException($"There are still some data left at the end of the file.");
                }

                return new KnnRanker(vectors);
            }    
        }

        public static KnnRanker FromDirectory(string inputDirectory, int maxKeyframes = -1, string extension = ".w2vv")
        {
            string inputFile = Directory.GetFiles(inputDirectory, $"*{extension}").FirstOrDefault();
            if (inputFile == null)
            {
                throw new FileNotFoundException($"W2VV features file was not found in directory '{inputDirectory}'");
            }
            return FromFile(inputFile, maxKeyframes);
        }


        public int[] ComputeKnnRanking(float[] queryVector)
        {
            int[] ranks = new int[Vectors.Length];
            double[] similarities = new double[Vectors.Length];

            // compute similarities
            Parallel.For(0, Vectors.Length, (i) => 
            {
                similarities[i] = GetSimilarity(Vectors[i], queryVector);
                ranks[i] = i;
            });

            // sort descending
            Array.Sort(similarities, ranks, Comparer<double>.Create((x, y) => y.CompareTo(x)));
            return ranks;
        }

        public int[] ComputeKnnRanking(int vectorId)
        {
            // TODO: cache or pre-compute
            return ComputeKnnRanking(Vectors[vectorId]);
        }

        private static double GetSimilarity(float[] x, float[] y)
        {
            return DotProductPlusOne(x, y);
        }

        /// <summary>
        /// Returns values from 0 to 2, but only for normalized vectors!
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static double DotProductPlusOne(float[] x, float[] y)
        {
            double result = 0;
            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }
            return result + 1;
        }
    }
}
