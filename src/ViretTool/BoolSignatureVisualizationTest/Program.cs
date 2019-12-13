using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Thumbnails;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataIO.DatasetIO;


namespace BoolSignatureVisualizationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputDirectory = args[0];
            string inputExtension = args[1];
            string outputDirectory = args[2];
            int videoId = int.Parse(args[3]);

            Dataset dataset = new DatasetProvider().FromDirectory(inputDirectory);
            outputDirectory = Path.Combine(outputDirectory, videoId.ToString("00000") + "-sigatures");
            Directory.CreateDirectory(outputDirectory);

            BoolSignatureDescriptorProvider colorSignatureProvider
                = BoolSignatureDescriptorProvider.FromDirectory(inputDirectory, inputExtension);
            JpegThumbnailService thumbnailService = new JpegThumbnailService(inputDirectory);

            (int frameNumber, bool[] signature)[] signatures = dataset
                .Videos[videoId]
                .Frames
                .Select(f => (f.FrameNumber, colorSignatureProvider.Descriptors[f.Id]))
                .ToArray();

            Console.WriteLine(
                $"Writing {signatures.Length} color signatures of video ID: {videoId} to \"{outputDirectory}\"");
            foreach ((int frameNumber, bool[] signature) frameSignature in signatures)
            {
                string outputFile = Path.Combine(outputDirectory, frameSignature.frameNumber.ToString("000000") + ".png");

                Thumbnail<byte[]> thumbnail = thumbnailService.GetThumbnail(videoId, frameSignature.frameNumber);
                Bitmap thumbnailBitmap = new Bitmap(new MemoryStream(thumbnail.Image));
                Bitmap signatureBitmap = new Bitmap(colorSignatureProvider.SignatureWidth, colorSignatureProvider.SignatureHeight);
                int iPixel = 0;

                for (int y = 0; y < signatureBitmap.Height; y++)
                {
                    for (int x = 0; x < signatureBitmap.Width; x++)
                    {
                        bool isMasked = frameSignature.signature[iPixel];
                        iPixel++;
                        Color color = isMasked ? Color.White : Color.Black;
                        signatureBitmap.SetPixel(x, y, color);
                    }
                }


                Bitmap outputBitmap = new Bitmap(thumbnailBitmap.Width, thumbnailBitmap.Height * 2);
                using (Graphics gfx = Graphics.FromImage(outputBitmap))
                {
                    gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
                    gfx.PixelOffsetMode = PixelOffsetMode.Half;

                    gfx.DrawImage(thumbnailBitmap,
                                     new Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height / 2),
                                     new Rectangle(0, 0, thumbnailBitmap.Width, thumbnailBitmap.Height),
                                     GraphicsUnit.Pixel);
                    gfx.DrawImage(signatureBitmap,
                                     new Rectangle(0, thumbnailBitmap.Height, outputBitmap.Width, outputBitmap.Height / 2),
                                     new Rectangle(0, 0, signatureBitmap.Width, signatureBitmap.Height),
                                     GraphicsUnit.Pixel);

                    gfx.Save();
                }


                outputBitmap.Save(outputFile, ImageFormat.Png);
                Console.WriteLine($"Written: {outputFile}");
            }

            Console.WriteLine("Done!");
        }
    }
}
