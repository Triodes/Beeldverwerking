using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    static class Sobel
    {
        public static int[,] Compute(int[,] input)
        {
            // Compute the horizontal Sobel pass.
            int[,] horizontal = Defaults.ApplyKernel(input, new float[,] 
            {
                {-1, -2, -1},
                {0,   0,  0},
                {1,   2,  1}
            });

            // Compute the vertical Sobel pass.
            int[,] vertical = Defaults.ApplyKernel(input, new float[,] 
            {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            });

            // combine the horizontal and vertical pass using the pythagorean theorem.
            int[,] result = Defaults.Combine(horizontal, vertical, (x, y) =>
            {
                return (int)Math.Round(Math.Sqrt(x*x+y*y));
            });
            return result;
        }
    }
}
