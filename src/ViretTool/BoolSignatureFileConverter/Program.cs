using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO;
using ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO;

namespace BoolSignatureFileConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string datasetHeaderContainingFile = args[0];
            string inputFile = args[1];
            string outputFile = args[2];


            // load dataset header
            byte[] datasetHeader;
            using (BinaryReader headerReader = new BinaryReader(
                File.Open(datasetHeaderContainingFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int headerLength = headerReader.ReadInt32();
                datasetHeader = headerReader.ReadBytes(headerLength);
            }

            // load input data
            using (BinaryReader reader = new BinaryReader(
                File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int signatureCount = reader.ReadInt32();
                int signatureWidth = reader.ReadInt32();
                int signatureHeight = reader.ReadInt32();
                int signatureLength = signatureWidth * signatureHeight;
                
                // write output data
                using (BoolSignatureWriter writer = new BoolSignatureWriter(
                    outputFile, datasetHeader, signatureWidth, signatureHeight, signatureCount))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        byte[] signature = reader.ReadBytes(signatureLength);
                        writer.WriteDescriptor(signature);
                    }
                }
            }
        }
    }
}
