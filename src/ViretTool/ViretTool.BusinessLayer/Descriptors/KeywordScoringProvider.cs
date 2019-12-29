using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO;

namespace ViretTool.BusinessLayer.Descriptors
{
    // TODO: consider generalizing scores similarly to descriptor providers
    // (provides precomputed score for a query)
    public class KeywordScoringProvider : IKeywordScoringProvider
    {
        //public Dictionary<int, float[]> Scorings { get; }
        public Dictionary<int, (int frameId, float score)[]> TopScorings { get; }

        public int ScoringVectorSize { get; private set; }
        public int ScoringCount { get; private set; }


        public KeywordScoringProvider(string inputFile, string topKFile)
        {
            //using (KeywordScoringReader reader = new KeywordScoringReader(inputFile))
            //{
            //    ScoringVectorSize = reader.ScoringVectorSize;
            //    ScoringCount = reader.ScoringCount;

            //    Scorings = new Dictionary<int, float[]>();
            //    for (int i = 0; i < reader.ScoringCount; i++)
            //    {
            //        int synsetId = reader.IdToSynsetIdMapping[i];
            //        Scorings[synsetId] = reader.ReadScoring(i);
            //    }
            //}

            // read top scores per synset
            TopScorings = new Dictionary<int, (int frameId, float score)[]>();
            using (SynsetFramesReader reader = new SynsetFramesReader(topKFile))
            {
                ScoringVectorSize = reader.ScoringVectorSize;
                ScoringCount = reader.ScoringCount;

                for (int i = 0; i < reader.ScoringCount; i++)
                {
                    int synsetId = reader.IdToSynsetIdMapping[i];
                    TopScorings[synsetId] = reader.ReadSynsetFrames(i);
                }
            }
        }

        public static KeywordScoringProvider FromDirectory(string directory, int topKScoreCount = 0)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(KeywordScoringIOBase.KEYWORD_SCORING_EXTENSION))
                    .FirstOrDefault();
            string topKFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(SynsetFramesIOBase.SYNSET_FRAMES_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new KeywordScoringProvider(inputFile, topKFile);
            }
            else
            {
                return null;
            }
        }


        

        //public float[] GetScoring(int frameId)
        //{
        //    float[] result;
        //    try
        //    {
        //        Scorings.TryGetValue(frameId, out result);
        //    }
        //    catch
        //    {
        //        throw new ArgumentNullException($"No keyword scoring found for frameId {frameId}");
        //        //return new float[0];
        //    }
        //    return result;
        //}

        public (int frameId, float scoring)[] GetTopScoring(int synsetId)
        {
            (int frameId, float scoring)[] result;
            try
            {
                TopScorings.TryGetValue(synsetId, out result);
            }
            catch
            {
                throw new ArgumentNullException($"No keyword scoring found for synsetId {synsetId}");
                //return new (int frameId, float scoring)[0];
            }
            return result;
        }
    }
}
