using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DatasetIO;
using ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO;
using ViretTool.DataLayer.DataModel;

namespace KeywordFileDeinverter
{
    class Program
    {
        static List<(int synsetId, float rank)>[] _keywords;
        static Dictionary<int, int> _classOffsets;
        


        static void Main(string[] args)
        {
            string datasetFile = args[0];
            string inputFile = args[1];
            string outputFile = args[2];
            int topKSynsets = int.Parse(args[3]);

            
            // load dataset header
            Console.WriteLine("Loading dataset...");
            Dataset dataset = DatasetBinaryFormatter.Instance.Deserialize(File.OpenRead(datasetFile));
            int frameCount = dataset.Frames.Count;
            _keywords = new List<(int synsetId, float rank)>[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                _keywords[i] = new List<(int synsetId, float rank)>();
            }

            // convert data
            using (BinaryReader reader = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // check header = 'KS INDEX'+(Int64)-1
                if (reader.ReadInt64() != 0x4b5320494e444558 && /* why? */ reader.ReadInt64() != -1)
                {
                    throw new InvalidDataException("Invalid index file format.");
                }

                // read offsets of each class
                Console.WriteLine("Reading offsets...");
                _classOffsets = new Dictionary<int, int>();
                while (true)
                {
                    int classId = reader.ReadInt32();
                    int classOffset = reader.ReadInt32();

                    if (classId == -1)
                    {
                        break;
                    }
                    _classOffsets.Add(classId, classOffset);
                }

                // read all classes
                Console.WriteLine("Reading classes...");
                foreach (int synsetId in _classOffsets.Keys.OrderBy(x => x))
                {
                    foreach ((int frameId, float frameRank) in ReadFrameRanks(reader, synsetId))
                    {
                        if (frameId >= frameCount)
                        {
                            throw new InvalidDataException("Invalid index file format.");
                        }
                        
                        _keywords[frameId].Add((synsetId, frameRank));
                    }
                }


                // store all classes
                Console.WriteLine("Writing converted file...");
                using (KeywordWriter writer = new KeywordWriter(outputFile, dataset.DatasetId, frameCount))
                {
                    for (int i = 0; i < frameCount; i++)
                    {
                        writer.WriteDescriptor(_keywords[i].OrderByDescending(x => x.rank).Take(topKSynsets).ToArray());
                    }
                }
            }
        }


        static List<(int frameId, float frameRank)> ReadFrameRanks(BinaryReader reader, int synsetId)
        {
            if (!_classOffsets.ContainsKey(synsetId))
            {
                throw new ArgumentOutOfRangeException("Class ID is incorrect.");
            }

            // TODO: move to property + constructor parameter
            int LIST_DEFAULT_SIZE = 32768;
            List<(int frameId, float rank)> result = new List<(int frameId, float rank)>(LIST_DEFAULT_SIZE);

            reader.BaseStream.Seek(_classOffsets[synsetId], SeekOrigin.Begin);

            // read frame ranks until stop flag (-1)
            while (true)
            {
                int frameId = reader.ReadInt32();
                float frameRank = reader.ReadSingle();

                if (frameId != -1)
                {
                    result.Add((frameId, frameRank));
                }
                else break;
            }
            
            return result;
        }

    }
}
