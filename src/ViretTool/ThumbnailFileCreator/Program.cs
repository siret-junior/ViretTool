#define PARALLEL
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.ThumbnailIO;

namespace ThumbnailFileCreator
{
    public class Program
    {
        /// <summary>
        /// V3C1 arguments:
        /// 128 72 4 "V3C1\Keyframes\images" "V3C1.dataset" "V3C1.thumbs"
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("ThumbnailFileCreator.exe <frameWidth> <frameHeight> <framerate>\n" +
                "<inputDirectory> <headerContainingFile> <outputFile>");
        }

        public static void Main(string[] args)
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            // parse program arguments
            int frameWidth, frameHeight, framesPerSecond;
            try
            {
                ParseArguments(args, out frameWidth, out frameHeight, out framesPerSecond);
            }
            catch
            {
                Console.Error.WriteLine("Error parsing program arguments!");
                PrintUsage();
                return;
            }
            string inputDirectory = Path.GetFullPath(args[3]);
            string headerContainingFile = Path.GetFullPath(args[4]);
            string outputFile = Path.GetFullPath(args[5]);



            // count files and directories
            int frameCount, videoCount;
            CountFramesAndVideos(inputDirectory, 
                out frameCount, out videoCount, out (int videoId, int frameNumber)[] keys);

            // run merging
            MergeFramesToBinaryFile(inputDirectory, headerContainingFile, outputFile,
                frameWidth, frameHeight, framesPerSecond, frameCount, videoCount, keys);
        }


        private static void ParseArguments(string[] args,
            out int frameWidth, out int frameHeight, out int framesPerSecond)
        {
            frameWidth = int.Parse(args[0]);
            frameHeight = int.Parse(args[1]);
            framesPerSecond = int.Parse(args[2]);
        }
        

        private static void CountFramesAndVideos(string inputDirectory, 
            out int frameCount, out int videoCount, out (int videoId, int frameNumber)[] thumbnailKeys)
        {
            Console.Write("Counting frames and videos... ");
            frameCount = 0;
            videoCount = 0;
            List<(int videoId, int frameNumber)> keys = new List<(int videoId, int frameNumber)>();

            string[] videoDirectories = Directory.GetDirectories(inputDirectory).OrderBy(x => x).ToArray();
            foreach (string videoDirectory in videoDirectories)
            {
                string[] filenames = Directory.GetFiles(videoDirectory).OrderBy(x => x).ToArray();
                for (int iFile = 0; iFile < filenames.Length; iFile++)
                {
                    ParseFrameHeirarchy(Path.GetFileName(filenames[iFile]),
                        out int videoId, 
                        out int frameNumber,
                        out decimal seconds,
                        //out string datetime,
                        out string extension);
                    keys.Add((videoCount, frameNumber));
                }

                frameCount += filenames.Length;
                videoCount++;
            }
            thumbnailKeys = keys.ToArray();
            Console.WriteLine("DONE!");
        }
        
        private static readonly System.Text.RegularExpressions.Regex _tokenFormatRegex
            = new System.Text.RegularExpressions.Regex(
            @"^[Vv](?<videoId>[0-9]+)"
            + @"_"
            + @"[Ff](?<frameNumber>[0-9]+)"
            + @"_"
            + @"(?<seconds>[0-9\.]+)sec"
            //+ @"_"
            //+ @"[Dd](?<datetime>[^\.]+)"
            + @"\.(?<extension>.*)$",
            System.Text.RegularExpressions.RegexOptions.ExplicitCapture);

        private static void ParseFrameHeirarchy(string inputString,
            out int videoId,
            out int frameNumber,
            out decimal seconds,
            //out string datetime,
            out string extension)
        {
            System.Text.RegularExpressions.Match match = _tokenFormatRegex.Match(inputString);
            if (!match.Success)
            {
                throw new ArgumentException("Unknown interaction token format: " + inputString);
            }

            videoId = int.Parse(match.Groups["videoId"].Value);
            frameNumber = int.Parse(match.Groups["frameNumber"].Value);
            seconds = decimal.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture);
            //datetime = match.Groups["datetime"].Value;
            extension = match.Groups["extension"].Value;
        }


        private static void MergeFramesToBinaryFile(
            string inputDirectory, string headerContainingFile, string outputFile, 
            int frameWidth, int frameHeight, int framesPerSecond, 
            int frameCount, int videoCount, (int videoId, int frameNumber)[] thumbnailKeys)
        {
            // statistics variables
            Stopwatch stopwatch = Stopwatch.StartNew();
            int processedVideosCount = 0;

            // merge frames
            Console.WriteLine("Merging frame images:");
            try
            {
                // copy file header
                byte[] header;
                using (BinaryReader reader = new BinaryReader(File.OpenRead(headerContainingFile)))
                {
                    int headerLength = reader.ReadInt32();
                    header = reader.ReadBytes(headerLength);
                }


                using (ThumbnailWriter writer = new ThumbnailWriter(outputFile,
                    header, frameWidth, frameHeight, videoCount, frameCount, framesPerSecond, thumbnailKeys))
                {
                    // load video directories
                    string[] videoDirectories = Directory.GetDirectories(inputDirectory).OrderBy(x => x).ToArray();
                    
                    foreach (string videoDirectory in videoDirectories)
                    {
                        Console.Write("Processing video ID {0}... ", Path.GetFileName(videoDirectory));

                        // load video frames
                        string[] filenames = Directory.GetFiles(videoDirectory).OrderBy(x => x).ToArray();

                        // convert images in parallel
                        byte[][] resizedImageData = new byte[filenames.Length][];
#if PARALLEL && !DEBUG
                        Parallel.For(0, filenames.Length, index =>
#else
                        for (int index = 0; index < filenames.Length; index++)
#endif
                        {
                            string filename = filenames[index];
                            byte[] originalImageData = File.ReadAllBytes(filename);
                            resizedImageData[index] = PreprocessImage(originalImageData, frameWidth, frameHeight);
                        }
#if PARALLEL && !DEBUG
                        );
#endif
                        // append to binary file
                        for (int i = 0; i < filenames.Length; i++)
                        {
                            writer.WriteThumbnail(resizedImageData[i]);
                        }
                        
                        Console.WriteLine("DONE!");

                        // compute and print statistics
                        processedVideosCount++;
                        PrintVideoStatistics(stopwatch, processedVideosCount, videoDirectories, videoDirectory);
                    }
                    PrintFinalStatistics(stopwatch, videoDirectories);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return;
            }
        }

        private static void PrintFinalStatistics(Stopwatch stopwatch, string[] videoDirectories)
        {
            int secondsElapsed = (int)(stopwatch.ElapsedMilliseconds * 0.001);
            int hoursElapsed = secondsElapsed / (60 * 60);
            int minutesElapsed = secondsElapsed / 60 % 60;
            secondsElapsed = secondsElapsed % 60;
            Console.WriteLine("Merging of {0} files finished in {1}h {2}m {3}s.", videoDirectories.Length,
                hoursElapsed.ToString("00"), minutesElapsed.ToString("00"), secondsElapsed.ToString("00"));
        }

        private static void PrintVideoStatistics(Stopwatch stopwatch, int processedVideosCount, string[] videoDirectories, string videoDirectory)
        {
            double processedPerSecond = processedVideosCount / (stopwatch.ElapsedMilliseconds * 0.001);
            int secondsRemaining = (int)((videoDirectories.Length - processedVideosCount) / processedPerSecond);
            int hoursRemaining = secondsRemaining / (60 * 60);
            int minutesRemaining = secondsRemaining / 60 % 60;
            secondsRemaining = secondsRemaining % 60;
            Console.WriteLine("Video ID: {0} processed: {1} of {2}, ({3} per second, {4}h {5}m {6}s remaining).",
                Path.GetFileName(videoDirectory),
                processedVideosCount, videoDirectories.Length, processedPerSecond.ToString("0.000"),
                hoursRemaining.ToString("00"), minutesRemaining.ToString("00"), secondsRemaining.ToString("00"));
        }




        private static byte[] PreprocessImage(byte[] jpgData, int width, int height)
        {
            byte[] result;
            using (MemoryStream inputStream = new MemoryStream(jpgData))
            using (MemoryStream outputStream = new MemoryStream())
            {
                Image originalImage = Image.FromStream(inputStream);
                Image resizedImage = new Bitmap(width, height);
                using (Graphics gfx = Graphics.FromImage(resizedImage))
                {
                    ImageAttributes attributes = new ImageAttributes();     // ghost edges fix
                    attributes.SetWrapMode(WrapMode.TileFlipXY);

                    gfx.SmoothingMode = SmoothingMode.HighQuality;
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    //gfx.DrawImage(originalImage, new Rectangle(0, 0, width, height));

                    gfx.DrawImage(originalImage,
                        new Rectangle(0, 0, width, height),
                        0, 0, originalImage.Width, originalImage.Height,
                        GraphicsUnit.Pixel, attributes);
                }
                resizedImage.Save(outputStream, ImageFormat.Jpeg);
                result = outputStream.ToArray();
            }
            return result;
        }
        

    }
}
