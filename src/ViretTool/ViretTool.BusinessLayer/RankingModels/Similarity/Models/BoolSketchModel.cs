using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public class BoolSketchModel : IBoolSketchModel
    {
        public int SignatureWidth = 26;     // TODO: load dynamically from provided initializer file
        public int SignatureHeight = 15;
        public bool[][] Signatures { get; }

        public ColorSketchQuery CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }

        private Dictionary<Ellipse, float[]> _partialRankingCache = new Dictionary<Ellipse, float[]>();


        public BoolSketchModel(IBoolSignatureDescriptorProvider boolSignatureDescriptorProvider)
        {
            Signatures = boolSignatureDescriptorProvider.Descriptors;
            SignatureWidth = boolSignatureDescriptorProvider.SignatureWidth;
            SignatureHeight = boolSignatureDescriptorProvider.SignatureHeight;
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

        private float[] EvaluateOneQueryCentroid(Ellipse ellipse)
        {
            float[] distances = new float[InputRanking.Ranks.Length];

            // transform [x, y] to a list of investigated positions in mGridRadius
            (int[] offsets, bool color) = PrepareQuery(ellipse);

            Parallel.For(0, distances.Length, i =>
            {
                // ignore filtered frames
                if (InputRanking.Ranks[i] == float.MinValue)
                {
                    distances[i] = float.MinValue;
                    return;
                }
                
                bool[] signature = Signatures[i];
                switch (ellipse.EllipseState)
                {
                    case Ellipse.State.All:
                        double avgRank = 0;
                        foreach (int offset in offsets)
                        {
                            double colorDistance = color ^ signature[offset] ? 1 : 0;
                            avgRank += colorDistance;
                        }
                        distances[i] -= Convert.ToSingle(avgRank / offsets.Length);
                        break;

                    case Ellipse.State.Any:
                        double minRank = int.MaxValue;
                        foreach (int offset in offsets)
                        {
                            double colorDistance = color ^ signature[offset] ? 1 : 0;
                            minRank = Math.Min(minRank, colorDistance);
                        }
                        distances[i] -= Convert.ToSingle(Math.Sqrt(minRank));
                        break;

                    default:
                        throw new NotImplementedException();
                }
            });
            
            return distances;
        }

        private (int[] offsets, bool color) PrepareQuery(Ellipse ellipse)
        {
            double x = ellipse.PositionX * SignatureWidth, y = ellipse.PositionY * SignatureHeight;
            double ax = ellipse.HorizontalAxis * SignatureWidth, ay = ellipse.VerticalAxis * SignatureHeight;
            double ax2 = ax * ax;
            double ay2 = ay * ay;

            List<int> offsets = new List<int>();
            for (int i = 0; i < SignatureWidth; i++)
                for (int j = 0; j < SignatureHeight; j++)
                    if (1 >= (x - i - 0.5) * (x - i - 0.5) / ax2 + (y - j - 0.5) * (y - j - 0.5) / ay2)
                        offsets.Add(j * SignatureWidth + i);

            return (offsets.ToArray(), 
                ellipse.ColorR != 0 || ellipse.ColorG != 0 || ellipse.ColorB != 0);
        }
    }
}
