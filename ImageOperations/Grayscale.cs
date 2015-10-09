using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.ImageOperations
{
    class Grayscale
    {
        
        public int[,] FromBitmap(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;

            BitmapData bitmapData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, input.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(input.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * input.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            int[,] result = new int[width, height];

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = pixels[currentLine + x];
                    int oldGreen = pixels[currentLine + x + 1];
                    int oldRed = pixels[currentLine + x + 2];

                    result[x/bytesPerPixel, y] = (oldBlue + oldGreen + oldRed) / 3;
                }
            }

            //int[,] result = new int[width, height];

            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        Color val = inputImage.GetPixel(x,y);
            //        result[x, y] = (int)(val.GetBrightness()*255);
            //    }
            //}

            return result;
        }

        public Bitmap ToBitmap(int[,] input)
        {
            int width = input.GetLength(0);
            int height = input.GetLength(1);

            //Bitmap output = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        output.SetPixel(x, y, Color.FromArgb(input[x,y],input[x,y],input[x,y]));
            //    }
            //}

            Bitmap output = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bitmapData =
                output.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                output.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(output.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * height;
            byte[] pixels = new byte[byteCount];
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            int[,] result = new int[width, height];

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    pixels[currentLine + x] = (byte)input[x / bytesPerPixel, y];
                    pixels[currentLine + x + 1] = (byte)input[x / bytesPerPixel, y];
                    pixels[currentLine + x + 2] = (byte)input[x / bytesPerPixel, y];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmapData.Scan0, byteCount);

            output.UnlockBits(bitmapData);

            return output;
        }
    }
}
