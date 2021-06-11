using System;
using System.Collections.Generic;
using System.IO;
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
        // TODO: error/warn/info logger
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

        public ContextAwareRanker ContextAwareRankerW2vv { get; private set; }
        public ContextAwareRanker ContextAwareRankerBert { get; private set; }
        public ContextAwareRanker ContextAwareRankerClip { get; private set; }
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
            Dataset = Dataset.FromDirectory(inputDirectory, "frame-ID-to-filepath.*.csv", maxVideos);
            ThumbnailReader = ThumbnailReader.FromDirectory(inputDirectory, "*.thumbnails", maxVideos);
            int maxKeyframes = (maxVideos > 0) ? Dataset.Keyframes.Count : -1;

            // features
            FeatureVectorsW2vv = FeatureVectors.FromDirectory(Path.Combine(inputDirectory, "W2VV_BoW"), "frame-features.*.bin", maxKeyframes);
            FeatureVectorsBert = FeatureVectors.FromDirectory(Path.Combine(inputDirectory, "W2VV_BERT"), "frame-features.*.bin", maxKeyframes);
            FeatureVectorsClip = FeatureVectors.FromDirectory(Path.Combine(inputDirectory, "CLIP"), "frame-features.*.bin", maxKeyframes);

            // text to vector services
            BowToVectorW2vv = BowToVectorW2vv.FromDirectory(Path.Combine(inputDirectory, "W2VV_BoW"), 
                "keyword-to-ID.*.csv", 
                "keyword-dense-weigths.*.float32.bin", 
                "keyword-dense-bias.*.float32.bin", 
                "PCA-matrix.*.float32.bin", 
                "PCA-mean.*.float32.bin",
                vectorDimension: 2048);
            TextToVectorRemoteBert = TextToVectorRemote.FromDirectory(Path.Combine(inputDirectory, "W2VV_BERT"), 
                "server.url", 
                "PCA-matrix.*.float32.bin", 
                "PCA-mean.*.float32.bin",
                vectorDimension: 2048);
            TextToVectorRemoteClip = TextToVectorRemote.FromDirectory(Path.Combine(inputDirectory, "CLIP"),
                "server.url",
                "PCA-matrix.*.float32.bin",
                "PCA-mean.*.float32.bin",
                vectorDimension: 640);

            // ranking
            int[] videoLastFrameIds = Dataset.Videos.Select(video => video.Keyframes.Last().Id).ToArray();
            ContextAwareRankerW2vv = FeatureVectorsW2vv == null ? null : new ContextAwareRanker(FeatureVectorsW2vv.Vectors, videoLastFrameIds);
            ContextAwareRankerBert = FeatureVectorsBert == null ? null : new ContextAwareRanker(FeatureVectorsBert.Vectors, videoLastFrameIds);
            ContextAwareRankerClip = FeatureVectorsClip == null ? null : new ContextAwareRanker(FeatureVectorsClip.Vectors, videoLastFrameIds);
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
