using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO;

namespace ViretTool.BusinessLayer.Descriptors.KeywordTopScoring
{
    public class KeywordTopScoringFactory
    {
        public const string SYNSET_FRAMES_EXTENSION = ".synsetframes";

        public static IKeywordTopScoringProvider FromDirectory(string directory, int topKScoreCount = 0)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(SYNSET_FRAMES_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return FromFile(inputFile);
            }
            else
            {
                return new KeywordTopScoringProviderDummy();
            }
        }


        public static IKeywordTopScoringProvider FromFile(string inputFile)
        {
            // read top scores per synset
            Dictionary<int, (int frameId, float score)[]>  topScorings = new Dictionary<int, (int frameId, float score)[]>();
            using (SynsetFramesReader reader = new SynsetFramesReader(inputFile))
            {
                for (int i = 0; i < reader.ScoringCount; i++)
                {
                    int synsetId = reader.IdToSynsetIdMapping[i];
                    topScorings[synsetId] = reader.ReadSynsetFrames(i);
                }

                return new KeywordTopScoringProvider(topScorings, reader.ScoringVectorSize, reader.ScoringCount);
            }

        }
    }
}
