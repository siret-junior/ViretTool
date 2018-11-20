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
        public byte[] DatasetHeader { get; }

        public int DescriptorCount { get; }
        public int DescriptorLength { get; }

        public int SignatureWidth { get; }
        public int SignatureHeight { get; }

        public byte[][] Descriptors { get; }


        public ColorSignatureReader(string filePath)
        {
            using (FixedSizeBlobReader blobReader = new FixedSizeBlobReader(filePath))
            {
                DatasetHeader = blobReader.DatasetHeader;
                DescriptorCount = blobReader.BlobCount;
                DescriptorLength = blobReader.BlobLength;

                // load metadata
                byte[] metadata = blobReader.FiletypeMetadata;
                using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
                {
                    SignatureWidth = reader.ReadInt32();
                    SignatureHeight = reader.ReadInt32();
                }

                // load descriptors
                Descriptors = new byte[DescriptorCount][];
                for (int iSignature = 0; iSignature < DescriptorCount; iSignature++)
                {
                    Descriptors[iSignature] = blobReader.ReadByteBlob(iSignature);
                }
            }
        }
    }
}
