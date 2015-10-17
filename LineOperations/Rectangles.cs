using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace INFOIBV.LineOperations 
{
    public struct Card
    {
        public readonly Point topLeft;
        public readonly Point topRight;
        public readonly Point bottomRight;
        public readonly Point bottomLeft;

        public Card(Point topLeft, Point topRight, Point bottomRight, Point bottomLeft) 
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
        }
    }

    public static class Rectangles 
    {


        public static int[,] CreateMask(int[,] image, Card card)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];

            Parallel.For(0, width * height, i => {
                int x = i % width;
                int y = i / width;

                bool inside = InsideTriangle(card.topLeft, card.topRight, card.bottomLeft, x, y);
                inside |= InsideTriangle(card.topRight, card.bottomRight, card.bottomLeft, x, y);
                if(inside)
                    result[x, y] = image[x,y];
            });

            return result;
        }

        private static bool InsideTriangle(Point a, Point b, Point c, int x, int y)
        {
            double fab = (y - a.Y) * (b.X - a.X) - (x - a.X) * (b.Y - a.Y);
            double fca = (y - c.Y) * (a.X - c.X) - (x - c.X) * (a.Y - c.Y);
            double fbc = (y - b.Y) * (c.X - b.X) - (x - b.X) * (c.Y - b.Y);

            return (fab * fbc > 0 && fbc * fca > 0);
        }

    }
}

