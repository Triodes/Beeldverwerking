using INFOIBV.CardOperations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace INFOIBV.LineOperations 
{

    public struct Line : IComparable<Line>
    {
        public readonly double theta;
        public readonly double rho;
        public readonly int value;

        public Line(double theta, double rho, int value)
        {
            this.theta = theta;
            this.rho = rho;
            this.value = value;
        }

        // Orders the lines in non decreasing order using theta. Rho is used when theta's are equal. Value is used when theta's and rho's are equal.
        public int CompareTo(Line obj) 
        {
            if(obj.GetType() != typeof(Line)) 
                return 0;
            
            Line other = (Line)obj;
            if(theta != other.theta)
                return theta.CompareTo(other.theta);
            if(rho != other.rho)
                return rho.CompareTo(other.rho);
            return value.CompareTo(other.value);
        }

        public override string ToString()
        {
            return String.Format("Theta: {0}, Rho: {1}, Value: {2}", theta, rho, value);
        }
    }

    // Sorts the lines in non decreasing order using rho. Value is used if rho's are equal.
    public class LineRhoComparer : IComparer<Line>
    {
        public int Compare(Line a, Line b)
        {
            if (a.rho != b.rho)
                return a.rho.CompareTo(b.rho);
            return a.value.CompareTo(b.value);
        }
    }

    public static class Lines 
    {
        private const double BUCKET_SIZE = 5 * Math.PI / 180.0;

        // Creates a set of lines for all non zero values in the hough transform.
        public static SortedSet<Line> FindLines(int[,] image, double stepSize)
        {
            SortedSet<Line> result = new SortedSet<Line>();

            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int maxVal = (height - 1) / 2;

            // Loop over the hough image.
            for (int step = 0; step < width; step++)
            {
                // Compute the theta from the current step
                double theta = ((step * stepSize) * Math.PI/180.0);

                // Go over the y-axis of the Hough image (rho).
                for (int r = 0; r < height; r++)
                {
                    int value = image[step, r];
                    if(value == 0)
                        continue;

                    // Recompute the rho from the pixel location.
                    double rho = r - maxVal;
                    result.Add(new Line(theta, rho, value));
                }
            }

            return result;
        }

        // Attempts to find loacal maxima in the set of lines.
        public static SortedSet<Line> FilterLines(SortedSet<Line> lines)
        {
            SortedSet<Line> result = new SortedSet<Line>();

            // Go over the lines, they are ordered on the angle.
            // So group them in 'buckets' of max 5 degrees.
            double currentAngle = -1;
            double currentSize = 0;

            LineRhoComparer comparer = new LineRhoComparer();
            SortedSet<Line> bucket = new SortedSet<Line>(comparer);
            foreach(Line line in lines)
            {
                if(bucket.Count == 0 || currentSize + (line.theta - currentAngle) < BUCKET_SIZE) 
                {
                    // Add the line to the bucket.
                    bucket.Add(line);
                    currentAngle = line.theta;
                }
                else
                {
                    // Handle the bucket.
                    HandleBucket(bucket, result);

                    // Prepare for the other bucket.
                    currentSize = 0;
                    currentAngle = -1;
                    bucket = new SortedSet<Line>(comparer);
                }
            }
            // TODO: Handle the last bucket, this will possibly contain duplicates with the first bucket.
            HandleBucket(bucket, result);

            return result;
        }

        public static IList<Card> FindCardShapedRectangles(SortedSet<Line> lines, int width, int height)
        {
            // Extract the horizontal and vertical lines from the set of lines
            IList<Line> horizontals = new List<Line>();
            IList<Line> verticals = new List<Line>();
            foreach (Line line in lines)
            {
                if (line.theta <= 0.15 || Math.Abs(line.theta - Math.PI) <= 0.15)
                    verticals.Add(line);
                if (Math.Abs(line.theta - Math.PI / 2) <= 0.075)
                    horizontals.Add(line);
            }

            // Remove duplicate lines caused by the edges of the hough space, namely around 0 and PI.
            SortedSet<Line> toRemove = new SortedSet<Line>();
            for(int v1 = 0; v1 < verticals.Count; v1++) 
            {
                for(int v2 = v1 + 1; v2 < verticals.Count; v2++) 
                {
                    Line line1 = verticals[v1];
                    Line line2 = verticals[v2];

                    double d = Math.Abs(Math.Abs(line2.rho) - Math.Abs(line1.rho));
                    if (d < 15)
                    {
                        if (line1.value > line2.value)
                            toRemove.Add(line2);
                        else if (line1.value < line2.value)
                            toRemove.Add(line1);
                        else
                            toRemove.Add(line1.theta < line2.theta ? line1 : line2);
                    }
                }
            }
            foreach (Line item in toRemove)
            {
                verticals.Remove(item);
            }

            // Find the rectangles in the sets of lines
            IList<Card> result = new List<Card>();
            for(int v1 = 0; v1 < verticals.Count; v1++) 
            {
                for(int v2 = v1 + 1; v2 < verticals.Count; v2++) 
                {
                    Line lineV1 = verticals[v1];
                    Line lineV2 = verticals[v2];

                    // Loop over the horizontal lines.
                    for(int h1 = 0; h1 < horizontals.Count; h1++) 
                    {
                        for(int h2 = h1 + 1; h2 < horizontals.Count; h2++) 
                        {
                            Line lineH1 = horizontals[h1];
                            Line lineH2 = horizontals[h2];

                            // Compute intersection points.
                            Point a = Intersection(lineV1, lineH1);
                            Point b = Intersection(lineV1, lineH2);
                            Point c = Intersection(lineV2, lineH1);
                            Point d = Intersection(lineV2, lineH2);

                            // Check if intersections are withing the bounds of the image.
                            if (!IsValidPoint(a, width, height) || !IsValidPoint(b, width, height) || !IsValidPoint(c, width, height) || !IsValidPoint(d, width, height))
                                continue;

                            // Compute the lengths of the line segments.
                            double height1 = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
                            double height2 = Math.Sqrt((c.X - d.X) * (c.X - d.X) + (c.Y - d.Y) * (c.Y - d.Y));
                            double width1 = Math.Sqrt((a.X - c.X) * (a.X - c.X) + (a.Y - c.Y) * (a.Y - c.Y));
                            double width2 = Math.Sqrt((b.X - d.X) * (b.X - d.X) + (b.Y - d.Y) * (b.Y - d.Y));

                            // Approximate the height/width ratio.
                            double ratio = ((height1 + height2)/2) / ((width1 + width2)/2);

                            if (Math.Abs(ratio - 1.43) <= 0.15)
                            {
                                Console.WriteLine("ratio: " + ratio);
                                Card card = OrderLines(a, b, c, d);

                                result.Add(card);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static Card OrderLines(Point a, Point b, Point c, Point d)
        {
            if (a.Y < b.Y)
            {
                // Line H1 is the upper line.
                if (a.X < c.X)
                {
                    // Line V1 is the left line.
                    return new Card(a, c, d, b);
                }
                else
                {
                    // Line V2 is the left line.
                    return new Card(c, a, b, d);
                }
            }
            else
            {
                // Line H2 is the upper line.
                if (a.X < c.X)
                {
                    // Line V1 is the left line.
                    return new Card(b, d, c, a);
                }
                else
                {
                    // Line V2 is the left line.
                    return new Card(d, b, a, c);
                }
            }
        }

        public static IList<Card> FilterCardShapedRectangles(int[,] image, IList<Card> cards)
        {
            List<Card> result = new List<Card>();
            foreach (Card card in cards)
            {
                if (HasSupport(image, card))
                    result.Add(card);
            }
            return result;
        }

        private static bool HasSupport(int[,] image, Card card)
        {
            // Calculate the support for the separate line segments of the card.
            double threshold = 0.80;
            double suppH1 = ComputeLineSegmentSupport(image, card.topLeft, card.topRight);
            double suppH2 = ComputeLineSegmentSupport(image, card.bottomLeft, card.bottomRight);
            double suppV1 = ComputeLineSegmentSupport(image, card.topLeft, card.bottomLeft);
            double suppV2 = ComputeLineSegmentSupport(image, card.topRight, card.bottomRight);
            return suppH1 > threshold && suppH2 > threshold && suppV1 > threshold && suppV2 > threshold;
        }

        private static double ComputeLineSegmentSupport(int[,] image, Point a, Point b)
        {
            // Calculate the segment length.
            int length = (int)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

            // Calculate the stepsize.
            double stepSize = 1.0 / length;

            int support = 0;

            // Sum the support for this line.
            for (int i = 0; i < length; i++)
            {
                Point current = Lerp(a,b, i*stepSize);
                support += image[current.X, current.Y] > 0 ? 1 : 0;
            }

            // Average the support over the length of the line.
            return (double)support / (double)length;
        }

        private static Point Lerp(Point a, Point b, double alpha)
        {
            return new Point((int)(a.X * (1 - alpha) + b.X * alpha), (int)(a.Y * (1 - alpha) + b.Y * alpha));
        }

        private static Point Intersection(Line one, Line two) 
        {
            // Calculates the intersection of 2 lines. Returns null if intersection is out
            double a = Math.Cos(one.theta);
            double b = Math.Sin(one.theta);
            double c = Math.Cos(two.theta);
            double d = Math.Sin(two.theta);

            double det = a * d - b * c;
            if(det == 0.0)
                return new Point(0, 0);

            int x = (int)((d * one.rho - b * two.rho) / det);
            int y = (int)((-c * one.rho + a * two.rho) / det);

            return new Point(x, y);
        }

        private static bool IsValidPoint(Point point, int width, int height)
        {
            return point.X >= 0 && point.X < width && point.Y >= 0 && point.Y < height;
        }

        private static void HandleBucket(SortedSet<Line> bucket, SortedSet<Line> result)
        {
            // Loop over the lines and remove close lines.
            double currentRho = -1;
            Line? currentBest = null;
            foreach(Line parallelLine in bucket) 
            {
                if(currentRho != -1 && Math.Abs(currentRho - parallelLine.rho) >= 10) 
                {
                    result.Add(currentBest.Value);
                    currentBest = null;
                }
                if(currentBest == null || parallelLine.value > currentBest.Value.value) 
                {
                    currentBest = parallelLine;
                }
                currentRho = parallelLine.rho;
            }
            // Add trailing line.
            if (currentBest != null)
                result.Add(currentBest.Value);
        }


    }
}

