using System.Collections.ObjectModel;

namespace ViretTool.DataLayer.DataModel
{
    /// <summary>
    /// Holds a tree structure of the input video dataset.
    /// 
    /// Simple tree structure description:
    /// The videos are split into sequential shots from which were selected its representative frames.
    /// Dataset 
    ///     -> Videos (sequentially ordered) 
    ///     -> Shots (sequentially ordered)
    ///     -> Frames (sequentially ordered)
    /// 
    /// Extended tree structure explanation (used in the extraction phase):
    /// The representative frames are a subset of a set of playback frames (k-FramesPerSecond).
    /// The playback frames are a subset of all frames from a video.
    /// Dataset 
    ///     -> Videos 
    ///     -> Shots
    ///     -> (Representative) Frames (of shots) 
    ///     -[subset]-> Playback Frames (k-FPS)
    ///     -[subset]-> All video file frames (variable FPS per video)
    /// </summary>
    public class Dataset
    {
        // hierarchy
        public readonly ReadOnlyCollection<Video> Videos;
        public readonly ReadOnlyCollection<Shot> Shots;
        public readonly ReadOnlyCollection<Frame> Frames;
        
        
        /// <summary>
        /// Create the dataset structure from already preloaded items.
        /// </summary>
        /// <param name="videos">Objects representing dataset videos (sequentially ordered).</param>
        /// <param name="shots">Objects holding individual shots of videos (sequentially ordered).</param>
        /// <param name="frames">Representative frames of shots (sequentially ordered).</param>
        public Dataset(Video[] videos, Shot[] shots, Frame[] frames)
        {
            // wrap input arrays into readonly collections.
            Videos = new ReadOnlyCollection<Video>(videos);
            Shots = new ReadOnlyCollection<Shot>(shots);
            Frames = new ReadOnlyCollection<Frame>(frames);

            foreach (Video video in videos)
            {
                video.ParentDataset = this;
            }
        }
    }
}
