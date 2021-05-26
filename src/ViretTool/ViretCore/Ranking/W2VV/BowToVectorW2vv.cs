using System;
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

        public static BowToVectorW2vv FromDirectory(string path, string subDirectory = "w2vv")
        {
            return new BowToVectorW2vv(Path.Combine(path, subDirectory));
        }

        public float[] BowToVector(string[] query, bool applyPCA = true)
        {
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
