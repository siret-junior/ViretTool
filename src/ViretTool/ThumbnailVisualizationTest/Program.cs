using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Thumbnails;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ThumbnailVisualizationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputDirectory = args[0];
            string outputDirectory = args[1];
            int videoId = int.Parse(args[2]);

            //Dataset dataset = new DatasetProvider().FromDirectory(inputDirectory);
            JpegThumbnailService thumbnailService = new JpegThumbnailService(inputDirectory);
            outputDirectory = Path.Combine(outputDirectory, videoId.ToString("00000"));
            Directory.CreateDirectory(outputDirectory);

            Thumbnail<byte[]>[] thumbnails = thumbnailService.GetThumbnails(videoId);
            Console.WriteLine($"Writing {thumbnails.Length} images of video ID: {videoId} to \"{outputDirectory}\"");
            foreach (Thumbnail<byte[]> thumbnail in thumbnails)
            {
                string outputFile = Path.Combine(outputDirectory, thumbnail.FrameNumber.ToString("000000") + ".jpg");
                Bitmap bitmap = new Bitmap(new MemoryStream(thumbnail.Image));
                bitmap.Save(outputFile, ImageFormat.Jpeg);
                Console.WriteLine($"Written: {outputFile}");
            }

            Console.WriteLine("Done!");
        }
    }
}
