using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Submission
{
    public interface ISubmissionService
    {
        Task<string> SubmitFramesAsync(FrameToSubmit frameToSubmit);
        Task<string> SubmitLog();
    }
}
