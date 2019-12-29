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
        private Dictionary<string, int> mDictionary;
        private List<float[]> M11147x2048;
        private List<float[]> Bias;
        private List<float[]> PCA;
        private List<float[]> PCAMean;

        private int mDimension = 2048;

        public W2VVQueryToVectorProvider(string path)
        {
            mDictionary = LoadDictionary(Path.Combine(path, "word2idx.txt"));
            M11147x2048 = LoadData(Path.Combine(path, "txt_weight-11147x2048floats.bin"), mDimension);
            Bias = LoadData(Path.Combine(path, "txt_bias-2048floats.bin"), mDimension);
            PCA = LoadData(Path.Combine(path, "V3C1_20191228.w2vv.pca.matrix.bin"), mDimension);
            PCAMean = LoadData(Path.Combine(path, "V3C1_20191228.w2vv.pca.mean.bin"), mDimension);
        }

        public static W2VVQueryToVectorProvider FromDirectory(string path)
        {
            return new W2VVQueryToVectorProvider(Path.Combine(path, "w2vv"));
        }

        public float[] TextToVector(string[] query, bool applyPCA = true)
        {
            float[] v = new float[mDimension];
            foreach (string q in query)
                if (mDictionary.ContainsKey(q.ToLower()))
                    AddVectors(v, M11147x2048[mDictionary[q.ToLower()]]);
                else
                { } // TODO - consider a list of synonyms to map words to the supported dictionary?

            AddVectors(v, Bias[0]);
            TANH(v);

            if (!applyPCA) return v;

            // PCA
            v = Normalize(v);
            SubtractVectors(v, PCAMean[0]);
            List<float> result = new List<float>();
            foreach (float[] row in PCA)
                result.Add(DotProduct(row, v));

            v = result.ToArray();

            return Normalize(v);
        }

        private void AddVectors(float[] v1, float[] v2)
        {
            for (int i = 0; i < v1.Length; i++)
                v1[i] += v2[i];
        }

        private void SubtractVectors(float[] v1, float[] v2)
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

        private void TANH(float[] v)
        {
            for (int i = 0; i < v.Length; i++)
                v[i] = (float)Math.Tanh(v[i]);
        }

        private float[] Normalize(float[] v)
        {
            float size = (float)Math.Sqrt(DotProduct(v, v));
            if (size == 0) return v;

            for (int i = 0; i < v.Length; i++)
                v[i] /= size;

            return v;
        }


        private List<float[]> LoadData(string fileName, int dimension)
        {
            List<float[]> data = new List<float[]>();

            byte[] fileData = null;
            using (FileStream FS = new FileStream(fileName, FileMode.Open))
            {
                fileData = new byte[FS.Length];
                FS.Read(fileData, 0, (int)FS.Length);      // read all vectors
            }

            int recordLength = dimension * 4;
            int count = fileData.Length / recordLength;

            for (int i = 0; i < count; i++)
            {
                // read vector
                float[] vector = new float[dimension];
                Buffer.BlockCopy(fileData, i * recordLength, vector, 0, recordLength);
                data.Add(vector);
            }

            return data;
        }

        private Dictionary<string, int> LoadDictionary(string filename)
        {
            List<string> rows = new List<string>();
            using (StreamReader SR = new StreamReader(filename))
                while (!SR.EndOfStream)
                    rows.Add(SR.ReadLine());

            Dictionary<string, int> d = new Dictionary<string, int>();

            foreach (string line in rows)
            {
                if (line.IndexOf(":") == -1) continue;
                string[] values = line.Split(':');
                string v = values[0].Trim().ToLower();
                d.Add(v, int.Parse(values[1].Trim()));
            }
            return d;
        }

    }
}
