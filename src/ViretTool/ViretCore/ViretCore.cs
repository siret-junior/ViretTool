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
        public KnnRanker KnnRanker { get; private set; }
        public W2vvBowToVector W2vvBowToVector { get; private set; }
        public ContextAwareRanker ContextAwareRanker { get; private set; }
        public RankingService RankingService { get; private set; }

        public bool IsLoaded { get; private set; } = false;


        public ViretCore()
        {
            InteractionLogger = new InteractionLogger();
            ItemSubmitter = new ItemSubmitter("TODO", "TODO from config file");
            LogSubmitter = new LogSubmitter("TODO", "TODO from config file", InteractionLogger);
        }

        public void LoadFromDirectory(string inputDirectory)
        {
            // TODO: tasks, dispose
            Console.WriteLine($"Loading dataset...");
            Dataset = Dataset.FromDirectory(inputDirectory);
            Console.WriteLine($"Loading thumbnails...");
            ThumbnailReader = ThumbnailReader.FromDirectory(inputDirectory);
            Console.WriteLine($"Loading kNN ranker...");
            KnnRanker = KnnRanker.FromDirectory(inputDirectory);
            Console.WriteLine($"Loading W2VV BOW to vector...");
            W2vvBowToVector = W2vvBowToVector.FromDirectory(inputDirectory);
            Console.WriteLine($"Loading context-aware ranker...");
            ContextAwareRanker = new ContextAwareRanker(KnnRanker.Vectors, Dataset.Videos.Select(video => video.Keyframes.Last().Id).ToArray());
            RankingService = new RankingService(W2vvBowToVector, ContextAwareRanker);

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
