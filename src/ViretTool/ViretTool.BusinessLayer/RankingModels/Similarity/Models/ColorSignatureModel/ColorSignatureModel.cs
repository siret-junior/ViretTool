//#define USE_MULTIPLICATION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models.ColorSignatureModel
{
    public class ColorSignatureModel : IColorSketchModel
    {
        public ColorSketchQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; set; }
        public RankingBuffer OutputRanking { get; set; }
        //public IRankFusion RankFusion { get; }

        /// <summary>
        /// A thumbnail based signature in RGB format, stored as a 1D byte array.
        /// </summary>
        private byte[][] _colorSignatures;
        private int _signatureWidth = 26;     // TODO: load dynamically from provided initializer file
        private int _signatureHeight = 15;

        private Dictionary<Ellipse, float[]> _partialRankingCache = new Dictionary<Ellipse, float[]>();


        public ColorSignatureModel(IDescriptorProvider<byte[]> colorSignatures)
        {
            //RankFusion = rankFusion;
            _colorSignatures = colorSignatures.Descriptors;
        }

        
        public void ComputeRanking(ColorSketchQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking)
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

            // remove old not used ellipse cache entires
            foreach (Ellipse key in _partialRankingCache.Keys.ToList())
            {
                if (!query.ColorSketchEllipses.Contains(key))
                {
                    _partialRankingCache.Remove(key);
                }
            }

            // add new ellipse partial ranking entries
            foreach (Ellipse ellipse in query.ColorSketchEllipses)
            {
                if (!_partialRankingCache.ContainsKey(ellipse))
                {
                    _partialRankingCache.Add(ellipse, EvaluateOneQueryCentroid(ellipse));
                }
            }

            // perform fusion of partial rankings
            float[][] distances = _partialRankingCache.Values.ToArray();
            Parallel.For(0, inputRanking.Ranks.Length, itemId =>
            {
                if (InputRanking.Ranks[itemId] == float.MinValue)
                {
                    // ignore filtered frames
                    OutputRanking.Ranks[itemId] = float.MinValue;
                    return;
                }
                
                float centroidSum = 0;
                for (int iCentroid = 0; iCentroid < distances.Length; iCentroid++)
                {
                    centroidSum += distances[iCentroid][itemId];
                }
                OutputRanking.Ranks[itemId] = centroidSum;
            });

        }

        private bool HasQueryOrInputChanged(ColorSketchQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(ColorSketchQuery query)
        {
            return query == null 
                || query.ColorSketchEllipses == null
                || !query.ColorSketchEllipses.Any();
        }

        private float[] EvaluateOneQueryCentroid(Ellipse ellipse)//Tuple<Point, Color, Point, bool> qc)
        {
            float[] distances = new float[InputRanking.Ranks.Length];


            // initialize to -1
            for (int i = 0; i < distances.Length; i++)
            {
#if USE_MULTIPLICATION
                distances[i] = -1;
#else
                distances[i] = 0;
#endif
            }

            // transform [x, y] to a list of investigated positions in mGridRadius
            Tuple<int[], Color, Ellipse.State> t = PrepareQuery(ellipse);

            Parallel.For(0, distances.Length, i =>
            {
                // ignore filtered frames
                if (InputRanking.Ranks[i] == float.MinValue)
                {
                    distances[i] = float.MinValue;
                    return;
                }


                byte[] signature = _colorSignatures[i];

                int R = t.Item2.R, G = t.Item2.G, B = t.Item2.B;

                switch (ellipse.EllipseState)
                {
                    case Ellipse.State.All:

                        double avgRank = 0;
                        foreach (int offset in t.Item1)
                        {
                            avgRank += Math.Sqrt(
                                L2SquareDistance(
                                    R, signature[offset],
                                    G, signature[offset + 1],
                                    B, signature[offset + 2]));
                        }
                        distances[i] -= Convert.ToSingle(avgRank / t.Item1.Length);
                        break;

                    case Ellipse.State.Any:
                        double minRank = int.MaxValue;
                        foreach (int offset in t.Item1)
                        {
                            minRank = Math.Min(minRank,
                                L2SquareDistance(
                                    R, signature[offset],
                                    G, signature[offset + 1],
                                    B, signature[offset + 2]));
                        }
                        distances[i] -= Convert.ToSingle(Math.Sqrt(minRank));
                        break;

                    default:
                        throw new NotImplementedException();
                }
            });

            // TODO
            return distances;
        }


        public double ComputeDistance(int frameId1, int frameId2)
        {
            return ComputeDistance(_colorSignatures[frameId1], _colorSignatures[frameId2]);
        }

        public static double ComputeDistance(byte[] vectorA, byte[] vectorB)
        {
            return L2Distance(vectorA, vectorB);
        }

        
        /// <summary>
        /// Precompute a set of 2D grid cells (represented as offsets in 1D array) that should be investigated for the most similar query color.
        /// </summary>
        /// <param name="queryCentroid">Colored point from the color sketch.</param>
        /// <returns></returns>
        private Tuple<int[], Color, Ellipse.State> PrepareQuery(Ellipse ellipse)
        {
            List<Tuple<int[], Color, Ellipse.State>> queries = new List<Tuple<int[], Color, Ellipse.State>>();

            double x = ellipse.PositionX * _signatureWidth, y = ellipse.PositionY * _signatureHeight;
            double ax = ellipse.HorizontalAxis * _signatureWidth, ay = ellipse.VerticalAxis * _signatureHeight;
            double ax2 = ax * ax;
            double ay2 = ay * ay;

            List<int> offsets = new List<int>();
            for (int i = 0; i < _signatureWidth; i++)
                for (int j = 0; j < _signatureHeight; j++)
                    if (1 >= (x - i - 0.5) * (x - i - 0.5) / ax2 + (y - j - 0.5) * (y - j - 0.5) / ay2)
                        offsets.Add(j * _signatureWidth * 3 + i * 3);

            return new Tuple<int[], Color, Ellipse.State>(offsets.ToArray(), 
                ImageHelper.RGBtoLabByte(ellipse.ColorR, ellipse.ColorG, ellipse.ColorB), ellipse.EllipseState);
        }


        private static int L2SquareDistance(int r1, int r2, int g1, int g2, int b1, int b2)
        {
            int rDiff = (r1 - r2);
            int gDiff = (g1 - g2);
            int bDiff = (b1 - b2);
            return rDiff * rDiff + gDiff * gDiff + bDiff * bDiff;
        }

        private static double L2Distance(byte[] x, byte[] y)
        {
            double result = 0, r;
            for (int i = 0; i < x.Length; i++)
            {
                r = x[i] - y[i];
                result += r * r;
            }
            return Math.Sqrt(result);
        }

    }
}
