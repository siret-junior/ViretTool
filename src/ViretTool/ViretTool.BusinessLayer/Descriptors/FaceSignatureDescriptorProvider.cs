//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO;

//namespace ViretTool.BusinessLayer.Descriptors
//{
//    public class FaceSignatureDescriptorProvider : IFaceSignatureDescriptorProvider
//    {
//        public const string FILE_EXTENSION = ".face";

//        public byte[] DatasetHeader { get; }

//        public int DescriptorCount { get; }
//        public int DescriptorLength { get; }

//        public bool[][] Descriptors { get; }
//        public int SignatureWidth { get; }
//        public int SignatureHeight { get; }


//        public FaceSignatureDescriptorProvider(string inputFile)
//        {
//            using (BoolSignatureReader reader = new BoolSignatureReader(inputFile))
//            {
//                DatasetHeader = reader.DatasetHeader;

//                SignatureWidth = reader.SignatureWidth;
//                SignatureHeight = reader.SignatureHeight;

//                DescriptorCount = reader.DescriptorCount;
//                DescriptorLength = reader.DescriptorLength;

//                Descriptors = new bool[DescriptorCount][];
//                for (int i = 0; i < DescriptorCount; i++)
//                {
//                    Descriptors[i] = reader.ReadDescriptor(i);
//                }
//            }
//        }


//        public static FaceSignatureDescriptorProvider FromDirectory(string directory, string extension)
//        {
//            string inputFile = Directory.GetFiles(directory)
//                    .Where(dir => Path.GetFileName(dir).EndsWith(extension))
//                    .FirstOrDefault();

//            if (inputFile != null)
//            {
//                return new FaceSignatureDescriptorProvider(inputFile);
//            }
//            else
//            {
//                throw new IOException($"File not found: {inputFile}");
//            }
//        }
//    }
//}
