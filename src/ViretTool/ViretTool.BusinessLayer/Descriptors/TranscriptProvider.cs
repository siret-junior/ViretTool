using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class TranscriptProvider : ITranscriptProvider
    {
        private readonly string[] _videoTranscripts;

        public TranscriptProvider(string[] videoTranscripts)
        {
            _videoTranscripts = videoTranscripts;
        }

        public static TranscriptProvider FromDirectory(string directory)
        {
            string[] videoTranscripts = File
                .ReadAllLines(Path.Combine(directory, "V3C1-videoTranscript.txt"))
                .Select(line => line.Replace("~", " ").ToLower())
                .ToArray();
            return new TranscriptProvider(videoTranscripts);
        }

        public string[] GetTranscripts()
        {
            return _videoTranscripts;
        }
    }
}
