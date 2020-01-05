using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public class TranscriptFilter : ITranscriptFilter
    {
        private IDatasetServicesManager _datasetServicesManager { get; }
        private readonly string[] _videoTranscripts;

        public TranscriptFilter(IDatasetServicesManager datasetServicesManager, string[] videoTranscripts)
        {
            _datasetServicesManager = datasetServicesManager;
            _videoTranscripts = videoTranscripts;

            _datasetServicesManager.DatasetOpened += (_, services) =>
            {
                _maxFramesFromVideo = services.DatasetParameters.IsLifelogData ? 50 : 3;
                NotifyOfPropertyChange(nameof(MaxFramesFromVideo));
            };

            for (int i = 0; i < _videoTranscripts.Length; i++)
                _videoTranscripts[i] = _videoTranscripts[i].Replace("~", " ");
        }

        public static TranscriptFilter FromDirectory(string path)
        {
            return new TranscriptFilter(Path.Combine(path, "V3C1-videoTranscript.txt"));
        }

        public bool[] GetFilterMask(string query)
        {
            bool[] videoMask = FilterVideos(query);



            return videoMask;
        }

        // query is a string of phrases separated by '+'
        private bool[] FilterVideos(string query)
        {
            int[] resultCounts = new int[_videoTranscripts.Length];

            string[] queryPhrases = query.Split('+');
            for (int i = 0; i < resultCounts.Length; i++)
                resultCounts[i] = FilterVideo(queryPhrases, i);

            int max = resultCounts.Max();

            // true = keep video, false = filter video
            bool[] resultFilter = new bool[_videoTranscripts.Length];
            for (int i = 0; i < resultFilter.Length; i++)
                resultFilter[i] = (resultCounts[i] == max);

            return resultFilter;
        }

        private int FilterVideo(string[] queryPhrases, int idx)
        {
            int count = 0;

            foreach (string queryPhrase in queryPhrases)
                if (_videoTranscripts[idx].Contains(queryPhrase))
                    count++;

            return count;
        }
    }
}
