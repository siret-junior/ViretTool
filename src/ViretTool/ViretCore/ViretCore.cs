using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viret.DataModel;
using Viret.Logging;
using Viret.Ranking;
using Viret.Ranking.ContextAware;
using Viret.Ranking.Knn;
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
        public KnnRanker KnnRankerBert { get; private set; }
        public KnnRanker KnnRankerClip { get; private set; }
        public W2vvBowToVector W2vvBowToVector { get; private set; }
        public W2vvTextToVectorRemote W2vvTextToVectorRemote { get; private set; }
        public ContextAwareRanker ContextAwareRanker { get; private set; }
        public RankingService RankingService { get; private set; }

        public bool IsLoaded { get; private set; } = false;


        public ViretCore()
        {
            InteractionLogger = new InteractionLogger();
            ItemSubmitter = new ItemSubmitter("TODO server URL", "TODO sessionId from config file");
            LogSubmitter = new LogSubmitter("TODO server URL", "TODO sessionId from config file", InteractionLogger);
            W2vvTextToVectorRemote = new W2vvTextToVectorRemote("TODO server URL");
        }

        public void LoadFromDirectory(string inputDirectory, int maxVideos = 75)
        {
            // TODO: tasks, dispose
            Console.WriteLine($"Loading dataset...");
            Dataset = Dataset.FromDirectory(inputDirectory, maxVideos, ".dataset");

            Console.WriteLine($"Loading thumbnails...");
            ThumbnailReader = ThumbnailReader.FromDirectory(inputDirectory, maxVideos, ".thumbnails");

            Console.WriteLine($"Loading kNN ranker...");
            int maxKeyframes = (maxVideos > 0) ? Dataset.Keyframes.Count : -1;
            KnnRankerBert = KnnRanker.FromDirectory(inputDirectory, maxKeyframes, ".w2vv-bert");
            //KnnRankerClip = KnnRanker.FromDirectory(inputDirectory, maxKeyframes, ".w2vv-clip");

            Console.WriteLine($"Loading W2VV BOW to vector...");
            W2vvBowToVector = W2vvBowToVector.FromDirectory(inputDirectory, "w2vv");

            Console.WriteLine($"Loading context-aware ranker...");
            int[] videoLastFrameIds = Dataset.Videos.Select(video => video.Keyframes.Last().Id).ToArray();
            ContextAwareRanker = new ContextAwareRanker(KnnRankerBert.Vectors, videoLastFrameIds);
            RankingService = new RankingService(W2vvBowToVector, W2vvTextToVectorRemote, ContextAwareRanker);

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
