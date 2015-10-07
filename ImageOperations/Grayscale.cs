using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace INFOIBV.ImageOperations
{
    class Grayscale
    {
        public int[,] Compute(Bitmap inputImage)
        {
            int[,] image = new int[inputImage.Size.Width, inputImage.Size.Height];
            for (int x = 0; x < inputImage.Size.Width; x++)
            {
                for (int y = 0; y < inputImage.Size.Height; y++)
                {
                    image[x, y] = (int)(inputImage.GetPixel(x, y).GetBrightness() * 255);
                }
            }
            return image;
        }
    }
}
