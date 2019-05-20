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
    public class BoolSketchModelSkeleton : IBoolSketchModel
    {
        public ColorSketchQuery CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }
        
        public BoolSketchModelSkeleton()
        {
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
            

            // perform fusion of partial rankings
            Parallel.For(0, inputRanking.Ranks.Length, itemId =>
            {
                if (InputRanking.Ranks[itemId] == float.MinValue)
                {
                    // ignore filtered frames
                    OutputRanking.Ranks[itemId] = float.MinValue;
                    return;
                }

                OutputRanking.Ranks[itemId] = 0;
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
    }
}
