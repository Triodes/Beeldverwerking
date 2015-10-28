using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.ImageOperations
{
    static class Morphologicals
    {
        private static int[,] ErosionDilation(int[,] image, bool[,] strucElem, bool erosion)
        {
            // Create a result image the size of the input image.
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];

            // Determine the size and center of the kernel
            int kernelWidth = strucElem.GetLength(0);
            int kernelHeight = strucElem.GetLength(1);
            int halfKernelWidth = kernelWidth / 2;
            int halfKernelHeight = kernelHeight / 2;

            // Loop over the center pixels for the kernel.
            Parallel.For(0, width * height, i =>
            {
                int x = i % width;
                int y = i / width;
                // Variable holding the new value in the result.
                int newValue = erosion ? int.MaxValue : int.MinValue;

                // Go over the kernel
                for (int kx = -halfKernelWidth; kx <= halfKernelWidth; kx++)
                {
                    for (int ky = -halfKernelHeight; ky <= halfKernelHeight; ky++)
                    {
                        if (!strucElem[kx + halfKernelWidth, ky + halfKernelHeight])
                            continue;

                        int oldValue;
                        // Check the bounds, outside is black
                        if (x + kx < 0 || x + kx >= width
                            || y + ky < 0 || y + ky >= height)
                            oldValue = 0;
                        else
                            oldValue = image[x + kx, y + ky];

                        newValue = erosion ? Math.Min(oldValue, newValue) : Math.Max(oldValue, newValue);
                    }
                }

                result[x,y] = newValue;
            });

            return result;
        }

        public static int[,] Erosion(int[,] image, bool[,] s)
        {
            return ErosionDilation(image, s, true);
        }

        public static int[,] Dilation(int[,] image, bool[,] s)
        {
            return ErosionDilation(image, s, false);
        }

        public static int[,] Opening(int[,] image, bool[,] s)
        {
            return Dilation(Erosion(image, s), s);
        }

        public static int[,] Closing(int[,] image, bool[,] s)
        {
            return Erosion(Dilation(image, s), s);
        }
    }
}
