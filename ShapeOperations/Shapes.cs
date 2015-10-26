using INFOIBV.LineOperations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace INFOIBV.ShapeOperations
{
    public static class Shapes
    {
        public static List<ShapeInfo> Find(int[,] input, Card card)
        {
            List<ShapeInfo> shapes = new List<ShapeInfo>();
            card = Rectangles.Shrink(card, 0.75f, 0.9f);
            int[,] cardContent = Rectangles.CreateMask(input, card);

            int objects = 0;
            for (int y = 0; y < cardContent.GetLength(1); y++)
            {
                for (int x = 0; x < cardContent.GetLength(0); x++)
                {
                    if (cardContent[x, y] > objects)
                    {
                        // We found a new object.
                        objects++;

                        // Calculate properties
                        IList<int> path = Perimeter.WalkPerimeter(cardContent, x, y);
                        double length = Perimeter.ComputeLength(path);
                        double area = Perimeter.ComputeArea(Perimeter.To4Connected(path));
                        double compactness = area / length;
                        int[] bounding = Perimeter.BoundingBox(path, x, y);

                        // Create a new ShapeInfo struct.
                        ShapeInfo info = new ShapeInfo(area, bounding);

                        // Filter invalid shapes.
                        if (length <= 50 || compactness <= 0.6 || length > 500 || info.Ratio > 2.5)
                        {
                            objects--;
                            Perimeter.Colour(cardContent, x, y, 0);
                            continue;
                        }

                        // Colour the shape.
                        Perimeter.Colour(cardContent, x, y, objects);

                        shapes.Add(info);
                    }
                }
            }


            return shapes;
        }
      
        public static Suit ClassifyShapes(IList<ShapeInfo> shapes, Card card, out double avgSolidity)
        {
            List<double> solidities = new List<double>();
            avgSolidity = 0;
            if (shapes.Count == 0)
                return Suit.Unknown;

            int count = 0;
            foreach (ShapeInfo shape in shapes)
            {
                // Classify the shape.
                double area = shape.Area;

                double deltaY = card.bottomRight.Y - card.bottomLeft.Y;
                double deltaX = card.bottomRight.X - card.bottomLeft.X;
                double angle = Math.Atan2(deltaY, deltaX);

                int centerX = shape.Center.X;
                int centerY = shape.Center.Y;
                Point tl = new Point(shape.X - centerX, shape.Y - centerY);
                Point tr = new Point(shape.X + shape.Width - centerX, shape.Y - centerY);
                Point bl = new Point(shape.X - centerX, shape.Y + shape.Height - centerY);
                Point br = new Point(shape.X + shape.Width - centerX, shape.Y + shape.Height - centerY);

                double tlX = tl.X * Math.Cos(angle) - tl.Y * Math.Sin(angle) + centerX;
                double tlY = tl.X * Math.Sin(angle) + tl.Y * Math.Cos(angle) + centerY;
                double trX = tr.X * Math.Cos(angle) - tr.Y * Math.Sin(angle) + centerX;
                double trY = tr.X * Math.Sin(angle) + tr.Y * Math.Cos(angle) + centerY;
                double blX = bl.X * Math.Cos(angle) - bl.Y * Math.Sin(angle) + centerX;
                double blY = bl.X * Math.Sin(angle) + bl.Y * Math.Cos(angle) + centerY;
                double brX = br.X * Math.Cos(angle) - br.Y * Math.Sin(angle) + centerX;
                double brY = br.X * Math.Sin(angle) + br.Y * Math.Cos(angle) + centerY;
                int minX = (int)Enumerable.Min(new double[] { tlX, trX, blX, brX });
                int minY = (int)Enumerable.Min(new double[] { tlY, trY, blY, brY });
                int maxX = (int)Enumerable.Max(new double[] { tlX, trX, blX, brX });
                int maxY = (int)Enumerable.Max(new double[] { tlY, trY, blY, brY });

                int width = maxX - minX;
                int height = maxY - minY;

                double solidity = (width * height) / area;
                if (solidity >= 2)
                {
                    // Something very wrong.
                    continue;
                }
                count++;
                solidities.Add(solidity);
            }
            if(solidities.Count == 0)
                return Suit.Unknown;
            solidities.Sort();
            avgSolidity = solidities.Count % 2 == 0 ? (solidities[solidities.Count / 2] + solidities[solidities.Count / 2 - 1]) / 2 : solidities[solidities.Count / 2];
            Console.WriteLine("Solidity: {0}", avgSolidity);

            if (avgSolidity >= 1.70)
            {
                return Suit.Diamonds;
            }
            if (avgSolidity >= 1.57)
            {
                return Suit.Clubs;
            }
            if (avgSolidity >= 1.45)
            {
                return Suit.Spades;
            }
            if (avgSolidity >= 1.30)
            {
                return Suit.Hearts;
            }
            return Suit.Unknown;
        }

    }
    public enum Suit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs,
        Unknown
    }
}
