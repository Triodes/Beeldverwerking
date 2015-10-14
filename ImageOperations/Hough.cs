using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.ImageOperations
{
    class Hough
    {
        public int[,] Compute(int[,] image, double stepSize)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int maxVal = (int)Math.Ceiling(Math.Sqrt(width * width + height * height));

            int[,] result = new int[(int)(180 / stepSize), maxVal * 2 + 1];

            Parallel.For(0, (int)(180 / stepSize), step =>
            {
                double theta = (step * stepSize) * Math.PI / 180.0;

                double sin = Math.Sin(theta);
                double cos = Math.Cos(theta);

                Random random = null;
                bool useRandom = false;
                if (step * stepSize == 45 || step * stepSize == 135)
                {
                    random = new Random(460437102);
                    useRandom = true;
                }


                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int value = image[x, y];
                        if (value <= 0)
                            continue;

                        int r;
                        double temp = x * cos + y * sin;

                        if (useRandom)
                        {
                            temp += random.NextDouble() - 0.5;
                        }
                        
                        r = (int)Math.Round(temp);
                        r += maxVal;

                        result[step, r] += value;
                    }
                }
            });
            return result;
        }
    }
}
