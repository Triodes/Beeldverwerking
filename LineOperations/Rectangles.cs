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
        public readonly double ratio;

        public Card(Point topLeft, Point topRight, Point bottomRight, Point bottomLeft, double ratio) 
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
            this.ratio = ratio;
        }

        public void Draw(Graphics g, Pen pen)
        {
            g.DrawLine(pen, topLeft, topRight);
            g.DrawLine(pen, topRight, bottomRight);
            g.DrawLine(pen, bottomRight, bottomLeft);
            g.DrawLine(pen, bottomLeft, topLeft);
        }

        public void Draw(Graphics g)
        {
            Draw(g, Pens.Cyan);
        }
    }

    public static class Rectangles 
    {


        public static int[,] CreateMask(int[,] image, Card card)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];

            Parallel.For(0, width * height, i => 
            {
                int x = i % width;
                int y = i / width;

                bool inside = InsideTriangle(card.topLeft, card.topRight, card.bottomLeft, x, y) 
                    || InsideTriangle(card.topRight, card.bottomRight, card.bottomLeft, x, y);
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


        public static Card Shrink(Card card, float factorX, float factorY) 
        {
            // Determine middle point.
            int centerX = (card.bottomLeft.X + card.bottomRight.X + card.topLeft.X + card.topRight.X) / 4;
            int centerY = (card.bottomLeft.Y + card.bottomRight.Y + card.topLeft.Y + card.topRight.Y) / 4;

            Point topLeft = new Point(
                (int)((card.topLeft.X - centerX) * factorX + centerX), 
                (int)((card.topLeft.Y - centerY) * factorY + centerY)
            );

            Point topRight = new Point(
                (int)((card.topRight.X - centerX) * factorX + centerX),
                (int)((card.topRight.Y - centerY) * factorY + centerY)
            );

            Point bottomRight = new Point(
                (int)((card.bottomRight.X - centerX) * factorX + centerX),
                (int)((card.bottomRight.Y - centerY) * factorY + centerY)
            );

            Point bottomLeft = new Point(
                (int)((card.bottomLeft.X - centerX) * factorX + centerX),
                (int)((card.bottomLeft.Y - centerY) * factorY + centerY)
            );

            return new Card(topLeft, topRight, bottomRight, bottomLeft, card.ratio);
        }
    }
}

