using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO
{
    public class ColorSignatureReader : ColorSignatureIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        public byte[] DatasetHeader => BaseBlobReader.DatasetHeader;

        public int DescriptorCount => BaseBlobReader.BlobCount;
        public int DescriptorLength => BaseBlobReader.BlobLength;

        public int SignatureWidth { get; }
        public int SignatureHeight { get; }

        //public byte[][] Descriptors { get; }
        

        public ColorSignatureReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);

            //byte[] metadata = BaseBlobReader.FiletypeMetadata;
            //using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            BinaryReader reader = BaseBlobReader.BaseBinaryReader;
            {
                //ReadAndVerifyFiletypeAndVersion(reader);

                SignatureWidth = reader.ReadInt32();
                SignatureHeight = reader.ReadInt32();
            }

            BaseBlobReader.MarkDataStartOffset();
        }

        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }

        public byte[] ReadDescriptor(int id)
        {
            return BaseBlobReader.ReadByteBlob(id);
        }


        //private void ReadAndVerifyFiletypeAndVersion(BinaryReader reader)
        //{
        //    string filetype = reader.ReadString();
        //    if (!filetype.Equals(COLOR_SIGNATURES_FILETYPE_ID))
        //    {
        //        throw new IOException($"Filetype error: {filetype} (expected {COLOR_SIGNATURES_FILETYPE_ID})");
        //    }

        //    int version = reader.ReadInt32();
        //    if (version != COLOR_SIGNATURES_VERSION)
        //    {
        //        throw new IOException($"Incorrect \"{COLOR_SIGNATURES_FILETYPE_ID}\" filetype version: "
        //            + $"{version} (expected {COLOR_SIGNATURES_VERSION})");
        //    }
        //}


    }
}
