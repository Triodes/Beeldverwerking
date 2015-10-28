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
    static class Grayscale
    {
        
        public static int[,] FromBitmap(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;

            // Get the bitmapdata from the bitmap and lock the bitmap.
            BitmapData bitmapData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, input.PixelFormat);

            // Get the amount of bytes per pixel
            int bytesPerPixel = Bitmap.GetPixelFormatSize(input.PixelFormat) / 8;

            // Get the total amount of bytes in the image
            int byteCount = bitmapData.Stride * input.Height;

            //Create a new array to contain the pixel data
            byte[] pixels = new byte[byteCount];

            // Get the start pointer
            IntPtr ptrFirstPixel = bitmapData.Scan0;

            //Copy the pixel data to the array
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            // Unlock the bitmap.
            input.UnlockBits(bitmapData);

            // Get the number of scan lines and the width of a scan line.
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            int[,] result = new int[width, height];

            // Copy the pixel data into an int array and do grayscale conversion.
            for (int y = 0; y < heightInPixels; y++)
            {
                // Calculate the current scanline.
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    // Sum the color channels.
                    int temp = 0;
                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        temp += pixels[currentLine + x + i];
                    }

                    // Put the average of the color channels into the current pixel spot of the output.
                    result[x/bytesPerPixel, y] = temp / bytesPerPixel;
                }
            }

            return result;
        }

        public static Bitmap ToBitmap(int[,] input)
        {
            int width = input.GetLength(0);
            int height = input.GetLength(1);

            // Create a new bitmap
            Bitmap output = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Get the bitmapdata from the bitmap and lock the bitmap.
            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bitmapData =
                output.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                output.PixelFormat);

            // Get the amount of bytes per pixel.
            int bytesPerPixel = Bitmap.GetPixelFormatSize(output.PixelFormat) / 8;

            // Get the total amount of bytes.
            int byteCount = bitmapData.Stride * height;

            // Create an array to hold the pixel data.
            byte[] pixels = new byte[byteCount];

            // Get the number of scan lines and the width of a scan line.
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            // Copy the image to the pixel array.
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

            // Copy the pixel data to the bitmap.
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmapData.Scan0, byteCount);

            // Unlock the bitmap.
            output.UnlockBits(bitmapData);

            return output;
        }
    }
}
