using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models.DCNNKeywords
{
    /// <summary>
    /// Searches an index file and displays results
    /// </summary>
    public class KeywordSubModel : IKeywordModel
    {
        private const string KEYWORD_MODEL_EXTENSION = ".keyword";

        public KeywordQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; set; }
        public RankingBuffer OutputRanking { get; set; }


        private bool mUseIDF;
        private string mSource;
        
        private int LastId;
        private BinaryReader mReader;
        private Dictionary<int, int> mClassLocations;
        //private Task mLoadTask;

        private float[] IDF;

        private Random mRandom = new Random();
        private Dictionary<int, List<KeywordSearchFrame>> mClassCache;
        private const int CACHE_DELETE = 10;
        private const int MAX_CACHE_SIZE = 100;
        private const int LIST_DEFAULT_SIZE = 32768;

        private const int MAX_CLAUSE_CACHE_SIZE = 10;
        private Dictionary<List<int>, Dictionary<int, float>> mClauseCache;

        /// <param name="lp">For class name to class id conversion</param>
        /// <param name="filePath">Relative or absolute path to index file</param>
        public KeywordSubModel(string inputFile, string source, bool useIDF = false) {
            mSource = source;
            mUseIDF = useIDF;

            //mLoadTask = Task.Factory.StartNew(LoadFromFile);
            LoadFromFile(inputFile);
        }

        public static KeywordSubModel FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(KEYWORD_MODEL_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new KeywordSubModel(inputFile, "TODO: NasNet");
            }
            else
            {
                return null;
            }
        }


        #region Rank Methods

        public void ComputeRanking(KeywordQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking)//List<List<int>> query) 
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

            if (!HasQueryOrInputChanged(query, inputRanking))
            {
                // nothing changed, OutputRanking contains cached data from previous computation
                OutputRanking.IsUpdated = false;
                return;
            }
            else
            {
                CachedQuery = query;
                OutputRanking.IsUpdated = true;
            }

            if (IsQueryEmpty(query))
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
                return;
            }

            Tuple<int, RankingBuffer> result = ComputeRankedFrames(query);
            OutputRanking.Ranks = result.Item2.Ranks;
        }

        private bool HasQueryOrInputChanged(KeywordQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(KeywordQuery query)
        {
            return query == null || !query.SynsetGroups.Any();
        }

        #endregion


        #region (Private) Index File Loading

        private void LoadFromFile(string inputFile) {
            //throw new NotImplementedException();
            mClassLocations = new Dictionary<int, int>();
            mClassCache = new Dictionary<int, List<KeywordSearchFrame>>();
            mClauseCache = new Dictionary<List<int>, Dictionary<int, float>>(new DCNNKeywords.ListComparer());

            if (mUseIDF)
            {
                throw new NotImplementedException();
                //string idfFilename = mDataset.GetFileNameByExtension($"-{mSource}.keyword.idf");
                //IDF = DCNNKeywords.IDFLoader.LoadFromFile(idfFilename);
            }

            //LastId = mDataset.LAST_FRAME_TO_LOAD;
            string indexFilename = inputFile;// mDataset.GetFileNameByExtension($"-{mSource}.keyword");

            mReader = new BinaryReader(File.Open(indexFilename, FileMode.Open, FileAccess.Read, FileShare.Read));

            // header = 'KS INDEX'+(Int64)-1
            if (mReader.ReadInt64() != 0x4b5320494e444558 && mReader.ReadInt64() != -1)
                throw new FileFormatException("Invalid index file format.");

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

        private List<KeywordSearchFrame> ReadClassFromFile(int classId) {
            if (mClassCache.ContainsKey(classId))
                return mClassCache[classId];

            if (!mClassLocations.ContainsKey(classId)) {
                //throw new FileFormatException("Class ID is incorrect.");
                return new List<KeywordSearchFrame>();
            }

            var list = new List<KeywordSearchFrame>(LIST_DEFAULT_SIZE);

            mReader.BaseStream.Seek(mClassLocations[classId], SeekOrigin.Begin);

            // add all images
            while (true) {
                int imageId = mReader.ReadInt32();
                float imageProbability = mReader.ReadSingle();

                if (imageId != -1) {
                    //if (imageId > LastId) continue; // skipping frames after a certain frame is now handled differently

                    list.Add(new KeywordSearchFrame(imageId, imageProbability));
                } else break;
            }

            if (mClassCache.Count == MAX_CACHE_SIZE) {
                for (int i = 0; i < CACHE_DELETE; i++) {
                    var randClass = mClassCache.Keys.ToList()[mRandom.Next(mClassCache.Count)];
                    mClassCache.Remove(randClass);
                }
            }
            mClassCache.Add(classId, list);

            return list;
        }

        #endregion

        #region (Private) List Unions & Multiplications

        private Tuple<int, RankingBuffer> ComputeRankedFrames(KeywordQuery query)//List<List<int>> query) 
        {
            // TODO
            RankingBuffer result = RankingBuffer.Zeros("TODO", InputRanking.Ranks.Length);
            
            List<Dictionary<int, float>> clauses = ResolveClauses(query.SynsetGroups);
            Dictionary<int, float> queryClause = UniteClauses(clauses);

            foreach (KeyValuePair<int, float> pair in queryClause)
            {
                result.Ranks[pair.Key] = pair.Value;
            }

            return new Tuple<int, RankingBuffer>(queryClause.Count, result);
        }


        private Dictionary<int, float> UniteClauses(List<Dictionary<int, float>> clauses) {
            var result = clauses[clauses.Count - 1];
            clauses.RemoveAt(clauses.Count - 1);

            foreach (Dictionary<int, float> clause in clauses) {
                Dictionary<int, float> tempResult = new Dictionary<int, float>();

                foreach (KeyValuePair<int, float> rf in clause) {
                    float rfFromResult;
                    if (result.TryGetValue(rf.Key, out rfFromResult)) {
                        tempResult.Add(rf.Key, rf.Value * rfFromResult);
                    }
                }
                result = tempResult;
            }
            return result;
        }

        private List<Dictionary<int, float>> ResolveClauses(SynsetGroup[] synsetGroups) {
            List<List<int>> ids = synsetGroups.Select(g => g.Synsets.Select(s => s.SynsetId).ToList()).ToList();

            var list = new List<Dictionary<int, float>>();

            // should be fast
            // http://alicebobandmallory.com/articles/2012/10/18/merge-collections-without-duplicates-in-c
            foreach (List<int> listOfIds in ids) {
                if (mClauseCache.ContainsKey(listOfIds)) {
                    list.Add(mClauseCache[listOfIds]);
                    continue;
                }

                Dictionary<int, float> dict = new Dictionary<int, float>(); //= mClasses[listOfIds[i]].ToDictionary(f => f.Frame.ID);
                
                for (int i = 0; i < listOfIds.Count; i++) {
                    var classFrames = ReadClassFromFile(listOfIds[i]);
                    if (classFrames.Count == 0) continue;

                    if (mUseIDF) {
                        float idf = IDF[listOfIds[i]];
                        foreach (KeywordSearchFrame f in classFrames) {
                            if (dict.ContainsKey(f.Id)) {
                                dict[f.Id] += f.Rank * idf;
                            } else {
                                dict.Add(f.Id, f.Rank * idf);
                            }
                        }
                    } else {
                        foreach (KeywordSearchFrame f in classFrames) {
                            if (dict.ContainsKey(f.Id)) {
                                dict[f.Id] += f.Rank;
                            } else {
                                dict.Add(f.Id, f.Rank);
                            }
                        }
                    }
                }

                if (MAX_CLAUSE_CACHE_SIZE == mClauseCache.Count) {
                    var randClass = mClauseCache.Keys.ToList()[mRandom.Next(mClauseCache.Count)];
                    mClauseCache.Remove(randClass);
                }
                mClauseCache.Add(listOfIds, dict);

                list.Add(dict);
            }
            return list;
        }

        #endregion

    }

}
