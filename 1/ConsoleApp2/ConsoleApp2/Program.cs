using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Metadata;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var px = new double[] { 0.15, 0.07, 0.09, 0.08, 0.09, 0.12, 0.04, 0.13, 0.12, 0.10 };
            Console.WriteLine("Entropy of sequence: " + FindEntropy(px) + "\n\n");

            Bitmap bmp = new Bitmap("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\1\\assets\\harrison-qi-PBFtL7_RFJk-unsplash.jpg");

            var numOfPixels = bmp.Width * bmp.Height;
            Console.WriteLine("Number of pixels: " + numOfPixels);

            bmp = ChangePixels(bmp);
            
            bmp.Save("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\1\\assets\\grey.jpg", ImageFormat.Jpeg);
            bmp.Save("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\1\\assets\\grey.png", ImageFormat.Png);
            bmp.Save("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\1\\assets\\grey.tiff", ImageFormat.Tiff);
            bmp.Save("C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\1\\assets\\grey.bmp", ImageFormat.Bmp);
            Console.WriteLine("Images saved");

            var imageTypes = new string[] { "jpg", "png", "tiff", "bmp" };

            foreach (var type in imageTypes)
            {
                Console.WriteLine($"\nTYPE: {type}");
                bmp = new Bitmap($"C:\\Users\\Vitalii\\OneDrive\\University\\3 семестр\\Теорія Інформації\\1\\assets\\grey.{type}");

                var pixelsMap = new Dictionary<byte, long>();
                GroupPixels(bmp, pixelsMap);

                var probability = new Dictionary<byte, double>();
                foreach (KeyValuePair<byte, long> entry in pixelsMap)
                {
                    probability.Add(entry.Key, entry.Value / (double)numOfPixels);
                }

                var sumOfValues = probability.Values.Sum();
                Console.WriteLine("Sum of probabilities: " + sumOfValues);

                var entropy = FindEntropy(probability.Values.ToArray());
                Console.WriteLine("Entropy: " + entropy);
            }
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
    }
}
