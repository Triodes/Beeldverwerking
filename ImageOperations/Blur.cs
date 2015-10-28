using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    static class Blur
    {
        public static int[,] Compute(int[,] input)
        {
            // Apply a non normalized blur kernel to the given image.
            int[,] horizontal = Defaults.ApplyKernel(input, new float[,] 
            {
                {0.25f, 0.50f, 0.25f},
                {0.50f, 1.00f, 0.50f},
                {0.25f, 0.50f, 0.25f}
            });

            return horizontal;
        }
    }
}
