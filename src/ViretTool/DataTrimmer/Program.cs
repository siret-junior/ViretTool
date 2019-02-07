using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Proxy;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ViretTool.DataLayer.DataIO.DatasetIO;
using ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO;
using ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO;
using ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO;
using ViretTool.DataLayer.DataIO.FilterIO;
using ViretTool.DataLayer.DataIO.ThumbnailIO;
using ViretTool.DataLayer.DataModel;

namespace DataTrimmer
{
    class Program
    {
        private const int PROGRESS_BAR_WIDTH = 20;
        private static string _filenameSuffix = "_first";
        private static byte[] _datasetHeader;

        static void Main(string[] args)
        {
            // TODO: print usage, parameter check
            int maxVideos = int.Parse(args[0]);
            string datasetDirectory = args[1];
            string outputDirectory = args[2];
            Directory.CreateDirectory(outputDirectory);
            _filenameSuffix += maxVideos;

            string[] directoryFiles = Directory.GetFiles(datasetDirectory);
            Dataset dataset = TrimDatasetFile(directoryFiles, maxVideos, outputDirectory);
            int frameCount = dataset.Frames.Count;
            _datasetHeader = dataset.DatasetId;

            TrimKeywordFile(directoryFiles, frameCount, outputDirectory);
            TrimColorFile(directoryFiles, frameCount, outputDirectory);
            TrimFloatVectorFile(directoryFiles, frameCount, outputDirectory);
            TrimMaskFilterFile(directoryFiles, ".bwfilter", frameCount, outputDirectory);
            TrimMaskFilterFile(directoryFiles, ".pbcfilter", frameCount, outputDirectory);
            TrimBoolFile(directoryFiles, ".faces", frameCount, outputDirectory);
            TrimBoolFile(directoryFiles, ".text", frameCount, outputDirectory);
            TrimFrameattributeFile(directoryFiles, frameCount, outputDirectory);
            //TrimThumbnailsFile(directoryFiles, maxVideos, outputDirectory);
        }


        private static Dataset TrimDatasetFile(string[] directoryFiles, int maxVideos, string outputDirectory)
        {
            Console.Write("Trimming dataset file...");

            string inputDatasetPath = directoryFiles
                .Where(path => path.EndsWith(DatasetSerializationBase.DATASET_EXTENSION))
                .Single();
            string outputDatasetPath
                = GenerateOutputPath(inputDatasetPath, outputDirectory, DatasetSerializationBase.DATASET_EXTENSION);
            Dataset dataset = DatasetTrimmer.Trim(inputDatasetPath, outputDatasetPath, maxVideos);

            Console.WriteLine("DONE");
            return dataset;
        }

        private static void TrimKeywordFile(string[] directoryFiles, int frameCount, string outputDirectory)
        {
            Console.Write("Trimming keyword file...");

            const string KEYWORD_EXTENSION = ".keyword";
            string inputPath = directoryFiles.Where(path => path.EndsWith(KEYWORD_EXTENSION)).Single();
            string outputPath = GenerateOutputPath(inputPath, outputDirectory, KEYWORD_EXTENSION);


            using (BinaryReader reader = new BinaryReader(File.OpenRead(inputPath)))
            using (BinaryWriter writer = new BinaryWriter(File.Open(outputPath, FileMode.CreateNew)))
            {
                // header = 'KS INDEX'+(Int64)-1
                writer.Write(reader.ReadInt64());
                writer.Write(reader.ReadInt64());

                // read offests of each class
                Dictionary<int, int> classOffsets = new Dictionary<int, int>();
                while (true)
                {
                    int classId = reader.ReadInt32();
                    int classOffset = reader.ReadInt32();

                    if (classId == -1)
                    {
                        break;
                    }
                    classOffsets.Add(classId, classOffset);
                }

                // read inverted index
                Dictionary<int, List<(int itemId, float probability)>> invertedIndex 
                    = new Dictionary<int, List<(int itemId, float probability)>>();
                foreach (int classId in classOffsets.Keys)
                {
                    // seek to the class offset
                    int classOffset = classOffsets[classId];
                    List<(int itemId, float probability)> probabilities = new List<(int itemId, float probability)>();
                    reader.BaseStream.Seek(classOffset, SeekOrigin.Begin);

                    // read all class items
                    while (true)
                    {
                        int itemId = reader.ReadInt32();
                        float itemProbability = reader.ReadSingle();

                        if (itemId == -1)
                        {
                            break;
                        }

                        // trim items
                        if (itemId < frameCount)
                        {
                            probabilities.Add((itemId, itemProbability));
                        }
                    }
                    invertedIndex.Add(classId, probabilities);
                }

                // write offset placeholders
                long classOffsetsOffset = writer.BaseStream.Position;
                foreach (int classId in classOffsets.Keys)
                {
                    int classOffset = classOffsets[classId];

                    writer.Write(classId);
                    writer.Write((int)-1);
                }
                writer.Write(-1);
                writer.Write(-1);

                // write trimmed inverted index
                foreach (int classId in invertedIndex.Keys)
                {
                    classOffsets[classId] = (int)writer.BaseStream.Position;

                    List<(int itemId, float probability)> probabilities = invertedIndex[classId];
                    foreach ((int itemId, float probability) in probabilities)
                    {
                        writer.Write(itemId);
                        writer.Write(probability);
                    }
                    writer.Write(-1);
                    writer.Write(-1);
                }

                // write offsets
                writer.BaseStream.Seek(classOffsetsOffset, SeekOrigin.Begin);
                foreach (int classId in classOffsets.Keys)
                {
                    int classOffset = classOffsets[classId];

                    writer.Write(classId);
                    writer.Write(classOffset);
                }
            }

            Console.WriteLine("DONE");
        }

        private static void TrimColorFile(string[] directoryFiles, int frameCount, string outputDirectory)
        {
            Console.Write("Trimming color signature file... ");

            string fileExtension = ColorSignatureIOBase.COLOR_SIGNATURES_EXTENSION;
            string inputPath = directoryFiles.Where(path => path.EndsWith(fileExtension)).Single();
            string outputPath = GenerateOutputPath(inputPath, outputDirectory, fileExtension);

            using (ColorSignatureReader reader = new ColorSignatureReader(inputPath))
            using (ColorSignatureWriter writer = new ColorSignatureWriter(outputPath, 
                _datasetHeader, reader.SignatureWidth, reader.SignatureHeight, frameCount))
            {
                for (int i = 0; i < frameCount; i++)
                {
                    writer.WriteDescriptor(reader.ReadDescriptor(i));
                    if (i % (frameCount / PROGRESS_BAR_WIDTH) == 0) { Console.Write("_"); }
                }
            }

            Console.WriteLine("DONE");
        }

        private static void TrimFloatVectorFile(string[] directoryFiles, int frameCount, string outputDirectory)
        {
            Console.Write("Trimming floatvector file... ");

            string fileExtension = FloatVectorIOBase.FLOAT_VECTOR_EXTENSION;
            string inputPath = directoryFiles.Where(path => path.EndsWith(fileExtension)).Single();
            string outputPath = GenerateOutputPath(inputPath, outputDirectory, fileExtension);

            using (FloatVectorReader reader = new FloatVectorReader(inputPath))
            using (FloatVectorWriter writer = new FloatVectorWriter(outputPath,
                _datasetHeader, reader.DescriptorLength, frameCount, reader.Source))
            {
                for (int i = 0; i < frameCount; i++)
                {
                    writer.WriteDescriptor(reader.ReadDescriptor(i));
                    if (i % (frameCount / PROGRESS_BAR_WIDTH) == 0) { Console.Write("_"); }
                }
            }

            Console.WriteLine("DONE");
        }

        private static void TrimMaskFilterFile(string[] directoryFiles, string fileExtension, int frameCount, string outputDirectory)
        {
            Console.Write($"Trimming mask filter file ({fileExtension})...");

            string inputPath = directoryFiles.Where(path => path.EndsWith(fileExtension)).Single();
            string outputPath = GenerateOutputPath(inputPath, outputDirectory, fileExtension);

            using (MaskFilterReader reader = new MaskFilterReader(inputPath))
            using (MaskFilterWriter writer = new MaskFilterWriter(outputPath,
                _datasetHeader, frameCount))
            {
                float[] filterValues = reader.ReadFilter().Take(frameCount).ToArray();
                writer.WriteFilter(filterValues);
            }

            Console.WriteLine("DONE");
        }

        private static void TrimBoolFile(string[] directoryFiles, string fileExtension, int frameCount, string outputDirectory)
        {
            Console.Write($"Trimming bool signature file ({fileExtension})... ");

            string inputPath = directoryFiles.Where(path => path.EndsWith(fileExtension)).Single();
            string outputPath = GenerateOutputPath(inputPath, outputDirectory, fileExtension);

            using (BoolSignatureReader reader = new BoolSignatureReader(inputPath))
            using (BoolSignatureWriter writer = new BoolSignatureWriter(outputPath,
                _datasetHeader, reader.SignatureWidth, reader.SignatureHeight, frameCount))
            {
                for (int i = 0; i < frameCount; i++)
                {
                    writer.WriteDescriptor(reader.ReadDescriptor(i));
                    if (i % (frameCount / PROGRESS_BAR_WIDTH) == 0) { Console.Write("_"); }
                }
            }

            Console.WriteLine("DONE");
        }

        private static void TrimFrameattributeFile(string[] directoryFiles, int frameCount, string outputDirectory)
        {
            Console.Write("Trimming frame attributes file...");

            string fileExtension = ".frameattributes";
            string inputPath = directoryFiles.Where(path => path.EndsWith(fileExtension)).Single();
            string outputPath = GenerateOutputPath(inputPath, outputDirectory, fileExtension);

            File.WriteAllLines(outputPath, File.ReadAllLines(inputPath).Take(frameCount));

            Console.WriteLine("DONE");
        }

        private static void TrimThumbnailsFile(string[] directoryFiles, int maxVideos, string outputDirectory)
        {
            Console.Write("Trimming thumbnails file... ");

            string fileExtension = ThumbnailIOBase.THUMBNAILS_EXTENSION;
            string inputPath = directoryFiles.Where(path => path.EndsWith(fileExtension)).Single();
            string outputPath = GenerateOutputPath(inputPath, outputDirectory, fileExtension);


            using (ThumbnailReader reader = new ThumbnailReader(inputPath))
            {
                int thumnailCount = reader.VideoFrameCounts.Take(maxVideos).Sum();
                (int videoId, int frameNumber)[] globalIdToVideoFramenumber 
                    = reader.GlobalIdToVideoFramenumber.Take(thumnailCount).ToArray();

                using (ThumbnailWriter writer = new ThumbnailWriter(outputPath,
                    _datasetHeader, reader.ThumbnailWidth, reader.ThumbnailHeight,
                    maxVideos, thumnailCount, reader.FramesPerSecond, globalIdToVideoFramenumber))
                {
                    for (int i = 0; i < thumnailCount; i++)
                    {
                        writer.WriteThumbnail(reader.ReadVideoThumbnail(i).JpegData);
                        if (i % (thumnailCount / PROGRESS_BAR_WIDTH) == 0) { Console.Write("_"); }
                    }
                }
            }
            Console.WriteLine("DONE");
        }



        private static string GenerateOutputPath(string inputPath, string outputDirectory, string fileExtension)
        {
            string outputFilename = Path.GetFileNameWithoutExtension(inputPath) + _filenameSuffix + fileExtension;
            string outputPath = Path.Combine(outputDirectory, outputFilename);
            return outputPath;
        }
    }
}
