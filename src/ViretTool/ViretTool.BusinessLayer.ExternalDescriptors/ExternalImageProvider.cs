using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace ViretTool.BusinessLayer.ExternalDescriptors
{
    public class ExternalImageProvider
    {
        private const string ImageDirectory = "Images";
        private const string ImageUrlPrefix = "imgurl=";

        public ExternalImageProvider()
        {
            if (!Directory.Exists(ImageDirectory))
            {
                Directory.CreateDirectory(ImageDirectory);
            }
        }

        public string ParseAndDownloadImage(string wholeUrl)
        {
            int imageUrlStart = wholeUrl.IndexOf(ImageUrlPrefix, StringComparison.InvariantCultureIgnoreCase);
            if (imageUrlStart == -1)
            {
                return null;
            }

            imageUrlStart += ImageUrlPrefix.Length;


            int imageUrlEnd = wholeUrl.IndexOf('&', imageUrlStart);
            string imageUrl = imageUrlEnd == -1 ? wholeUrl.Substring(imageUrlStart) : wholeUrl.Substring(imageUrlStart, imageUrlEnd - imageUrlStart);

            return DownloadImage(Uri.UnescapeDataString(imageUrl));
        }

        private string DownloadImage(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(url);
                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    using (Image image = Image.FromStream(memoryStream))
                    {
                        using (Image resizedImage = ResizeImage(image, 255, 255))
                        {
                            string imagePath = Path.Combine(ImageDirectory, $"{GetCurrentDateTimeString()}.png");
                            resizedImage.Save(imagePath, ImageFormat.Png);
                            return imagePath;
                        }
                    }
                }
            }
        }

        private static string GetCurrentDateTimeString()
        {
            return DateTime.UtcNow.ToString("O").Replace(':', '_').Replace('.', '_');
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destinationRectangle = new Rectangle(0, 0, width, height);
            Bitmap destinationImage = new Bitmap(width, height);

            destinationImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destinationImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destinationImage;
        }
    }
}
