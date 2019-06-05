using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class SemanticVectorDescriptorProvider : IDescriptorProvider<float[]>
    {
        public byte[] DatasetHeader { get; }

        public int DescriptorCount { get; }
        public int DescriptorLength { get; }

        public float[][] Descriptors { get; }


        public SemanticVectorDescriptorProvider(string inputFile)
        {
            using (FloatVectorReader reader = new FloatVectorReader(inputFile))
            {
                DatasetHeader = reader.DatasetHeader;
                
                DescriptorCount = reader.DescriptorCount;
                DescriptorLength = reader.DescriptorLength;

                Descriptors = new float[DescriptorCount][];
                for (int i = 0; i < DescriptorCount; i++)
                {
                    Descriptors[i] = reader.ReadDescriptor(i);
                }
            }
        }


        public static SemanticVectorDescriptorProvider FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(FloatVectorIOBase.FLOAT_VECTOR_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new SemanticVectorDescriptorProvider(inputFile);
            }
            else
            {
                return null;
            }
        }


        public float[] GetDescriptor(int index)
        {
            return Descriptors[index];
        }

        public float[] this[int index]
        {
            get => Descriptors[index];
        }


        public double GetDistance(int id1, int id2)
        {
            return CosineDistance(Descriptors[id1], Descriptors[id2]);
        }


        private static double CosineDistance(float[] x, float[] y)
        {
            return 1 - CosineSimilarity(x, y);
        }

        private static double CosineSimilarity(float[] x, float[] y)
        {
            return CosineSimilaritySISD(x, y);
        }

        private static double CosineSimilaritySISD(float[] x, float[] y)
        {
            double result = 0.0;

            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }

            return result;
        }
    }
}
