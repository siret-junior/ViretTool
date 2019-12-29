using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public interface IKeywordScoringProvider
    {
        int ScoringVectorSize { get; }
        int ScoringCount { get; }

        //Dictionary<int, float[]> Scorings { get; }
        Dictionary<int, (int frameId, float score)[]> TopScorings { get; }

        //float[] GetScoring(string[] query);

        (int frameId, float scoring)[] GetTopScoring(int synsetId);
    }
}
