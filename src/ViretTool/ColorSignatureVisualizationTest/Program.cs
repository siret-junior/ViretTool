﻿using System;
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
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ColorSignatureVisualizationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputDirectory = args[0];
            string outputDirectory = args[1];
            int videoId = int.Parse(args[2]);

            Dataset dataset = new DatasetProvider().FromDirectory(inputDirectory);
            outputDirectory = Path.Combine(outputDirectory, videoId.ToString("00000") + "-sigatures");
            Directory.CreateDirectory(outputDirectory);

            ColorSignatureDescriptorProvider colorSignatureProvider 
                = ColorSignatureDescriptorProvider.FromDirectory(inputDirectory);
            JpegThumbnailService thumbnailService = new JpegThumbnailService(inputDirectory);

            (int frameNumber, byte[] signature)[] signatures = dataset
                .Videos[videoId]
                .Frames
                .Select(f => (f.FrameNumber, colorSignatureProvider.Descriptors[f.Id]))
                .ToArray();

            Console.WriteLine(
                $"Writing {signatures.Length} color signatures of video ID: {videoId} to \"{outputDirectory}\"");
            foreach ((int frameNumber, byte[] signature) frameSignature in signatures)
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
                        byte L = frameSignature.signature[iPixel];
                        byte a = frameSignature.signature[iPixel + 1];
                        byte b = frameSignature.signature[iPixel + 2];

                        CIELab lab = ProjectByteToLab(L, a, b);
                        Color color = LabtoColor(lab);
                        signatureBitmap.SetPixel(x, y, color);
                        iPixel += 3;
                    }
                }


                Bitmap outputBitmap = new Bitmap(thumbnailBitmap.Width, thumbnailBitmap.Height * 2);
                using (Graphics gfx = Graphics.FromImage(outputBitmap))
                {
                    gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
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



        private static Tuple<byte, byte, byte> ProjectLabToByte(CIELab lab)
        {
            //maxL = 100, maxA = 98,2497239576523, maxB = 94,4877196299886
            //minL = 0, minA = -86,1884340941196, minB = -107,853734252327

            double normalizer = 255.0 / (CIELab.bMaxValue - CIELab.bMinValue);

            byte L = (byte)(lab.L * normalizer);
            byte A = (byte)((lab.A - CIELab.aMinValue) * normalizer);
            byte B = (byte)((lab.B - CIELab.bMinValue) * normalizer);

            return new Tuple<byte, byte, byte>(L, A, B);
        }

        private static CIELab ProjectByteToLab(byte byteL, byte byteA, byte byteB)
        {
            //maxL = 100, maxA = 98,2497239576523, maxB = 94,4877196299886
            //minL = 0, minA = -86,1884340941196, minB = -107,853734252327

            double normalizer = 255.0 / (CIELab.bMaxValue - CIELab.bMinValue);
            
            double L = byteL / normalizer;
            double A = byteA / normalizer + CIELab.aMinValue;
            double B = byteB / normalizer + CIELab.bMinValue;

            
            return new CIELab(L, A, B);
        }





        /// <summary>
        /// Structure to define CIE L*a*b*.
        /// </summary>
        private struct CIELab
        {
            /// <summary>
            /// Gets an empty CIELab structure.
            /// </summary>
            public static readonly CIELab Empty = new CIELab();

            private double l;
            private double a;
            private double b;


            public static double lMaxValue { get { return 100; } }
            public static double lMinValue { get { return 0; } }
            public static double aMaxValue { get { return 98.250; } }
            public static double aMinValue { get { return -86.189; } }
            public static double aMaxValueAbs { get { return 98.250; } }
            public static double bMaxValue { get { return 94.488; } }
            public static double bMinValue { get { return -107.854; } }
            public static double bMaxValueAbs { get { return 107.854; } }


            public static bool operator ==(CIELab item1, CIELab item2)
            {
                return (
                    item1.L == item2.L
                    && item1.A == item2.A
                    && item1.B == item2.B
                    );
            }

            public static bool operator !=(CIELab item1, CIELab item2)
            {
                return (
                    item1.L != item2.L
                    || item1.A != item2.A
                    || item1.B != item2.B
                    );
            }


            /// <summary>
            /// Gets or sets L component.
            /// </summary>
            public double L
            {
                get
                {
                    return this.l;
                }
                set
                {
                    this.l = value;
                }
            }

            /// <summary>
            /// Gets or sets a component.
            /// </summary>
            public double A
            {
                get
                {
                    return this.a;
                }
                set
                {
                    this.a = value;
                }
            }

            /// <summary>
            /// Gets or sets a component.
            /// </summary>
            public double B
            {
                get
                {
                    return this.b;
                }
                set
                {
                    this.b = value;
                }
            }

            public CIELab(double l, double a, double b)
            {
                this.l = l;
                this.a = a;
                this.b = b;
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;

                return (this == (CIELab)obj);
            }

            public override int GetHashCode()
            {
                return L.GetHashCode() ^ a.GetHashCode() ^ b.GetHashCode();
            }

        }


        /// <summary>
        /// Structure to define CIE XYZ.
        /// </summary>
        private struct CIEXYZ
        {
            /// <summary>
            /// Gets an empty CIEXYZ structure.
            /// </summary>
            public static readonly CIEXYZ Empty = new CIEXYZ();
            /// <summary>
            /// Gets the CIE D65 (white) structure.
            /// </summary>
            public static readonly CIEXYZ D65 = new CIEXYZ(0.9505, 1.0, 1.0890);


            private double x;
            private double y;
            private double z;

            public static bool operator ==(CIEXYZ item1, CIEXYZ item2)
            {
                return (
                    item1.X == item2.X
                    && item1.Y == item2.Y
                    && item1.Z == item2.Z
                    );
            }

            public static bool operator !=(CIEXYZ item1, CIEXYZ item2)
            {
                return (
                    item1.X != item2.X
                    || item1.Y != item2.Y
                    || item1.Z != item2.Z
                    );
            }

            /// <summary>
            /// Gets or sets X component.
            /// </summary>
            public double X
            {
                get
                {
                    return this.x;
                }
                set
                {
                    this.x = (value > 0.9505) ? 0.9505 : ((value < 0) ? 0 : value);
                }
            }

            /// <summary>
            /// Gets or sets Y component.
            /// </summary>
            public double Y
            {
                get
                {
                    return this.y;
                }
                set
                {
                    this.y = (value > 1.0) ? 1.0 : ((value < 0) ? 0 : value);
                }
            }

            /// <summary>
            /// Gets or sets Z component.
            /// </summary>
            public double Z
            {
                get
                {
                    return this.z;
                }
                set
                {
                    this.z = (value > 1.089) ? 1.089 : ((value < 0) ? 0 : value);
                }
            }

            public CIEXYZ(double x, double y, double z)
            {
                this.x = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
                this.y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
                this.z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;

                return (this == (CIEXYZ)obj);
            }

            public override int GetHashCode()
            {
                return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
            }

        }


        /// <summary>
        /// RGB structure.
        /// </summary>
        public struct RGB
        {
            /// <summary>
            /// Gets an empty RGB structure;
            /// </summary>
            public static readonly RGB Empty = new RGB();

            private int red;
            private int green;
            private int blue;

            public static bool operator ==(RGB item1, RGB item2)
            {
                return (
                    item1.Red == item2.Red
                    && item1.Green == item2.Green
                    && item1.Blue == item2.Blue
                    );
            }

            public static bool operator !=(RGB item1, RGB item2)
            {
                return (
                    item1.Red != item2.Red
                    || item1.Green != item2.Green
                    || item1.Blue != item2.Blue
                    );
            }

            /// <summary>
            /// Gets or sets red value.
            /// </summary>
            public int Red
            {
                get
                {
                    return red;
                }
                set
                {
                    red = (value > 255) ? 255 : ((value < 0) ? 0 : value);
                }
            }

            /// <summary>
            /// Gets or sets red value.
            /// </summary>
            public int Green
            {
                get
                {
                    return green;
                }
                set
                {
                    green = (value > 255) ? 255 : ((value < 0) ? 0 : value);
                }
            }

            /// <summary>
            /// Gets or sets red value.
            /// </summary>
            public int Blue
            {
                get
                {
                    return blue;
                }
                set
                {
                    blue = (value > 255) ? 255 : ((value < 0) ? 0 : value);
                }
            }

            public RGB(int R, int G, int B)
            {
                this.red = (R > 255) ? 255 : ((R < 0) ? 0 : R);
                this.green = (G > 255) ? 255 : ((G < 0) ? 0 : G);
                this.blue = (B > 255) ? 255 : ((B < 0) ? 0 : B);
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;

                return (this == (RGB)obj);
            }

            public override int GetHashCode()
            {
                return Red.GetHashCode() ^ Green.GetHashCode() ^ Blue.GetHashCode();
            }
        }


        /// <summary>
        /// Converts RGB to CIE XYZ (CIE 1931 color space)
        /// </summary>
        private static CIEXYZ RGBtoXYZ(int red, int green, int blue)
        {
            // normalize red, green, blue values
            double rLinear = (double)red / 255.0;
            double gLinear = (double)green / 255.0;
            double bLinear = (double)blue / 255.0;

            // convert to a sRGB form
            double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (
                1 + 0.055), 2.2) : (rLinear / 12.92);
            double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (
                1 + 0.055), 2.2) : (gLinear / 12.92);
            double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (
                1 + 0.055), 2.2) : (bLinear / 12.92);

            // converts
            return new CIEXYZ(
                (r * 0.4124 + g * 0.3576 + b * 0.1805),
                (r * 0.2126 + g * 0.7152 + b * 0.0722),
                (r * 0.0193 + g * 0.1192 + b * 0.9505)
                );
        }

        /// <summary>
        /// XYZ to L*a*b* transformation function.
        /// </summary>
        private static double Fxyz(double t)
        {
            return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
        }

        /// <summary>
        /// Converts CIEXYZ to CIELab.
        /// </summary>
        private static CIELab XYZtoLab(double x, double y, double z)
        {
            CIELab lab = CIELab.Empty;

            lab.L = 116.0 * Fxyz(y / CIEXYZ.D65.Y) - 16;
            lab.A = 500.0 * (Fxyz(x / CIEXYZ.D65.X) - Fxyz(y / CIEXYZ.D65.Y));
            lab.B = 200.0 * (Fxyz(y / CIEXYZ.D65.Y) - Fxyz(z / CIEXYZ.D65.Z));

            return lab;
        }

        /// <summary>
        /// Converts RGB to CIELab.
        /// </summary>
        private static CIELab RGBtoLab(int red, int green, int blue)
        {
            return XYZtoLab(RGBtoXYZ(red, green, blue));
        }
        
        private static CIELab XYZtoLab(CIEXYZ cIEXYZ)
        {
            return XYZtoLab(cIEXYZ.X, cIEXYZ.Y, cIEXYZ.Z);
        }

        /// <summary>
        /// Converts CIELab to RGB.
        /// </summary>
        private static RGB LabtoRGB(CIELab lab)
        {
            CIEXYZ xyz = LabtoXYZ(lab.L, lab.A, lab.B);
            return XYZtoRGB(xyz.X, xyz.Y, xyz.Z);
        }

        /// <summary>
        /// Converts CIELab to Color.
        /// </summary>
        private static Color LabtoColor(CIELab lab)
        {
            RGB rgb = LabtoRGB(lab);
            return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
        }

        /// <summary>
        /// Converts CIELab to CIEXYZ.
        /// </summary>
        private static CIEXYZ LabtoXYZ(double l, double a, double b)
        {
            double delta = 6.0 / 29.0;

            double fy = (l + 16) / 116.0;
            double fx = fy + (a / 500.0);
            double fz = fy - (b / 200.0);

            return new CIEXYZ(
                (fx > delta) ? CIEXYZ.D65.X * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.X,
                (fy > delta) ? CIEXYZ.D65.Y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.Y,
                (fz > delta) ? CIEXYZ.D65.Z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.Z
                );
        }

        /// <summary>
        /// Converts CIEXYZ to RGB structure.
        /// </summary>
        private static RGB XYZtoRGB(double x, double y, double z)
        {
            double[] Clinear = new double[3];
            Clinear[0] = x * 3.2410 - y * 1.5374 - z * 0.4986; // red
            Clinear[1] = -x * 0.9692 + y * 1.8760 - z * 0.0416; // green
            Clinear[2] = x * 0.0556 - y * 0.2040 + z * 1.0570; // blue

            for (int i = 0; i < 3; i++)
            {
                Clinear[i] = (Clinear[i] <= 0.0031308) ? 12.92 * Clinear[i] : (
                    1 + 0.055) * Math.Pow(Clinear[i], (1.0 / 2.4)) - 0.055;
            }

            return new RGB(
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[0] * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[1] * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[2] * 255.0)))
                );
        }
    }
}
