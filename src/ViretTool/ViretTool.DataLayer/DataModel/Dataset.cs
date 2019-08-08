using System;
using System.Collections.ObjectModel;
using ViretTool.DataLayer.DataIO;

namespace ViretTool.DataLayer.DataModel
{
    /// <summary>
    /// Holds a tree structure of the input video dataset.
    /// 
    /// Simple tree structure description:
    /// A: The videos are split into sequential shots from which were selected its representative frames.
    /// Dataset 
    ///     -> Videos (sequentially ordered) 
    ///     -> Shots (sequentially ordered)
    ///     -> Frames (sequentially ordered)
    /// B: The representative frames are joined into groups of similar frames.
    /// Dataset 
    ///     -> Videos (sequentially ordered)
    ///     -> Groups 
    ///     -> Frames 
    /// 
    /// Extended tree structure explanation (used in the extraction phase):
    /// The representative frames are a subset of a set of playback frames (k-FramesPerSecond).
    /// The playback frames are a subset of all frames from a video.
    /// Dataset 
    ///     -> Videos 
    ///     -> Shots/Groups
    ///     -> (Representative) Frames (of shots) 
    ///     -[subset]-> Playback Frames (k-FPS)
    ///     -[subset]-> All video file frames (variable FPS per video)
    /// </summary>
    public class Dataset
    {
        /// <summary>
        /// DatasetID represents a unique timestamp associated with 
        /// the actual input video dataset and the extraction time.
        /// </summary>
        public readonly byte[] DatasetId;
        // TODO: datasetId into a separate object + ToString() method?
        // TODO: interpret datasetId (datasetName:string + timestamp:DateTime)?        
        public readonly string DatasetName;
        public readonly DateTime DatasetCreationTime;


        // heirarchy
        public readonly ReadOnlyCollection<Video> Videos;
        public readonly ReadOnlyCollection<Shot> Shots;
        public readonly ReadOnlyCollection<Group> Groups;
        public readonly ReadOnlyCollection<Frame> Frames;
        
        
        /// <summary>
        /// Create the dataset structure from already preloaded items.
        /// </summary>
        /// <param name="datasetId">
        ///     An unique identifier associated with the actual input video dataset and the extraction time.</param>
        /// <param name="videos">Objects representing dataset videos (sequentially ordered).</param>
        /// <param name="shots">Objects holding individual shots of videos (sequentially ordered).</param>
        /// <param name="groups">Groups of similar representative frames.</param>
        /// <param name="frames">Representative frames of shots (sequentially ordered).</param>
        public Dataset(byte[] datasetId, Video[] videos, Shot[] shots, Group[] groups, Frame[] frames)
        {
            // TODO: validate datasetId
            DatasetId = datasetId;

            // wrap input arrays into readonly collections.
            Videos = new ReadOnlyCollection<Video>(videos);
            Shots = new ReadOnlyCollection<Shot>(shots);
            Groups = new ReadOnlyCollection<Group>(groups);
            Frames = new ReadOnlyCollection<Frame>(frames);

            foreach (Video video in videos)
            {
                video.ParentDataset = this;
            }

            FileHeaderUtilities.DecodeDatasetID(DatasetId,
                    out string datasetName, out DateTime creationTime);
            DatasetName = datasetName;
            DatasetCreationTime = creationTime;
        }
    }
}
