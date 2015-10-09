using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    class Sobel
    {
        public int[,] Compute(int[,] input)
        {
            int[,] horizontal = Defaults.ApplyKernel(input, new float[,] 
            {
                {-1, -2, -1},
                {0,   0,  0},
                {1,   2,  1}
            });
            int[,] vertical = Defaults.ApplyKernel(input, new float[,] 
            {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            });

            int[,] result = Defaults.Combine(horizontal, vertical, (x, y) =>
            {
                return (int)Math.Round(Math.Sqrt(x*x+y*y));
            });
            return result;
        }
    }
}
