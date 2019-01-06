using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.Datasets
{
    public interface IDatasetService
    {
        Dataset Dataset { get; }
        int FrameCount { get; }
        int[] VideoIds { get; }
        int[] GetFrameIdsForVideo(int videoId);
        int[] GetFrameNumbersForVideo(int videoId);
        int GetVideoIdForFrameId(int frameId);
        int GetFrameNumberForFrameId(int frameId);
        bool TryGetFrameIdForFrameNumber(int videoId, int frameNumber, out int frameId);
        int GetFrameIdForFrameNumber(int videoId, int frameNumber);
        (int StartFrame, int EndFrame)[] GetShotFrameNumbersForVideo(int videoId);
        int GetLastFrameIdForVideo(int videoId);
        int[] GetLastFrameIdsInVideoForFrameId();
        int[] GetFirstFrameIdsInVideoForFrameId();
        int GetShotNumberForFrameId(int frameId);
    }
}
