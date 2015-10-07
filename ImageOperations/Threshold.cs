using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    class Threshold
    {
        int threshold; 
        bool invert;


        public Threshold(int threshold, bool invert = false)
        {
            this.threshold = threshold;
            this.invert = invert;
        }


        public int[,] Compute(int[,] image)
        {
            return Defaults.Compute(image, ComputeThreshold);
        }

        private int ComputeThreshold(int input)
        {
            if (invert)
            {
                return input >= threshold ? 0 : Defaults.MAX_VALUE;
            }
            return input >= threshold ? Defaults.MAX_VALUE : 0;

        }
    }
}
