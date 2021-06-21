using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Viret.Ranking.Features
{
    public class FeatureVectors
    {
        public readonly float[][] Vectors;


        public FeatureVectors(float[][] vectors)
        {
            Vectors = vectors;
        }

        public static FeatureVectors FromFile(string inputFile, int vectorDimension, int vectorCount, int maxKeyframes = -1)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // trim dataset if required
                if (maxKeyframes > 0)
                {
                    vectorCount = maxKeyframes;
                }

                // load vectors
                float[][] vectors = new float[vectorCount][];
                for (int iBlob = 0; iBlob < vectorCount; iBlob++)
                {
                    byte[] byteArray = reader.ReadBytes(vectorDimension * sizeof(float));
                    float[] floatArray = new float[vectorDimension];
                    Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
                    vectors[iBlob] = floatArray;
                }

                if (maxKeyframes < 0 && reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    throw new DataMisalignedException($"There are still some data left at the end of the file.");
                }

                return new FeatureVectors(vectors);
            }
        }

        public static FeatureVectors FromDirectory(string inputDirectory, string filePattern, int maxKeyframes = -1)
        {
            try
            {
                string inputFile = Directory.GetFiles(inputDirectory, filePattern).FirstOrDefault();
                if (inputFile == null)
                {
                    // TODO: temporarily fail silently
                    return null;
                    //throw new FileNotFoundException($"Features file with extension '{extension}' was not found in directory '{inputDirectory}'.");
                }
                Regex regex = new Regex(@".*\.(?<vectorCount>[0-9]+)x(?<vectorDimension>[0-9]+)\..*");
                Match match = regex.Match(Path.GetFileNameWithoutExtension(inputFile));
                if (!match.Success)
                {
                    throw new FileNotFoundException($"Error parsing vector count and vector dimension from '{inputFile}' ({regex.ToString()})");
                }

                int vectorCount = int.Parse(match.Groups["vectorCount"].Value);
                int vectorDimension = int.Parse(match.Groups["vectorDimension"].Value);
                return FromFile(inputFile, vectorDimension, vectorCount, maxKeyframes);
            }
            catch
            {
                // temporarily fail silently
                return null;
            }
        }


        public (int[] Ranks, double[] Scores) ComputeKnnRanking(float[] queryVector)
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
            return (ranks, similarities);
        }

        public IEnumerable<(int ItemId, double Score)> ComputeKnnRankingWithScores(float[] queryVector)
        {
            return Vectors.Select<float[], (int ItemId, double Score)>((vector, index) => (index, GetSimilarity(vector, queryVector)))
                .OrderByDescending(item => item.Score);
        }

        public (int[] Ranks, double[] Scores) ComputeKnnRanking(int vectorId)
        {
            // TODO: cache or pre-compute
            return ComputeKnnRanking(Vectors[vectorId]);
        }


        public double GetSimilarity(int keyframeId1, int keyframeId2)
        {
            return GetSimilarity(Vectors[keyframeId1], Vectors[keyframeId2]);
        }

        public static double GetSimilarity(float[] x, float[] y)
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
