using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNKeywords
{
    public class KeywordModel : IKeywordModel
    {
        public KeywordQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; set; }
        public RankingBuffer OutputRanking { get; set; }

        private IKeywordScoringProvider _keywordScoringProvider;


        public KeywordModel(IKeywordScoringProvider keywordScoringProvider)
        {
            _keywordScoringProvider = keywordScoringProvider;
        }


        public void ComputeRanking(KeywordQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking)
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
                //Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
                // initialize empty keyword model to 1
                Parallel.For(0, inputRanking.Ranks.Length, i =>
                {
                    if (inputRanking.Ranks[i] == float.MinValue)
                    {
                        OutputRanking.Ranks[i] = float.MinValue;
                    }
                    else
                    {
                        OutputRanking.Ranks[i] = 1;
                    }
                });
            }
            else
            {
                // compute scoring
                //float[] scoring = GetScoring(query.Query);
                float[] scoring = inputRanking.Ranks;

                // propagate filtered frames
                Parallel.For(0, inputRanking.Ranks.Length, i =>
                {
                    if (inputRanking.Ranks[i] == float.MinValue)
                    {
                        OutputRanking.Ranks[i] = float.MinValue;
                    }
                    else
                    {
                        OutputRanking.Ranks[i] = scoring[i];
                    }
                });
            }
        }

        private bool HasQueryOrInputChanged(KeywordQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(KeywordQuery query)
        {
            return query == null || !query.Query.Any();
        }


        public float[] GetScoring(string[] query)
        {
            throw new NotImplementedException();
        }

        //public float[] GetScoring(Synset synsetLiteral)
        //{
        //    return _keywordScoringProvider.GetScoring(synsetLiteral.SynsetId);
        //}

        //public float[] GetScoring(SynsetClause synsetClause)
        //{
        //    // initialize
        //    float[] result = new float[InputRanking.Ranks.Length];

        //    // accumulate
        //    foreach (Synset synsetLiteral in synsetClause.SynsetLiterals)
        //    {
        //        float[] scoring = GetScoring(synsetLiteral);
        //        Parallel.For(0, scoring.Length, index =>
        //        {
        //            result[index] += scoring[index];
        //        });
        //    }
        //    return result;
        //}

        //public float[] GetScoring(SynsetClause[] synsetFormula)
        //{
        //    // initialize
        //    float[] result = new float[InputRanking.Ranks.Length];
        //    Parallel.For(0, result.Length, index =>
        //    {
        //        result[index] = 1;
        //    });

        //    // accumulate
        //    foreach (SynsetClause synsetClause in synsetFormula)
        //    {
        //        float[] scoring = GetScoring(synsetClause);
        //        Parallel.For(0, scoring.Length, index =>
        //        {
        //            result[index] *= scoring[index];
        //        });
        //    }
        //    return result;
        //}

    }
}
