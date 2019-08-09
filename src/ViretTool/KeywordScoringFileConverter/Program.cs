using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DatasetIO;
using ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO;
using ViretTool.DataLayer.DataModel;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion;

namespace KeywordScoringFileConverter
{
    class Program
    {
        private static Random mRandom = new Random();
        private const int MAX_CLAUSE_CACHE_SIZE = 2000;
        private static Dictionary<List<int>, Dictionary<int, float>> mClauseCache;
        private static Dictionary<int, List<KeywordSearchFrame>> mClassCache;
        private static BinaryReader mReader;
        private static Dictionary<int, int> mClassLocations;
        private static LabelProvider mLabelProvider;

        static void Main(string[] args)
        {
            Console.WriteLine("Preloading keywords...");
            LoadFromFile(args[0]);

            Console.WriteLine("Loading labels...");
            mLabelProvider = new LabelProvider(args[1]);

            // load dataset header
            Console.WriteLine("Loading dataset...");
            Dataset dataset = DatasetBinaryFormatter.Instance.Deserialize(File.OpenRead(args[2]));

            int[] synsetToIdMapping = mLabelProvider.Labels.Keys.OrderBy(x => x).ToArray();

            Dictionary<int, float> test = GetRankForOneSynsetClause(ExpandLabel(new List<int>() { 2084071 }));

            using (KeywordScoringWriter writer = new KeywordScoringWriter(args[3], dataset.DatasetId, 
                dataset.Frames.Count, synsetToIdMapping.Length, synsetToIdMapping))
            {
                int lengthSameCheck = -1;
                foreach (int synsetId in synsetToIdMapping)
                {
                    Label label = mLabelProvider.Labels[synsetId];
                    Console.WriteLine($"Processing synset {synsetId} ({label.Name})...");
                    List<int> hypernymSynsetIds = ExpandLabel(new[] { label.SynsetId });

                    Dictionary<int, float> ranks = GetRankForOneSynsetClause(hypernymSynsetIds);

                    float[] scoring = ranks.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
                    if (lengthSameCheck != -1 && lengthSameCheck != scoring.Length)
                    {
                        throw new Exception($"Lengths not equal: {lengthSameCheck} vs { scoring.Length }");
                    }
                    else
                    {
                        lengthSameCheck = scoring.Length;
                    }

                    writer.WriteScoring(scoring);
                }
            }
            Console.WriteLine("Done!");
        }

        // Taken from KeywordSubModel.cs
        private static void LoadFromFile(string inputFile)
        {
            //throw new NotImplementedException();
            mClassLocations = new Dictionary<int, int>();
            mClassCache = new Dictionary<int, List<KeywordSearchFrame>>();
            mClauseCache = new Dictionary<List<int>, Dictionary<int, float>>(new ListComparer());
            

            //LastId = mDataset.LAST_FRAME_TO_LOAD;
            string indexFilename = inputFile;// mDataset.GetFileNameByExtension($"-{mSource}.keyword");

            mReader = new BinaryReader(File.Open(indexFilename, FileMode.Open, FileAccess.Read, FileShare.Read));

            // header = 'KS INDEX'+(Int64)-1
            if (mReader.ReadInt64() != 0x4b5320494e444558 && mReader.ReadInt64() != -1)
                throw new System.IO.InvalidDataException("Invalid index file format.");

            // read offests of each class
            while (true)
            {
                int value = mReader.ReadInt32();
                int valueOffset = mReader.ReadInt32();

                if (value != -1)
                {
                    mClassLocations.Add(value, valueOffset);
                }
                else break;
            }
        }

        private static List<int> ExpandLabel(IEnumerable<int> ids)
        {
            var list = new List<int>();
            foreach (var item in ids)
            {
                var label = mLabelProvider.Labels[item];
                if (label.Id != -1)
                {
                    list.Add(label.Id);
                }

                if (label.Hyponyms != null)
                {
                    list.AddRange(ExpandLabel(label.Hyponyms));
                }
            }

            return list.Distinct().ToList();
        }

        private static Dictionary<int, float> GetRankForOneSynsetClause(List<int> listOfSynsetIds)
        {
            if (mClauseCache.TryGetValue(listOfSynsetIds, out var dict))
            {
                return dict;
            }

            dict = new Dictionary<int, float>();
            for (int i = 0; i < listOfSynsetIds.Count; i++)
            {
                var classFrames = ReadClassFromFile(listOfSynsetIds[i]);
                if (classFrames.Count == 0) return dict;

                
                foreach (KeywordSearchFrame f in classFrames)
                {
                    if (dict.ContainsKey(f.Id))
                    {
                        dict[f.Id] += f.Rank;
                    }
                    else
                    {
                        dict.Add(f.Id, f.Rank);
                    }
                }
            }

            if (MAX_CLAUSE_CACHE_SIZE == mClauseCache.Count)
            {
                var randClass = mClauseCache.Keys.ToList()[mRandom.Next(mClauseCache.Count)];
                mClauseCache.Remove(randClass);
            }

            mClauseCache.Add(listOfSynsetIds, dict);
            return dict;
        }

        private static List<KeywordSearchFrame> ReadClassFromFile(int classId)
        {
            if (!mClassLocations.ContainsKey(classId))
            {
                throw new InvalidDataException("Class ID is incorrect.");
                //return new List<KeywordSearchFrame>();
            }

            var list = new List<KeywordSearchFrame>();

            mReader.BaseStream.Seek(mClassLocations[classId], SeekOrigin.Begin);

            // add all images
            while (true)
            {
                int imageId = mReader.ReadInt32();
                float imageProbability = mReader.ReadSingle();

                if (imageId != -1)
                {
                    //if (imageId > LastId) continue; // skipping frames after a certain frame is now handled differently

                    list.Add(new KeywordSearchFrame(imageId, imageProbability));
                }
                else break;
            }
            
            return list;
        }
        
        struct KeywordSearchFrame
        {
            public int Id { get; }
            public float Rank { get; set; }

            public KeywordSearchFrame(int id, float rank)
            {
                Id = id;
                Rank = rank;
            }
        }

        class ListComparer : IEqualityComparer<List<int>>
        {

            public bool Equals(List<int> x, List<int> y)
            {
                if (x.Count != y.Count) return false;

                for (int i = 0; i < x.Count; i++)
                {
                    if (x[i] != y[i]) return false;
                }
                return true;
            }

            public int GetHashCode(List<int> obj)
            {
                int result = 17;
                for (int i = 0; i < obj.Count; i++)
                {
                    unchecked
                    {
                        result = result * 23 + obj[i];
                    }
                }
                return result;
            }

        }
    }
}
