using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    class Window
    {
        int lower, upper;

        public Window(int lower, int upper)
        {
            this.lower = lower;
            this.upper = upper;
        }


        public int[,] Compute(int[,] image)
        {
            return Defaults.Compute(image, ComputeThreshold);
        }

        private int ComputeThreshold(int input)
        {
            return input < lower || input >= upper ? 0 : input;
        }
    }
}
