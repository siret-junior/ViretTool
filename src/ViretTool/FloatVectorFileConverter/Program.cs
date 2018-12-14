﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO;

namespace FloatVectorFileConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string datasetHeaderContainingFile = args[0];
            string inputFile = args[1];
            string outputFile = args[2];
            string source = args[3];

            
            // load dataset header
            byte[] datasetHeader;
            using (BinaryReader headerReader = new BinaryReader(
                File.Open(datasetHeaderContainingFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int headerLength = headerReader.ReadInt32();
                datasetHeader = headerReader.ReadBytes(headerLength);
            }

            // load input data
            List<(int id, float[] vector)> descriptors = new List<(int id, float[] vector)> ();
            int vectorLength;
            using (BinaryReader reader = new BinaryReader(
                File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                reader.ReadBytes(36);   // header
                vectorLength = reader.ReadInt32();

                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    int id = reader.ReadInt32();
                    float[] vector = new float[vectorLength];
                    for (int i = 0; i < vectorLength; i++)
                    {
                        vector[i] = reader.ReadSingle();
                    }
                    descriptors.Add((id, vector));
                }
            }


            // write output data
            float[][] sortedDescriptors = descriptors.OrderBy(x => x.id).Select(x => x.vector).ToArray();
            using (FloatVectorWriter writer = new FloatVectorWriter(
                outputFile, datasetHeader, vectorLength, sortedDescriptors.Length, source))
            {
                for (int i = 0; i < sortedDescriptors.Length; i++)
                {
                    Normalize(sortedDescriptors[i]);
                    float[] normalizedVector = sortedDescriptors[i];
                    writer.WriteDescriptor(normalizedVector);
                }
            }
        }

        static void Normalize(float[] vector)
        {
            // compute length
            double length = GetVectorLength(vector);
            double normalizer = 1.0 / length;

            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)(vector[i] * normalizer);
            }
        }


        static double GetVectorLength(float[] vector)
        {
            double length = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                length += vector[i] * vector[i];
            }

            return Math.Sqrt(length);
        }
    }
}
