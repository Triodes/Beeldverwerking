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

                bool inside = false;
                inside |= InsideTriangle(card.topLeft, card.topRight, card.bottomRight, x, y);
                inside |= InsideTriangle(card.topRight, card.bottomRight, card.bottomLeft, x, y);
                inside |= InsideTriangle(card.bottomRight, card.bottomLeft, card.topLeft, x, y);
                inside |= InsideTriangle(card.bottomLeft, card.topLeft, card.topRight, x, y);
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
            //int centerX = (card.bottomLeft.X + card.bottomRight.X + card.topLeft.X + card.topRight.X) / 4;
            //int centerY = (card.bottomLeft.Y + card.bottomRight.Y + card.topLeft.Y + card.topRight.Y) / 4;

            int topCenterX = (card.topLeft.X + card.topRight.X) / 2;
            int topCenterY = (card.topLeft.Y + card.topRight.Y) / 2;

            int bottomCenterX = (card.bottomLeft.X + card.bottomRight.X) / 2;
            int bottomCenterY = (card.bottomLeft.Y + card.bottomRight.Y) / 2;

            Point topLeft = new Point(
                (int)((card.topLeft.X - topCenterX) * factorX + topCenterX), 
                (int)((card.topLeft.Y - topCenterY) * factorX + topCenterY)
            );

            Point topRight = new Point(
                (int)((card.topRight.X - topCenterX) * factorX + topCenterX),
                (int)((card.topRight.Y - topCenterY) * factorX + topCenterY)
            );

            Point bottomRight = new Point(
                (int)((card.bottomRight.X - bottomCenterX) * factorX + bottomCenterX),
                (int)((card.bottomRight.Y - bottomCenterY) * factorX + bottomCenterY)
            );

            Point bottomLeft = new Point(
                (int)((card.bottomLeft.X - bottomCenterX) * factorX + bottomCenterX),
                (int)((card.bottomLeft.Y - bottomCenterY) * factorX + bottomCenterY)
            );

            return new Card(topLeft, topRight, bottomRight, bottomLeft, card.ratio);
        }
    }
}

