using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Submission
{
    public interface ISubmissionService
    {
        string SubmissionUrl { get; set; }
        Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit);
        Task<string> SubmitLog();
    }
}
