using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Submission
{
    public interface ISubmissionService
    {
        Task<string> SubmitFramesAsync(int teamId, int memberId, FrameToSubmit frameToSubmit);
    }
}