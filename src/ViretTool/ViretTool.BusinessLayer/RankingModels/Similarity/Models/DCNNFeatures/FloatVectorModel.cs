using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.ExternalDescriptors;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNFeatures
{
    public class FloatVectorModel : ISemanticExampleModel
    {
        private readonly NasNetScorer _nasNetScorer;

        public FloatVectorModel(/*IRankFusion rankFusion, */IDescriptorProvider<float[]> semanticDescriptorProvider, NasNetScorer nasNetScorer)
        {
            _nasNetScorer = nasNetScorer;
            mFloatVectors = semanticDescriptorProvider.Descriptors;
            //RankFusion = rankFusion;
        }

        public SemanticExampleQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; set; }
        public RankingBuffer OutputRanking { get; set; }
        //public IRankFusion RankFusion { get; }

        /// <summary>
        /// Extracted features from DCNN, normalized to |v| = 1 and each dimension globally quantized to byte
        /// </summary>
        public float[][] mFloatVectors;

        private int mVectorDimension;

        private readonly string mDescriptorsFilename;

        private Dictionary<int, float[]> mCache = new Dictionary<int, float[]>();

        public void Clear()
        {
            mCache.Clear();
        }        

        public float[] AddQueryResultsToCache(int queryId, bool isPositiveExample)
        {
            if (mCache.ContainsKey(queryId))
                return mCache[queryId];

            float[] queryData = mFloatVectors[queryId];

            float[] results = ComputeSimilarity(queryData);

            mCache.Add(queryId, results);
            return mCache[queryId];
        }

        private float[] ComputeSimilarity(float[] queryData)
        {
            float[] results = new float[InputRanking.Ranks.Length];
            Parallel.For(
                0,
                results.Length,
                i =>
                {
                    if (InputRanking.Ranks[i] == float.MinValue)
                    {
                        // ignore filtered frames
                        return;
                    }

                    results[i] = CosineSimilarity(mFloatVectors[i], queryData);
                });
            return results;
        }

        public void ComputeRanking(SemanticExampleQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

            if (!HasQueryOrInputChanged(query, inputRanking))
            {
                // nothing changed, OutputRanking contains cached data from previous computation
                OutputRanking.IsUpdated = false;
                return;
            }
            else
            {
                CachedQuery = query;
                OutputRanking.IsUpdated = true;
            }

            if (IsQueryEmpty(query))
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
                return;
            }

            float[] ranking = new float[InputRanking.Ranks.Length];

            foreach (int positiveQueryId in query.PositiveExampleIds)
            {
                float[] partialResults = AddQueryResultsToCache(positiveQueryId, true);
                Parallel.For(0, InputRanking.Ranks.Length, i =>
                {
                    ranking[i] += partialResults[i];
                });
            }

            foreach (string externalImageRotated in query.ExternalImages)
            {
                float[] reducedFeatures = _nasNetScorer.GetReducedFeatures(externalImageRotated);
                float[] partialResults = ComputeSimilarity(reducedFeatures);
                Parallel.For(0, InputRanking.Ranks.Length, i =>
                                                           {
                                                               ranking[i] += partialResults[i];
                                                           });
            }

            if (query.NegativeExampleIds != null)
            {
                foreach (int negativeQueryId in query.NegativeExampleIds)
                {
                    float[] partialResults = AddQueryResultsToCache(negativeQueryId, false);
                    Parallel.For(0, InputRanking.Ranks.Length, i =>
                    {
                        ranking[i] -= partialResults[i];
                    });
                }
            }
            OutputRanking.Ranks = ranking;
        }

        private bool HasQueryOrInputChanged(SemanticExampleQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(SemanticExampleQuery query)
        {
            return query == null || !query.PositiveExampleIds.Any() && !query.ExternalImages.Any();
        }

        public float[] GetFrameSemanticVector(int frameId)
        {
            return mFloatVectors[frameId];
        }

        public static float ComputeDistance(float[] vectorA, float[] vectorB)
        {
            return CosineDistance(vectorA, vectorB);
        }

        private static float CosineDistance(float[] x, float[] y)
        {
            return 1 - CosineSimilarity(x, y);
        }

        private static float CosineSimilarity(float[] x, float[] y)
        {
            return CosineSimilaritySISD(x, y);
        }

        private static float CosineSimilaritySISD(float[] x, float[] y)
        {
            double result = 0.0;

            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }

            return Convert.ToSingle(result);
        }

        //private static float CosineSimilaritySIMD(float[] vector1, float[] vector2)
        //{
        //    int chunkSize = Vector<float>.Count;
        //    float result = 0f;

        //    Vector<float> vectorChunk1;
        //    Vector<float> vectorChunk2;
        //    for (var i = 0; i < vector1.Length; i += chunkSize)
        //    {
        //        vectorChunk1 = new Vector<float>(vector1, i);
        //        vectorChunk2 = new Vector<float>(vector2, i);

        //        result += Vector.Dot(vectorChunk1, vectorChunk2);
        //    }

        //    return result;
        //}

        //private static float L2Distance(float[] x, float[] y)
        //{
        //    double result = 0.0;

        //    for (int i = 0; i < x.Length; i++)
        //    {
        //        double difference = x[i] - y[i];
        //        result += difference * difference;
        //    }

        //    return Convert.ToSingle(Math.Sqrt(result));
        //}

        //private void LoadDescriptors()
        //{
        //    if (!File.Exists(mDescriptorsFilename))
        //        throw new Exception("Descriptors were not created to " + mDescriptorsFilename);

        //    using (BinaryReader reader = new BinaryReader(File.OpenRead(mDescriptorsFilename)))
        //    {
        //        if (!mDataset.ReadAndCheckFileHeader(reader))
        //            throw new Exception("Dataset/descriptor mismatch. Delete file " + mDescriptorsFilename);

        //        int vectorCount = reader.ReadInt32();
        //        if (vectorCount < mDataset.Frames.Count)
        //            throw new Exception("Too few descriptors in file " + mDescriptorsFilename);

        //        vectorCount = mDataset.Frames.Count;

        //        mVectorDimension = reader.ReadInt32();

        //        for (int i = 0; i < vectorCount; i++)
        //        {
        //            byte[] data = reader.ReadBytes(mVectorDimension * sizeof(float));
        //            float[] dataVector = new float[mVectorDimension];

        //            Buffer.BlockCopy(data, 0, dataVector, 0, data.Length);

        //            mFloatVectors.Add(dataVector);
        //        }
        //    }
        //}
    }
}
