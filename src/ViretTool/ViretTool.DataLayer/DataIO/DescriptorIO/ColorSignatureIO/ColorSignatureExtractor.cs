#define PARALLEL
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO
{
    public class ColorSignatureExtractor : ColorSignatureIOBase
    {
        public byte[] DatasetHeader { get; private set; }

        public int DescriptorCount { get; private set; }
        public int DescriptorLength => SignatureWidth * SignatureHeight * 3;

        public int SignatureWidth { get; }
        public int SignatureHeight { get; }

        public byte[][] Descriptors { get; private set; }

        public ColorSignatureExtractor(byte[] datasetHeader, 
            int signatureWidth, int signatureHeight)
        {
            DatasetHeader = datasetHeader;

            SignatureWidth = signatureWidth;
            SignatureHeight = signatureHeight;
        }

        public void RunExtraction(string[] inputFiles, Action<int> percentDoneCallback)
        {
            DescriptorCount = inputFiles.Length;
            Descriptors = new byte[DescriptorCount][];

#if PARALLEL
            ConcurrentQueue<(int id, string file)> queue 
                = new ConcurrentQueue<(int id, string file)>(
                    inputFiles.Select((file, id) => (id, file)));
            Parallel.For(0, inputFiles.Length, iTask =>
            {
                (int i, string inputFile) taskParameters;
                if (!queue.TryDequeue(out taskParameters))
                {
                    throw new Exception("Unexpected behavior when accessing ConcurrentQueue.");
                }
                int i = taskParameters.i;
                string inputFile = taskParameters.inputFile;
#else
            for (int i = 0; i < inputFiles.Length; i++)
            {
                string inputFile = inputFiles[i];
#endif
                Descriptors[i] = new byte[DescriptorLength];
                int iDescriptorCell = 0;

                Bitmap bitmap = new Bitmap(inputFile);
                Bitmap resized = new Bitmap(bitmap, SignatureWidth, SignatureHeight);

                for (int y = 0; y < SignatureHeight; y++)
                {
                    for (int x = 0; x < SignatureWidth; x++)
                    {
                        // TODO: optimize
                        Color color = resized.GetPixel(x, y);

                        // changed to LAB
                        CIELab lab = RGBtoLab(color.R, color.G, color.B);
                        Tuple<byte, byte, byte> labNormalized = ProjectLabToByte(lab);

                        Descriptors[i][iDescriptorCell++] = labNormalized.Item1;
                        Descriptors[i][iDescriptorCell++] = labNormalized.Item2;
                        Descriptors[i][iDescriptorCell++] = labNormalized.Item3;
                    }
                }

                // dispose bitmaps to avoid out-of-memory exception
                bitmap.Dispose();
                resized.Dispose();

                // report status
                int percentsDone = (int)(((double)i / DescriptorCount) * 100);
                if ((i % (DescriptorCount / 100)) == 0)
                {
                    percentDoneCallback?.Invoke(percentsDone);
                }
            }
#if PARALLEL
            );
#endif
        }

        public override void Dispose()
        {
            // nothing to do here
        }


        private static Tuple<byte, byte, byte> ProjectLabToByte(CIELab lab)
        {
            //maxL = 100, maxA = 98,2497239576523, maxB = 94,4877196299886
            //minL = 0, minA = -86,1884340941196, minB = -107,853734252327

            double normalizer = 255.0 / (lab.bMaxValue - lab.bMinValue);

            byte L = (byte)(lab.L * normalizer);
            byte A = (byte)((lab.A - lab.aMinValue) * normalizer);
            byte B = (byte)((lab.B - lab.bMinValue) * normalizer);

            return new Tuple<byte, byte, byte>(L, A, B);
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


            public double lMaxValue { get { return 100; } }
            public double lMinValue { get { return 0; } }
            public double aMaxValue { get { return 98.250; } }
            public double aMinValue { get { return -86.189; } }
            public double aMaxValueAbs { get { return 98.250; } }
            public double bMaxValue { get { return 94.488; } }
            public double bMinValue { get { return -107.854; } }
            public double bMaxValueAbs { get { return 107.854; } }


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
    }
}
