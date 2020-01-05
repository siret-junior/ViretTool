using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public class TranscriptFilter : ITranscriptFilter
    {
        private IDatasetServicesManager _datasetServicesManager { get; }
        private readonly string[] _videoTranscripts;

        public TranscriptFilter(IDatasetServicesManager datasetServicesManager, ITranscriptProvider transcriptProvider)
        {
            _datasetServicesManager = datasetServicesManager;
            _videoTranscripts = transcriptProvider.GetTranscripts();
        }


        public bool[] GetFilterMask(string query)
        {
            if (query == null || query.Equals(""))
            {
                return null;
            }

            bool[] videoMask = FilterVideos(query);

            int frameCount = _datasetServicesManager.CurrentDataset.DatasetService.FrameCount;
            bool[] result = new bool[frameCount];

            for (int iVideo = 0; iVideo < videoMask.Length; iVideo++)
            {
                if (videoMask[iVideo])
                {
                    // include
                    foreach (int frameId in _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdsForVideo(iVideo))
                    {
                        result[frameId] = true;
                    }
                }
            }
           
            return result;
        }

        // query is a string of phrases separated by '+'
        private bool[] FilterVideos(string query)
        {
            int[] resultCounts = new int[_videoTranscripts.Length];

            query = query.ToLower();
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
