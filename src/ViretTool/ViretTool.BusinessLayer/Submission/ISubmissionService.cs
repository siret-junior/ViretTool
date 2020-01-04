using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.Submission
{
    public interface ISubmissionService
    {
        string SubmissionUrl { get; set; }
        Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit);
        Task<string> SubmitLogAsync();
        Task<string> SubmitResultsAsync(BiTemporalQuery query, BiTemporalRankedResultSet results, long unixTimestamp);
    }
}
