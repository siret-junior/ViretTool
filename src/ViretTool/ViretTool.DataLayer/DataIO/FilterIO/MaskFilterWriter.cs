using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.FilterIO
{
    public class MaskFilterWriter : MaskFilterIOBase
    {
        public FixedSizeBlobWriter BaseBlobWriter { get; private set; }
        public byte[] DatasetHeader => BaseBlobWriter.DatasetHeader;

        private int _blobsWrittenCount = 0;

        public MaskFilterWriter(string outputFile, byte[] datasetHeader, int frameCount)
        {
            byte[] maskFilterMetadata = null;
            using (MemoryStream metadataStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(metadataStream))
            {
                writer.Write(MASK_FILTER_FILETYPE_ID);
                writer.Write(MASK_FILTER_VERSION);
                
                // reserve space
                writer.Write(new byte[METADATA_RESERVE_SPACE_SIZE]);

                maskFilterMetadata = metadataStream.ToArray();
            }

            BaseBlobWriter = new FixedSizeBlobWriter(
                outputFile, datasetHeader, 1, sizeof(float) * frameCount, maskFilterMetadata);
        }
        
        public void WriteFilter(float[] filterValues)
        {
            if (_blobsWrittenCount > 0)
            {
                throw new IOException("Attempted to write more than one mask filter blob.");
            }
            byte[] blob = DataConversionUtilities.TranslateToByteArray(filterValues);
            BaseBlobWriter.WriteBlob(blob);
        }

        public override void Dispose()
        {
            BaseBlobWriter.Dispose();
        }
    }
}
