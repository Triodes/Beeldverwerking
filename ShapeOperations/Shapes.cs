using INFOIBV.CardOperations;
using INFOIBV.LineOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV.ShapeOperations
{
    public static class Shapes
    {
        // Search for shapes inside the given card surface
        public static List<ShapeInfo> Find(int[,] input, Card card)
        {
            // List holding the discovered shapes
            List<ShapeInfo> shapes = new List<ShapeInfo>();
            // Slightly shrink the card to cut off the edges and the suit/number in the corners of the card
            card = Cards.Shrink(card, 0.75f, 0.9f);

            // Cut out the actual card content, leaving the outside black
            int[,] cardContent = Cards.MaskCard(input, card);

            // Go over every in pixel in search of objects
            int objects = 0;
            for (int y = 0; y < cardContent.GetLength(1); y++)
            {
                for (int x = 0; x < cardContent.GetLength(0); x++)
                {
                    // Only pixels with a value greater than 'objects' is part of an unkown object
                    if(cardContent[x, y] <= objects)
                        continue;

                    // New object found
                    objects++;

                    // Walk the perimeter of the object and compute various properties of the object
                    IList<int> path = Perimeter.WalkPerimeter(cardContent, x, y);
                    double length = Perimeter.ComputeLength(path);
                    double area = Perimeter.ComputeArea(Perimeter.To4Connected(path));
                    double compactness = area / length;
                    int[] bounding = Perimeter.BoundingBox(path, x, y);

                    // Create a new ShapeInfo describing the shape
                    ShapeInfo info = new ShapeInfo(area, bounding);

                    // Filter invalid shapes, we assume a valid shape has:
                    //  - a perimeter length that is within expected values
                    //  - a relatively high compactness, suit shapes are mostly round
                    //  - a height/width ratio that is relatively balanced, suit shape bounding boxes are mostly square
                    if (length <= 50 || length > 500 || compactness <= 0.6 || info.Ratio > 2.5)
                    {
                        // Remove the object from the count and colour it black
                        objects--;
                        Perimeter.Colour(cardContent, x, y, 0);
                        continue;
                    }

                    // Colour the object with its index as colour
                    Perimeter.Colour(cardContent, x, y, objects);

                    // Add the shape to the result set
                    shapes.Add(info);
                }
            }

            return shapes;
        }
      
        // Determine of which suit the given shapes are
        public static Suit ClassifyShapes(IList<ShapeInfo> shapes)
        {
            List<double> solidities = new List<double>();

            // Ensure there is at least one shape to classify
            if (shapes.Count == 0)
                return Suit.Unknown;

            // Go over the shapes and compute their solidity
            foreach (ShapeInfo shape in shapes)
            {
                // Solidity is the ratio between the axis-aligned bounding box and the area of the shape
                double area = shape.Area;
                double solidity = (shape.Width * shape.Height) / area;
                solidities.Add(solidity);
            }

            // Find the median solidity
            solidities.Sort();
            int count = solidities.Count;
            double meanSolidity = count % 2 == 0 ? (solidities[count / 2] + solidities[count / 2 - 1]) / 2 : solidities[count / 2];

            // Determine the suit based on the expected solidity for each suit
            if (meanSolidity >= 1.70)
                return Suit.Diamonds;
            if (meanSolidity >= 1.57)
                return Suit.Clubs;
            if (meanSolidity >= 1.45)
                return Suit.Spades;
            if (meanSolidity >= 1.30)
                return Suit.Hearts;
            return Suit.Unknown;
        }

    }

    // Enumeration of the various possible suits
    public enum Suit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs,
        Unknown
    }
}
