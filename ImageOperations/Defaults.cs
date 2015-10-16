using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.ImageOperations
{
    public static class Defaults
    {
        public const int MAX_VALUE = 255;

        public static int[,] ApplyKernel(int[,] image, float[,] kernel)
        {
            // Create a result image the size of the input image.
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];
            // Determine the size and center of the kernel
            int kWidth = kernel.GetLength(0);
            int kHeight = kernel.GetLength(1);
            int halfKWidth = kWidth / 2;
            int halfKHeight = kHeight / 2;

            // Loop over the center pixels for the kernel.
            Parallel.For(0, width * height, i =>
            {
                int x = i % width;
                int y = i / width;
                // Variable holding the new value in the result.
                int newValue = 0;

                // Go over the kernel
                for (int kx = -halfKWidth; kx <= halfKWidth; kx++)
                {
                    for (int ky = -halfKHeight; ky <= halfKHeight; ky++)
                    {
                        float weight = kernel[kx + halfKWidth, ky + halfKHeight];
                        int oldValue;

                        // Perform not padding, not mirroring, but the other
                        int ix = Clamp(x + kx, 0, width - 1);
                        int iy = Clamp(y + ky, 0, height - 1);
                        oldValue = image[ix, iy];

                        newValue += (int)(weight * oldValue);
                    }

                    result[x, y] = newValue;
                }

            });

            return result;
        }

        public delegate int PixelArithmetic(int x, int y);
        public delegate int PixelArithmeticOne(int x);

        public static int[,] Combine(int[,] one, int[,] two, PixelArithmetic f)
        {
            int[,] result = new int[one.GetLength(0), one.GetLength(1)];
            Parallel.For(0, one.GetLength(0) * one.GetLength(1), i =>
            {
                int x = i % one.GetLength(0);
                int y = i / one.GetLength(0);

                result[x, y] = f(one[x, y], two[x, y]);
            });
            return result;
        }

        public static int[,] Compute(int[,] image, PixelArithmeticOne f)
        {
            int[,] result = new int[image.GetLength(0), image.GetLength(1)];
            Parallel.For(0, image.GetLength(0) * image.GetLength(1), i =>
            {
                int x = i % image.GetLength(0);
                int y = i / image.GetLength(0);

                result[x, y] = f(image[x, y]);
            });
            return result;
        }

        private static void FindBounds(int[,] image, out int lower, out int upper)
        {
            lower = 0;
            upper = 0;

            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    int value = image[x, y];
                    if (value < lower)
                        lower = value;
                    if (value > upper)
                        upper = value;
                }
            }
        }

        public static int[,] Normalize(int[,] image, int max)
        {
            int lower, upper;
            FindBounds(image, out lower, out upper);

            float scale = (float)max / (upper - lower);
            int[,] result = new int[image.GetLength(0), image.GetLength(1)];

            Parallel.For(0, image.GetLength(0) * image.GetLength(1), i =>
            {
                int x = i % image.GetLength(0);
                int y = i / image.GetLength(0);
                int value = image[x, y];
                result[x, y] = (int)((value - lower) * scale);

            });

            return result;
        }

        public static int Clamp(int val, int lower, int upper)
        {
            return Math.Min(Math.Max(val, lower), upper);
        }
    }
}
