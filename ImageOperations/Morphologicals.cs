using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.ImageOperations
{
    static class Morphologicals
    {
        private static int[,] ErosionDilation(int[,] image, bool[,] s, bool erosion)
        {
            // Create a result image the size of the input image.
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];
            // Determine the size and center of the kernel
            int kwidth = s.GetLength(0);
            int kheight = s.GetLength(1);
            int hw = kwidth / 2;
            int hh = kheight / 2;

            // Loop over the center pixels for the kernel.
            Parallel.For(0, width * height, i =>
            {
                int x = i % width;
                int y = i / width;
                // Variable holding the new value in the result.
                int newValue = erosion ? int.MaxValue : int.MinValue;

                // Go over the kernel
                for (int kx = -hw; kx <= hw; kx++)
                {
                    for (int ky = -hh; ky <= hh; ky++)
                    {
                        if (!s[kx + hw, ky + hh])
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

                // Clamp the result to ensure valid gray values.
                result[x, y] = Math.Min(Math.Max(newValue, 0), 255);

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
