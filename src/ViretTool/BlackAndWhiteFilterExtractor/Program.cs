using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.FilterIO;

namespace BlackAndWhiteFilterExtractor
{
    class Program
    {
        // generally using threshold 32
        static void Main(string[] args)
        {
            int threshold = int.Parse(args[0]);
            string datasetHeaderContainingFile = args[1];
            string rootDirectory = args[2];
            string inputFilelist = args[3];
            string outputFilePrefix = args[4];

            
            // load dataset header
            byte[] datasetHeader;
            using (BinaryReader headerReader = new BinaryReader(
                File.Open(datasetHeaderContainingFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int headerLength = headerReader.ReadInt32();
                datasetHeader = headerReader.ReadBytes(headerLength);
            }

            // load input files
            string[] inputFiles = File.ReadAllLines(inputFilelist)
                .Select(file => Path.Combine(rootDirectory, file))
                .ToArray();
            int itemCount = inputFiles.Length;

            // prepare arrays with statistics for each frame
            float[] bwDeltaValues = new float[itemCount];
            float[] pbValues = new float[itemCount];

            // run extraction
            float maxRGBDelta = 0;
            for (int i = 0; i < itemCount; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Extracting filters {i + 1}/{itemCount}.");
                }
                // TODO: parallel
                Bitmap bitmap = new Bitmap(inputFiles[i]);

                // TODO: refactoring tuples
                Tuple<float, float> statistics = ComputeColorStatistics(bitmap, threshold);
                if (statistics.Item1 > maxRGBDelta)
                {
                    maxRGBDelta = statistics.Item1;
                }

                bwDeltaValues[i] = statistics.Item1;
                pbValues[i] = statistics.Item2;
            }

            // Normalization to 0 - 1
            // Inversion to stay consistent with idea "bigger number -> more black"
            for (int i = 0; i < itemCount; i++)
            {
                bwDeltaValues[i] = 1 - bwDeltaValues[i] / maxRGBDelta;
            }

            Console.WriteLine("Filters extracted, saving them.");



            // store arrays
            // TODO - use constants for filenames
            //StoreFilterValues(dataset.GetFileNameByExtension(".bwfilter"), bwDeltaValues, dataset);
            //StoreFilterValues(dataset.GetFileNameByExtension(".pbcfilter"), pbValues, dataset);

            using (MaskFilterWriter bwWriter = 
                new MaskFilterWriter(outputFilePrefix + ".bwfilter", datasetHeader, bwDeltaValues.Length))
            {
                bwWriter.WriteFilter(bwDeltaValues);
            }

            using (MaskFilterWriter pbcfilter =
                new MaskFilterWriter(outputFilePrefix + ".pbcfilter", datasetHeader, pbValues.Length))
            {
                pbcfilter.WriteFilter(pbValues);
            }
        }



        //private static void StoreFilterValues(string filename, float[] filterValues, Dataset dataset)
        //{
        //    using (var FS = new FileStream(filename, FileMode.Create))
        //    using (var BW = new BinaryWriter(FS))
        //    {
        //        BW.Write(dataset.DatasetId.Length);
        //        BW.Write(dataset.DatasetId);
        //        BW.Write((Int32)filterValues.Length);

        //        // TODO - optimize
        //        foreach (float f in filterValues)
        //            BW.Write(f);
        //    }
        //}

        public static unsafe Tuple<float, float> ComputeColorStatistics(Bitmap bitmap, int blackThreshold)
        {
            float maxRGBDelta = 0;
            float percentageOfBlack = 0;
            int blackThresholdSquared = blackThreshold * blackThreshold;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            System.Diagnostics.Debug.Assert(data.PixelFormat == PixelFormat.Format24bppRgb);

            byte* ptr = (byte*)data.Scan0.ToPointer();
            int bitmapSize = data.Height * data.Width * 3;

            int delta;
            byte R, G, B;
            for (int i = 0; i < bitmapSize; i += 3, ptr += 3)
            {
                R = *ptr; G = *(ptr + 1); B = *(ptr + 2);

                // count pixels with a limited L2 distance from black color
                if (R * R + G * G + B * B < blackThresholdSquared)
                    percentageOfBlack++;

                // find the maximal RGB delta
                delta = Math.Abs(R - G) + Math.Abs(G - B) + Math.Abs(B - R);
                if (delta > maxRGBDelta)
                    maxRGBDelta = (float)delta;
            }

            bitmap.UnlockBits(data);

            return new Tuple<float, float>(maxRGBDelta, percentageOfBlack / (data.Height * data.Width));
        }

    }
}
