using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viret.DataModel;
using Viret.Logging;
using Viret.Ranking;
using Viret.Ranking.ContextAware;
using Viret.Ranking.Features;
using Viret.Ranking.W2VV;
using Viret.Submission;
using Viret.Thumbnails;

namespace Viret
{
    /// <summary>
    /// Loads and holds all core modules.
    /// </summary>
    public class ViretCore : IDisposable
    {
        public InteractionLogger InteractionLogger { get; private set; }
        public ItemSubmitter ItemSubmitter { get; private set; }
        public LogSubmitter LogSubmitter { get; private set; }
        
        public Dataset Dataset { get; private set; }
        public ThumbnailReader ThumbnailReader { get; private set; }

        public FeatureVectors FeatureVectorsW2vv { get; private set; }
        public FeatureVectors FeatureVectorsBert { get; private set; }
        public FeatureVectors FeatureVectorsClip { get; private set; }

        public BowToVectorW2vv BowToVectorW2vv { get; private set; }
        public TextToVectorRemote TextToVectorRemoteBert { get; private set; }
        public TextToVectorRemote TextToVectorRemoteClip { get; private set; }

        public ContextAwareRanker ContextAwareRanker { get; private set; }
        public RankingService RankingService { get; private set; }

        public bool IsLoaded { get; private set; } = false;


        public ViretCore()
        {
            InteractionLogger = new InteractionLogger();
            ItemSubmitter = new ItemSubmitter("TODO server URL", "TODO sessionId from config file");
            LogSubmitter = new LogSubmitter("TODO server URL", "TODO sessionId from config file", InteractionLogger);
        }

        // TODO: tasks, dispose
        public void LoadFromDirectory(string inputDirectory, int maxVideos = -1)
        {
            // base
            Dataset = Dataset.FromDirectory(inputDirectory, maxVideos, ".dataset");
            ThumbnailReader = ThumbnailReader.FromDirectory(inputDirectory, maxVideos, ".thumbnails");
            int maxKeyframes = (maxVideos > 0) ? Dataset.Keyframes.Count : -1;

            // features
            FeatureVectorsW2vv = FeatureVectors.FromDirectory(inputDirectory, maxKeyframes, ".w2vv");
            //FeatureVectorsBert = FeatureVectors.FromDirectory(inputDirectory, maxKeyframes, ".bert");
            //FeatureVectorsClip = FeatureVectors.FromDirectory(inputDirectory, maxKeyframes, ".clip");

            // text to vector services
            BowToVectorW2vv = BowToVectorW2vv.FromDirectory(inputDirectory, "w2vv");
            //TextToVectorRemoteBert = new TextToVectorRemote(inputDirectory, "bert", "TODO server URL");
            //TextToVectorRemoteClip = new TextToVectorRemote(inputDirectory, "clip", "TODO server URL");

            // ranking
            int[] videoLastFrameIds = Dataset.Videos.Select(video => video.Keyframes.Last().Id).ToArray();
            ContextAwareRanker = new ContextAwareRanker(FeatureVectorsW2vv.Vectors, videoLastFrameIds);
            RankingService = new RankingService(this);

            IsLoaded = true;
        }

        public void Dispose()
        {
            // static
            InteractionLogger.Dispose();
            ItemSubmitter.Dispose();
            LogSubmitter.Dispose();

            // dataset dependent
            ThumbnailReader?.Dispose();
        }
    }
}
