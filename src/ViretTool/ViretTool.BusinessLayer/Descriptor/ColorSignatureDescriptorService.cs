using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO;

namespace ViretTool.BusinessLayer.Descriptor
{
    public class ColorSignatureDescriptorService : IDescriptorService<byte[]>
    {
        public byte[] DatasetHeader { get; }
        
        public int SignatureWidth { get; }
        public int SignatureHeight { get; }

        public int DescriptorCount { get; }
        public int DescriptorLength { get; }

        public byte[][] Descriptors { get; }


        public ColorSignatureDescriptorService(ColorSignatureReader reader)
        {
            DatasetHeader = reader.DatasetHeader;

            SignatureWidth = reader.SignatureWidth;
            SignatureHeight = reader.SignatureHeight;

            DescriptorCount = reader.DescriptorCount;
            DescriptorLength = reader.DescriptorLength;

            Descriptors = reader.Descriptors;
        }
        

        public byte[] GetDescriptor(int index)
        {
            return Descriptors[index];
        }

        public byte[] this[int index]
        {
            get => Descriptors[index];
        }

    }
}
