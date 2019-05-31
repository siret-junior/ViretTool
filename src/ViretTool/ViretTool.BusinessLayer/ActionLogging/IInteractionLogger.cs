namespace ViretTool.BusinessLayer.ActionLogging
{
    public enum LogCategory
    {
        Text, Image, Sketch, Filter, Browsing
    }

    public enum LogType
    {
        //text
        Concept, LocalizedObject,
        //image
        GlobalFeatures,
        //sketch
        Color,
        //Filter
        BW, MaxFrames, Lifelog,
        //browsing
        RankedList, VideoSummary, TemporalContext, Exploration, ExplicitSort, ResetAll

    }

    public enum SubmissionType
    {
        Submit, Flush
    }

    public interface IInteractionLogger
    {
        void LogInteraction(LogCategory category, LogType type, string value = null, string attributes = null);
        void ResetLog();
        InteractionLog Log { get; }
    }
}
