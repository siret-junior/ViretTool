using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.W2VV
{
    public class W2vvBowToVector
    {
        private const int VECTOR_DIMENSION = 2048;

        private readonly Dictionary<string, int> _wordToIdDictionary;
        private readonly float[][] _wordIdToVector;
        private readonly float[] _biasVector;
        private readonly float[][] _pcaMatrix;
        private readonly float[] _pcaMeanVector;

        public W2vvBowToVector(string inputDirectory)
        {
            _wordToIdDictionary = LoadDictionary(Path.Combine(inputDirectory, "word2idx.txt"));

            _wordIdToVector = LoadFloatTable(Path.Combine(inputDirectory, "txt_weight-11147x2048floats.bin"), VECTOR_DIMENSION);
            _biasVector = LoadFloatTable(Path.Combine(inputDirectory, "txt_bias-2048floats.bin"), VECTOR_DIMENSION)[0];
            _pcaMatrix = LoadFloatTable(Path.Combine(inputDirectory, Directory.GetFiles(inputDirectory, "*w2vv.pca.matrix.bin").First()), VECTOR_DIMENSION);
            _pcaMeanVector = LoadFloatTable(Path.Combine(inputDirectory, Directory.GetFiles(inputDirectory, "*w2vv.pca.mean.bin").First()), VECTOR_DIMENSION)[0];
        }

        public static W2vvBowToVector FromDirectory(string path)
        {
            return new W2vvBowToVector(Path.Combine(path, "w2vv"));
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
                NormalizeVector(vector);
                SubtractVector(vector, _pcaMeanVector);
                float[] result = new float[_pcaMatrix.Length];
                for (int iRow = 0; iRow < _pcaMatrix.Length; iRow++)
                {
                    float[] row = _pcaMatrix[iRow];
                    result[iRow] = GetDotProduct(row, vector);
                }
                vector = result;
                NormalizeVector(vector);
            }

            // 512-dim vector with PCA or 2048-dim vector without PCA
            return vector;
        }

        private void AddVector(float[] vector, float[] addition)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] += addition[i];
            }
        }

        private void SubtractVector(float[] vector, float[] subtraction)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] -= subtraction[i];
            }
        }

        private float GetDotProduct(float[] v1, float[] v2)
        {
            double result = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                result += v1[i] * v2[i];
            }
            return (float)result;
        }

        private void TanhElements(float[] v)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = (float)Math.Tanh(v[i]);
            }
        }

        private void NormalizeVector(float[] v)
        {
            float size = (float)Math.Sqrt(GetDotProduct(v, v));
            if (size == 0) return;

            for (int i = 0; i < v.Length; i++)
            {
                v[i] /= size;
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
