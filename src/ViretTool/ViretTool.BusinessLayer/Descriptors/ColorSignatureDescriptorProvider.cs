using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class ColorSignatureDescriptorProvider : IDescriptorProvider<byte[]>
    {
        public byte[] DatasetHeader { get; }

        public int DescriptorCount { get; }
        public int DescriptorLength { get; }

        public byte[][] Descriptors { get; }

        public int SignatureWidth { get; }
        public int SignatureHeight { get; }

        
        public ColorSignatureDescriptorProvider(string inputFile)
        {
            using (ColorSignatureReader reader = new ColorSignatureReader(inputFile))
            {
                DatasetHeader = reader.DatasetHeader;

                SignatureWidth = reader.SignatureWidth;
                SignatureHeight = reader.SignatureHeight;

                DescriptorCount = reader.DescriptorCount;
                DescriptorLength = reader.DescriptorLength;

                Descriptors = new byte[DescriptorCount][];
                for (int i = 0; i < DescriptorCount; i++)
                {
                    Descriptors[i] = reader.ReadDescriptor(i);
                }
            }    
        }

        public static ColorSignatureDescriptorProvider FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(ColorSignatureIOBase.COLOR_SIGNATURES_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new ColorSignatureDescriptorProvider(inputFile);
            }
            else
            {
                return null;
            }
        }


        public byte[] GetDescriptor(int index)
        {
            return Descriptors[index];
        }

        public byte[] this[int index]
        {
            get => Descriptors[index];
        }

        public static float GetDistance(byte[] x, byte[] y)
        {
            return (float)L2Distance(x, y);
        }


        private static double L2Distance(byte[] x, byte[] y)
        {
            double result = 0, r;
            for (int i = 0; i < x.Length; i++)
            {
                r = x[i] - y[i];
                result += r * r;
            }
            return Math.Sqrt(result);
        }
    }
}
