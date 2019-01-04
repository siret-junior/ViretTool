using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Submission
{
    public interface ISubmissionService
    {
        Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit);
        Task<string> SubmitLog();
    }
}
