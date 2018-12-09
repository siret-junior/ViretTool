namespace ViretTool.BusinessLayer.Datasets
{
    public interface IDatasetService
    {
        int[] VideoIds { get; }
        int[] GetFrameIdsForVideo(int videoId);
    }
}
