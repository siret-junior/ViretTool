namespace ViretTool.BusinessLayer.Datasets
{
    public interface IDatasetService
    {
        int[] VideoIds { get; }
        int[] GetFrameIdsForVideo(int videoId);
        int[] GetFrameNumbersForVideo(int videoId);
        int GetVideoIdForFrameId(int frameId);
        int GetFrameNumberForFrameId(int frameId);
        bool TryGetFrameIdForFrameNumber(int videoId, int frameNumber, out int frameId);
        int GetFrameIdForFrameNumber(int videoId, int frameNumber);
    }
}
