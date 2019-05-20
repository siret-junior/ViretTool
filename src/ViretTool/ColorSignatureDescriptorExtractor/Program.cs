using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO;

namespace ColorSignatureDescriptorExtractor
{
    /// <summary>
    /// 
    /// V3C1_2019 arguments: 
    /// 26 15 "V3C1.dataset" "V3C1\KeyFrames\images" "V3C1\KeyFrames\filelist.txt" "ExtractedData\V3C1\V3C1-26x15.color"
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // parse extraction parameters
            int signatureWidth = int.Parse(args[0]);
            int signatureHeight = int.Parse(args[1]);

            string datasetHeaderContainingFile = args[2];
            string rootDirectory = args[3];
            string inputFilelist = args[4];

            string outputFile = args[5];
            

            // load dataset header
            byte[] datasetHeader;
            using (BinaryReader headerReader = new BinaryReader(
                File.Open(datasetHeaderContainingFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int headerLength = headerReader.ReadInt32();
                datasetHeader = headerReader.ReadBytes(headerLength);
            }

            ColorSignatureExtractor extractor 
                = new ColorSignatureExtractor(datasetHeader, signatureWidth, signatureHeight);

            string[] inputFiles = File.ReadAllLines(inputFilelist)
                .Select(file => Path.Combine(rootDirectory, file))
                .ToArray();

            // extract signatures
            Console.WriteLine($"Extracting color signatures from {inputFiles.Length} image files:");
            DateTime startTime = DateTime.Now;
            extractor.RunExtraction(inputFiles,
                (percentDone) => 
                {
                    TimeSpan extractionTime = DateTime.Now.Subtract(startTime);
                    TimeSpan remainingTime = TimeSpan.FromTicks(
                        (long)((double)extractionTime.Ticks / percentDone * (100 - percentDone)));
                    Console.WriteLine($"> {percentDone.ToString("00")}% extracted " +
                        $"({(int)(percentDone / 100.0 * inputFiles.Length)}/{inputFiles.Length}). " +
                        $"{extractionTime.ToString(@"d\d\ hh\h\ mm\m\ ss\s")} elapsed, " +
                        $"{remainingTime.ToString(@"d\d\ hh\h\ mm\m\ ss\s")} remaining.");
                });

            // store to the outut binary file
            Console.WriteLine("Storing extracted signatures.");
            using (ColorSignatureWriter writer = new ColorSignatureWriter(outputFile, datasetHeader,
                signatureWidth, signatureHeight, extractor.DescriptorCount))
            {
                for (int i = 0; i < extractor.DescriptorCount; i++)
                {
                    writer.WriteDescriptor(extractor.Descriptors[i]);
                }
            }

            Console.WriteLine("Done");
        }
    }
}
