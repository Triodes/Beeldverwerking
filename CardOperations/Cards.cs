using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace INFOIBV.CardOperations 
{

    public static class Cards 
    {
        // Keep only the pixels that fall on the card's surface on the image
        public static int[,] MaskCard(int[,] image, Card card)
        {
            // Construct 
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];

            // Go over ech pixel
            Parallel.For(0, width * height, i => 
            {
                int x = i % width;
                int y = i / width;

                // Check if the pixel falls inside the triangles that the card surface is composed of.
                // Note: four triangles are tested to compensate for rounding errors when checking only two complementary triangles
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

        // Checks if the given coordinates fall inside a triangle defined by its three points
        private static bool InsideTriangle(Point a, Point b, Point c, int x, int y)
        {
            // Compute the determinant
            double fab = (y - a.Y) * (b.X - a.X) - (x - a.X) * (b.Y - a.Y);
            double fca = (y - c.Y) * (a.X - c.X) - (x - c.X) * (a.Y - c.Y);
            double fbc = (y - b.Y) * (c.X - b.X) - (x - b.X) * (c.Y - b.Y);

            // Non zero values indicate that the coordinates are on the wrong side of the triangel axes
            return (fab * fbc > 0 && fbc * fca > 0);
        }


        public static Card Shrink(Card card, float factorX, float factorY) 
        {
            // Determine the center point of the card.
            int centerX = (card.bottomLeft.X + card.bottomRight.X + card.topLeft.X + card.topRight.X) / 4;
            int centerY = (card.bottomLeft.Y + card.bottomRight.Y + card.topLeft.Y + card.topRight.Y) / 4;

            // Move the top left corner point inwards
            Point topLeft = new Point(
                (int)((card.topLeft.X - centerX) * factorX + centerX), 
                (int)((card.topLeft.Y - centerY) * factorY + centerY)
            );

            // Move the top right corner point inwards
            Point topRight = new Point(
                (int)((card.topRight.X - centerX) * factorX + centerX),
                (int)((card.topRight.Y - centerY) * factorY + centerY)
            );

            // Move the bottom right corner point inwards
            Point bottomRight = new Point(
                (int)((card.bottomRight.X - centerX) * factorX + centerX),
                (int)((card.bottomRight.Y - centerY) * factorY + centerY)
            );

            // Move the bottom left corner point inwards
            Point bottomLeft = new Point(
                (int)((card.bottomLeft.X - centerX) * factorX + centerX),
                (int)((card.bottomLeft.Y - centerY) * factorY + centerY)
            );

            // Return the resulting card
            return new Card(topLeft, topRight, bottomRight, bottomLeft);
        }
    }
}

