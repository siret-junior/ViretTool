using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.FilterIO;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class ColorSaturationDescriptorProvider : IDescriptorProvider<float>
    {
        public const string COLOR_SATURATION_FILTER_EXTENSION = ".bwfilter";

        public byte[] DatasetHeader { get; private set; }

        public int DescriptorCount { get; private set; }
        public int DescriptorLength { get; private set; }
        public float[] Descriptors { get; private set; }


        public ColorSaturationDescriptorProvider(string inputFile)
        {
            using (ThresholdFilterReader reader = new ThresholdFilterReader(inputFile))
            {
                DescriptorCount = reader.DescriptorCount;
                DescriptorLength = reader.DescriptorLength;

                Descriptors = reader.ReadFilter();
            }
        }

        public static PercentOfBlackDescriptorProvider FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(COLOR_SATURATION_FILTER_EXTENSION))
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
