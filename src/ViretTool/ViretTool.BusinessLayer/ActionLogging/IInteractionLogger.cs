using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ViretTool.BusinessLayer.ActionLogging
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogCategory
    {
        Text, Image, Sketch, Filter, Browsing
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogType
    {
        //text
        JointEmbedding, LocalizedObject,
        //image
        GlobalFeatures,
        //sketch
        Color,
        //Filter
        BW, MaxFrames, Lifelog, ASR,
        //browsing
        RankedList, VideoSummary, TemporalContext, Exploration, ExplicitSort, ResetAll,
        // none
        None
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubmissionType
    {
        Submit, Flush, Result
    }

    public interface IInteractionLogger : IDisposable
    {
        void LogInteraction(LogCategory category, LogType type, string value = null, string attributes = null);
        void ResetLog();
        InteractionLog Log { get; }
        string GetContent();

        long GetLastInteractionTimestamp();
    }
}
