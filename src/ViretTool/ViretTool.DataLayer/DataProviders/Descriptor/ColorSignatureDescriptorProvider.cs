//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

//namespace ViretTool.DataLayer.DataProviders.Descriptor
//{
//    public class ColorSignatureDescriptorProvider
//    {
//        public const string FILE_EXTENSION = ".color";

//        public int CanvasWidth { get; private set; }
//        public int CanvasHeight { get; private set; }

//        public int DescriptorCount { get; private set; }
//        public int DescriptorLength { get; private set; }
//        public byte[][] Descriptors { get; private set; }


//        public static ColorSignatureDescriptorProvider FromDirectory(string inputDirectory)
//        {
//            string[] files = Directory.GetFiles(inputDirectory);
//            string inputFile = files.Where(path => path.EndsWith(FILE_EXTENSION)).Single();
//            return FromFile(inputFile);
//        }

//        public static ColorSignatureDescriptorProvider FromFile(string inputFile)
//        {
//            ColorSignatureDescriptorProvider provider = new ColorSignatureDescriptorProvider();

//            FixedSizeBlobReader blobReader = new FixedSizeBlobReader(inputFile);
//            provider.DescriptorCount = blobReader.BlobCount;
//            provider.DescriptorLength = blobReader.BlobLength;

//            using (BinaryReader reader = new BinaryReader(new MemoryStream(blobReader.FiletypeMetadata)))
//            {
//                throw new NotImplementedException();
//            }


//            return provider;
//        }

//    }
//}
