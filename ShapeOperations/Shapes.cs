using INFOIBV.LineOperations;
using System;
using System.Collections.Generic;
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
      
        public static Suit ClassifyShapes(IList<ShapeInfo> shapes, out double avgSolidity)
        {
            avgSolidity = 0;
            if (shapes.Count == 0)
                return Suit.Unknown;

            int count = 0;
            foreach (ShapeInfo shape in shapes)
            {
                // Classify the shape.
                double area = shape.Area;
                double solidity = (shape.Width * shape.Height) / area;
                if (solidity >= 2)
                {
                    // Something very wrong.
                    continue;
                }
                count++;
                avgSolidity += solidity;
            }
            avgSolidity /= count;
            Console.WriteLine("Solidity: {0}", avgSolidity);

            if (avgSolidity >= 1.72)
            {
                return Suit.Diamonds;
            }
            if (avgSolidity >= 1.59)
            {
                return Suit.Clubs;
            }
            if (avgSolidity >= 1.42)
            {
                return Suit.Spades;
            }
            if (avgSolidity >= 1.25)
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
