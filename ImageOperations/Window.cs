using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    static class Window
    {
        // Compute the window.
        public static int[,] Compute(int[,] image, int lower, int upper)
        {
            return Defaults.Compute(image, input =>
            {
                return input < lower || input >= upper ? 0 : input;
            });
        }
    }
}
