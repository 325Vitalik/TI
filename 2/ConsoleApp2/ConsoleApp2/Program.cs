using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using AForge.Imaging.ColorReduction;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap bmp = new Bitmap("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\2\\assets\\asmund-gimre-7SjmvQgBSKY-unsplash.jpg");
            Console.WriteLine($"Entropy of image: {FindEntropyOfImage(bmp)}");

            Bitmap sampled2 = new Bitmap(bmp, new Size(bmp.Width / 2, bmp.Height / 2));
            sampled2.Save("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\2\\assets\\sampled2.jpg", ImageFormat.Jpeg);
            Console.WriteLine($"Entropy of sampled 2 image: {FindEntropyOfImage(sampled2)}");

            Bitmap sampled4 = new Bitmap(bmp, new Size(bmp.Width / 4, bmp.Height / 4));
            sampled4.Save("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\2\\assets\\sampled4.jpg", ImageFormat.Jpeg);
            Console.WriteLine($"Entropy of sampled 4 image: {FindEntropyOfImage(sampled4)}\n");

            var qu = new ColorImageQuantizer(new MedianCutQuantizer());

            quatizeAsync(qu, sampled2, 2, 8);
            quatizeAsync(qu, sampled2, 2, 16);
            quatizeAsync(qu, sampled2, 2, 64);
            quatizeAsync(qu, sampled4, 4, 8);
            quatizeAsync(qu, sampled4, 4, 16);
            quatizeAsync(qu, sampled4, 4, 64);
        }

        public static void quatizeAsync(ColorImageQuantizer qu, Bitmap bmp, int n, int colors)
        {
            Console.WriteLine("\n-----------------------------------------------------------------------------------------------------\n");
            var quantized = qu.ReduceColors(bmp, colors);
            quantized.Save($"C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\2\\assets\\quntized_{n}_{colors}.jpg", ImageFormat.Jpeg);
            Console.WriteLine($"Entropy of quntized_{n}_{colors}: {FindEntropyOfImage(quantized)}");
            Console.WriteLine($"Relative entropy: {FindRelativeEntropyOfTwoImages(bmp, quantized)}\n");

            var restoredBicubic = EnlargeImage(quantized, n, InterpolationMode.Bicubic);
            restoredBicubic.Save($"C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\2\\assets\\quntized_{n}_{colors}_restored_bicubic.jpg", ImageFormat.Jpeg);
            Console.WriteLine($"Entropy of quntized_{n}_{colors}_restored_bicubic: {FindEntropyOfImage(restoredBicubic)}");
            Console.WriteLine($"Relative entropy: {FindRelativeEntropyOfTwoImages(bmp, restoredBicubic)}\n");

            var restoredBilinear = EnlargeImage(quantized, n, InterpolationMode.Bilinear);
            restoredBilinear.Save($"C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\2\\assets\\quntized_{n}_{colors}_restored_bilibiar.jpg", ImageFormat.Jpeg);
            Console.WriteLine($"Entropy of quntized_{n}_{colors}_restored_bilinear: {FindEntropyOfImage(restoredBilinear)}");
            Console.WriteLine($"Relative entropy: {FindRelativeEntropyOfTwoImages(bmp, restoredBilinear)}\n");
        }

        public static Bitmap EnlargeImage(Bitmap original, int scale, InterpolationMode interpolationMode)
        {
            Bitmap newimg = new Bitmap(original.Width * scale, original.Height * scale);

            using (Graphics g = Graphics.FromImage(newimg))
            {
                g.InterpolationMode = interpolationMode;

                g.DrawImage(original, new Rectangle(Point.Empty, newimg.Size));
            }

            return newimg;
        }

        public static double FindEntropy(double[] arr)
        {
            double res = 0;
            foreach(var i in arr)
            {
                res += i * Math.Log2(i);
            }
            return -res;
        }

        public static double FindRelativeEntropyOfTwoImages(Bitmap img1, Bitmap img2)
        {
            var numOfPixels = img1.Width * img1.Height;
            var pixelsMap1 = new Dictionary<byte, long>();
            var pixelsMap2 = new Dictionary<byte, long>();
            GroupPixels(img1, pixelsMap1);
            GroupPixels(img2, pixelsMap2);

            var probability1 = new Dictionary<byte, double>();
            foreach (KeyValuePair<byte, long> entry in pixelsMap1)
            {
                probability1.Add(entry.Key, entry.Value / (double)numOfPixels);
            }

            var probability2 = new Dictionary<byte, double>();
            foreach (KeyValuePair<byte, long> entry in pixelsMap2)
            {
                probability2.Add(entry.Key, entry.Value / (double)numOfPixels);
            }

            double relativeProbability = 0;
            for (byte i = 0; i <= 255; i++)
            {
                var px = probability1.GetValueOrDefault(i, 0);
                var qx = probability2.GetValueOrDefault(i, 0);
                if(px == 0)
                {
                    continue;
                }
                if(qx == 0)
                {
                    relativeProbability = Double.PositiveInfinity;
                    break;
                }

                relativeProbability += px * Math.Log2(px / qx);
                if(i == 255)
                {
                    break;
                }
            }
            return relativeProbability;
        }

        public static double FindEntropyOfImage(Bitmap bmp)
        {
            var numOfPixels = bmp.Width * bmp.Height;
            var pixelsMap = new Dictionary<byte, long>();
            GroupPixels(bmp, pixelsMap);

            var probability = new Dictionary<byte, double>();
            foreach (KeyValuePair<byte, long> entry in pixelsMap)
            {
                probability.Add(entry.Key, entry.Value / (double)numOfPixels);
            }

            var entropy = FindEntropy(probability.Values.ToArray());
            return entropy;
        }

        public static void GroupPixels(Bitmap bmp, Dictionary<byte, long> groups)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                var r = rgbValues[counter];
                if (groups.TryGetValue(r, out var numberOfPixels))
                {
                    groups[r]++;
                }
                else
                {
                    groups.Add(r, 1);
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            bmp.UnlockBits(bmpData);
        }

        public static Bitmap ChangePixels(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                var r = rgbValues[counter];
                var g = rgbValues[counter + 1];
                var b = rgbValues[counter + 2];
                var tmp = (byte)(r * 0.3 + g * 0.59 + b * 0.11);

                rgbValues[counter] = tmp;
                rgbValues[counter + 1] = tmp;
                rgbValues[counter + 2] = tmp;
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        //public static Bitmap ResizeImage(Image image, int width, int height)
        //{
        //    var destRect = new Rectangle(0, 0, width, height);
        //    var destImage = new Bitmap(width, height);

        //    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        //    using (var graphics = Graphics.FromImage(destImage))
        //    {
        //        graphics.CompositingMode = CompositingMode.SourceCopy;
        //        graphics.CompositingQuality = CompositingQuality.HighQuality;
        //        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        graphics.SmoothingMode = SmoothingMode.HighQuality;
        //        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //        using (var wrapMode = new ImageAttributes())
        //        {
        //            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        //            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        //        }
        //    }

        //    return destImage;
        //}
    }
}
