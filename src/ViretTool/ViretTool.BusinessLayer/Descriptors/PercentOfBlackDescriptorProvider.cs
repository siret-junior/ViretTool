using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.FilterIO;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class PercentOfBlackDescriptorProvider : IDescriptorProvider<float>
    {
        public const string PERCENT_OF_BLACK_FILTER_EXTENSION = ".pbcfilter";

        public byte[] DatasetHeader { get; private set; }

        public int DescriptorCount { get; private set; }

        public int DescriptorLength { get; private set; }

        public float[] Descriptors { get; private set; }

        
        public PercentOfBlackDescriptorProvider(string inputFile)
        {
            using (MaskFilterReader reader = new MaskFilterReader(inputFile))
            {
                DescriptorCount = reader.DescriptorCount;
                DescriptorLength = reader.DescriptorLength;

                Descriptors = reader.ReadFilter();
            }
        }

        public static PercentOfBlackDescriptorProvider FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(PERCENT_OF_BLACK_FILTER_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new PercentOfBlackDescriptorProvider(inputFile);
            }
            else
            {
                return null;
            }
        }
        

        public float GetDescriptor(int index) => Descriptors[index];
        
        public float this[int index] => Descriptors[index];

    }
}
