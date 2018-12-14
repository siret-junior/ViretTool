using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO;

namespace ViretTool.DataLayer.Test
{
    [TestClass]
    public class ColorExtractionTest
    {
        [TestMethod]
        public void Extract()
        {
            //string inputHeaderFile = @"c:\Programming\ViretTool2018\ExtractedData\V3C1_First750\V3C1_first750.dataset";
            //string outputFile = @"c:\Programming\ViretTool2018\ExtractedData\V3C1_First750\V3C1_first750.color";
            //string inputFilelist = @"c:\Datasets\V3C1-first750\KeyFrames\filelist.txt";

            //byte[] header;
            //using (BinaryReader reader = new BinaryReader(File.OpenRead(inputHeaderFile)))
            //{
            //    int count = reader.ReadInt32();
            //    header = reader.ReadBytes(count);
            //}

            //ColorSignatureExtractor ex = new ColorSignatureExtractor(header, 28, 16);
            //string inputFiles
            //ex.RunExtraction(, null);
            //ColorSignatureWriter writer = new ColorSignatureWriter(outputFile, header, ex.SignatureWidth, ex.SignatureHeight, ex.DescriptorCount);

            //foreach (byte[] desc in ex.Descriptors)
            //{
            //    writer.WriteDescriptor(desc);
            //}

            //writer.Dispose();
        }
    }
}
