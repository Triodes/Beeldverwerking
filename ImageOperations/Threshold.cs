using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    static class Threshold
    {
        public static int[,] Compute(int[,] image, int threshold, bool invert = false)
        {
            return Defaults.Compute(image, input =>
            {
                if (invert)
                {
                    return input >= threshold ? 0 : Defaults.MAX_VALUE;
                }
                return input >= threshold ? Defaults.MAX_VALUE : 0;
            });
        }

    }
}
