using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.W2VV
{
    public class BowToVectorW2vv
    {
        private const int VECTOR_DIMENSION = 2048;

        private readonly Dictionary<string, int> _wordToIdDictionary;
        private readonly ConcurrentDictionary<string, float[]> _queryVectorCache = new ConcurrentDictionary<string, float[]>();
        private readonly float[][] _wordIdToVector;
        private readonly float[] _biasVector;
        private readonly PcaConversion _pcaConversion;

        public BowToVectorW2vv(string inputDirectory)
        {
            _wordToIdDictionary = LoadDictionary(Path.Combine(inputDirectory, "word2idx.txt"));
            _wordIdToVector = LoadFloatTable(Path.Combine(inputDirectory, "txt_weight-11147x2048floats.bin"), VECTOR_DIMENSION);
            _biasVector = LoadFloatTable(Path.Combine(inputDirectory, "txt_bias-2048floats.bin"), VECTOR_DIMENSION)[0];

            _pcaConversion = new PcaConversion(inputDirectory, VECTOR_DIMENSION);
        }

        public static BowToVectorW2vv FromDirectory(string inputDirectory, string subDirectory = "w2vv")
        {
            try
            {
                return new BowToVectorW2vv(Path.Combine(inputDirectory, subDirectory));
            }
            catch
            {
                // TODO: temporarily fail silently
                return null;
            }
        }

        public bool ContainsWord(string word)
        {
            return _wordToIdDictionary.ContainsKey(word);
        }

        public float[] BowToVector(string query)
        {
            return BowToVector(FulltextToWordArray(query));
        }

        public static string[] FulltextToWordArray(string fulltext)
        {
            return fulltext
                .Trim(new char[] { '.' })
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public float[] BowToVector(string[] query, bool applyPCA = true)
        {
            // check cache
            string cacheKey = string.Join("~", query);
            if (_queryVectorCache.TryGetValue(cacheKey, out float[] cachedVector))
            {
                return cachedVector;
            }

            float[] vector = new float[VECTOR_DIMENSION];

            // convert known words to vectors and add them together
            foreach (string word in query)
            {
                if (_wordToIdDictionary.ContainsKey(word.ToLower()))
                {
                    int wordId = _wordToIdDictionary[word.ToLower()];
                    AddVector(vector, _wordIdToVector[wordId]);
                }
                else
                {
                    // TODO - consider a list of synonyms to map words to the supported dictionary?
                }
            }

            // apply bias
            AddVector(vector, _biasVector);

            // apply tanh element-wise
            TanhElements(vector);

            // apply PCA if required
            if (applyPCA)
            {
                vector = _pcaConversion.ApplyPCA(vector);
            }

            _queryVectorCache.TryAdd(cacheKey, vector);
            return vector;
        }

        private void AddVector(float[] vector, float[] addition)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] += addition[i];
            }
        }

        private void TanhElements(float[] v)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = (float)Math.Tanh(v[i]);
            }
        }

        private Dictionary<string, int> LoadDictionary(string inputFile)
        {
            Dictionary<string, int> wordToIdDictionary = new Dictionary<string, int>();
            foreach (string line in File.ReadAllLines(inputFile))
            {
                if (line.IndexOf(":") == -1) continue;

                string[] tokens = line.Split(':');
                string word = tokens[0].Trim().ToLower();
                int id = int.Parse(tokens[1].Trim());
                wordToIdDictionary.Add(word, id);
            }
            return wordToIdDictionary;
        }


        private float[][] LoadFloatTable(string inputFile, int tableWidth)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(inputFile)))
            {
                int tableHeight = (int)(reader.BaseStream.Length / sizeof(float) / tableWidth);
                float[][] table = new float[tableHeight][];

                for (int iRow = 0; iRow < tableHeight; iRow++)
                {
                    byte[] byteArray = reader.ReadBytes(tableWidth * sizeof(float));
                    float[] floatArray = new float[tableWidth];
                    Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
                    table[iRow] = floatArray;
                }

                return table;
            }
        }
    }
}
