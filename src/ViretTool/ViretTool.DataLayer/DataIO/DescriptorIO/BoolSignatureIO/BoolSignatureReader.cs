﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO
{
    public class BoolSignatureReader : BoolSignatureIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        public byte[] DatasetHeader => BaseBlobReader.DatasetHeader;

        public int DescriptorCount => BaseBlobReader.BlobCount;
        public int DescriptorLength => BaseBlobReader.BlobLength;

        public int SignatureWidth { get; }
        public int SignatureHeight { get; }


        public BoolSignatureReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);

            byte[] metadata = BaseBlobReader.FiletypeMetadata;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            {
                ReadAndVerifyFiletypeAndVersion(reader);

                SignatureWidth = reader.ReadInt32();
                SignatureHeight = reader.ReadInt32();

                //int descriptorCount = reader.ReadInt32();
                //if (descriptorCount != DescriptorCount)
                //{
                //    throw new IOException(
                //        $"Descriptor count mismatch between ColorSignatureReader ({descriptorCount})" +
                //        $" and underlying BlobReader ({DescriptorCount})");
                //}

                //int descriptorLength = reader.ReadInt32();
                //if (descriptorLength != DescriptorLength)
                //{
                //    throw new IOException(
                //        $"Descriptor length mismatch between ColorSignatureReader ({descriptorLength})" +
                //        $" and underlying BlobReader ({DescriptorLength})");
                //}
            }
        }
        
        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }

        public byte[] ReadDescriptor(int id)
        {
            return BaseBlobReader.ReadByteBlob(id);
        }


        private void ReadAndVerifyFiletypeAndVersion(BinaryReader reader)
        {
            string filetype = reader.ReadString();
            if (!filetype.Equals(BOOL_SIGNATURES_FILETYPE_ID))
            {
                throw new IOException($"Filetype error: {filetype} (expected {BOOL_SIGNATURES_FILETYPE_ID})");
            }

            int version = reader.ReadInt32();
            if (version != BOOL_SIGNATURES_VERSION)
            {
                throw new IOException($"Incorrect \"{BOOL_SIGNATURES_FILETYPE_ID}\" filetype version: "
                    + $"{version} (expected {BOOL_SIGNATURES_VERSION})");
            }
        }
    }
}