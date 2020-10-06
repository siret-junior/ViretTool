using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class W2VVQueryToVectorProvider : IW2VVQueryToVectorProvider
    {
        private const int VECTOR_DIMENSION = 2048;
        
        private readonly Dictionary<string, int> _wordToIdDictionary;
        private readonly float[][] _wordIdToVector;
        private readonly float[] _biasVector;
        private readonly float[][] _pcaMatrix;
        private readonly float[] _pcaMeanVector;

        public W2VVQueryToVectorProvider(string path)
        {
            _wordToIdDictionary = LoadDictionary(Path.Combine(path, "word2idx.txt"));

            _wordIdToVector = LoadFloatTable(Path.Combine(path, "txt_weight-11147x2048floats.bin"), VECTOR_DIMENSION);
            _biasVector = LoadFloatTable(Path.Combine(path, "txt_bias-2048floats.bin"), VECTOR_DIMENSION)[0];
            _pcaMatrix = LoadFloatTable(Path.Combine(path, Directory.GetFiles(path, "*w2vv.pca.matrix.bin").First()), VECTOR_DIMENSION);
            _pcaMeanVector = LoadFloatTable(Path.Combine(path, Directory.GetFiles(path, "*w2vv.pca.mean.bin").First()), VECTOR_DIMENSION)[0];
        }

        public static W2VVQueryToVectorProvider FromDirectory(string path)
        {
            return new W2VVQueryToVectorProvider(Path.Combine(path, "w2vv"));
        }

        public float[] TextToVector(string[] query, bool applyPCA = true)
        {
            float[] vector = new float[VECTOR_DIMENSION];

            // convert words to vectors and add them together
            foreach (string word in query)
            {
                if (_wordToIdDictionary.ContainsKey(word.ToLower()))
                {
                    int wordId = _wordToIdDictionary[word.ToLower()];
                    AddVector(vector, _wordIdToVector[wordId]);
                }
                else
                { } // TODO - consider a list of synonyms to map words to the supported dictionary?
            }

            // apply bias
            AddVector(vector, _biasVector);
            
            // apply tanh element-wise
            TanhElements(vector);

            // apply PCA if required
            if (!applyPCA)
            {
                return vector;
            }
            else
            {
                // PCA
                vector = NormalizeVector(vector);
                SubtractVector(vector, _pcaMeanVector);
                List<float> result = new List<float>();
                foreach (float[] row in _pcaMatrix)
                {
                    result.Add(DotProduct(row, vector));
                }
                vector = result.ToArray();

                return NormalizeVector(vector);
            }
            // TODO: inconsistent result vector dimension!
            // PCA uses 128 dimensions while default is 2048
        }

        private void AddVector(float[] v1, float[] v2)
        {
            for (int i = 0; i < v1.Length; i++)
                v1[i] += v2[i];
        }

        private void SubtractVector(float[] v1, float[] v2)
        {
            for (int i = 0; i < v1.Length; i++)
                v1[i] -= v2[i];
        }

        private float DotProduct(float[] v1, float[] v2)
        {
            double result = 0;
            for (int i = 0; i < v1.Length; i++)
                result += v1[i] * v2[i];
            return (float)result;
        }

        private void TanhElements(float[] v)
        {
            for (int i = 0; i < v.Length; i++)
                v[i] = (float)Math.Tanh(v[i]);
        }

        private float[] NormalizeVector(float[] v)
        {
            float size = (float)Math.Sqrt(DotProduct(v, v));
            if (size == 0) return v;

            for (int i = 0; i < v.Length; i++)
                v[i] /= size;

            return v;
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

        //private float[][] LoadData(string fileName, int dimension)
        //{
        //    List<float[]> table = new List<float[]>();

        //    // read all bytes to byte array
        //    byte[] fileData = null;
        //    using (FileStream FS = new FileStream(fileName, FileMode.Open))
        //    {
        //        fileData = new byte[FS.Length];
        //        FS.Read(fileData, 0, (int)FS.Length);      // read all vectors
        //    }

        //    int recordLength = dimension * 4;
        //    int count = fileData.Length / recordLength;

        //    // convert byte vectors to float vectors
        //    for (int i = 0; i < count; i++)
        //    {
        //        // read vector
        //        float[] vector = new float[dimension];
        //        Buffer.BlockCopy(fileData, i * recordLength, vector, 0, recordLength);
        //        table.Add(vector);
        //    }

        //    return table.ToArray();
        //}

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
