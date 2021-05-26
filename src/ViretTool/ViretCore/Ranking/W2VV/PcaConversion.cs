using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.W2VV
{
    public class PcaConversion
    {
        private readonly float[][] _pcaMatrix;
        private readonly float[] _pcaMeanVector;

        public PcaConversion(string inputDirectory, int vectorDimension, 
            string matrixFileExtension = ".pca.matrix.bin", 
            string meanFileExtension = ".pca.mean.bin")
        {
            _pcaMatrix = LoadFloatTable(Path.Combine(inputDirectory, Directory.GetFiles(inputDirectory, $"*{matrixFileExtension}").First()), vectorDimension);
            _pcaMeanVector = LoadFloatTable(Path.Combine(inputDirectory, Directory.GetFiles(inputDirectory, $"*{meanFileExtension}").First()), vectorDimension)[0];
        }

        public float[] ApplyPCA(float[] vector)
        {
            NormalizeVector(vector);
            SubtractVector(vector, _pcaMeanVector);
            float[] result = new float[_pcaMatrix.Length];
            for (int iRow = 0; iRow < _pcaMatrix.Length; iRow++)
            {
                float[] row = _pcaMatrix[iRow];
                result[iRow] = GetDotProduct(row, vector);
            }
            NormalizeVector(result);
            return result;
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

        private void NormalizeVector(float[] v)
        {
            float size = (float)Math.Sqrt(GetDotProduct(v, v));
            if (size == 0) return;

            for (int i = 0; i < v.Length; i++)
            {
                v[i] /= size;
            }
        }
        private void SubtractVector(float[] vector, float[] subtraction)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] -= subtraction[i];
            }
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
