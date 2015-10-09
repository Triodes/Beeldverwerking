using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    class Blur
    {
        public int[,] Compute(int[,] input)
        {
            int[,] horizontal = Defaults.ApplyKernel(input, new float[,] 
            {
                {0.0625f, 0.125f, 0.0625f},
                {0.125f,   0.25f,  0.125f},
                {0.0625f,   0.125f,  0.0625f}
            });

            return horizontal;
        }
    }
}
