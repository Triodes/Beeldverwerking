using System;
using System.Collections.Generic;
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

        public int CompareTo(Line obj) {
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

    public class LineRhoComparer : IComparer<Line>
    {
        public int Compare(Line a, Line b) {
            if(a.rho != b.rho)
                return a.rho.CompareTo(b.rho);
            return a.value.CompareTo(b.value);
        }
    }

    public static class Lines 
    {
        private const double BUCKET_SIZE = 5 * Math.PI / 180.0;

        public static SortedSet<Line> FindLines(int[,] image, double stepSize, int maxVal)
        {
            SortedSet<Line> result = new SortedSet<Line>();

            int width = image.GetLength(0);
            int height = image.GetLength(1);

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
                    handleBucket(bucket, result);

                    // Prepare for the other bucket.
                    currentSize = 0;
                    currentAngle = -1;
                    bucket = new SortedSet<Line>(comparer);
                }
            }
            // TODO: Handle the last bucket, this will possibly contain duplicates with the first bucket.
            handleBucket(bucket, result);

            return result;
        }

        public static SortedSet<Line> FindRectangle(SortedSet<Line> input)
        {
            SortedSet<Line> horizontals = new SortedSet<Line>();
            SortedSet<Line> verticals = new SortedSet<Line>();
            foreach (Line line in input)
            {
                if (line.theta <= 0.15 || Math.Abs(line.theta - Math.PI) <= 0.15)
                    verticals.Add(line);
                if (Math.Abs(line.theta - Math.PI / 2) <= 0.075 && Math.Abs(line.rho) > 25)
                    horizontals.Add(line);
            }
            SortedSet<Line> toRemove = new SortedSet<Line>();
            foreach (Line line1 in verticals)
            {
                foreach (Line line2 in verticals)
                {
                    if (line1.Equals(line2))
                        continue;
                    double d = Math.Abs(Math.Abs(line2.rho) - Math.Abs(line1.rho));
                    if (d < 10)
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



            SortedSet<Line> result = new SortedSet<Line>();

            foreach (Line v1 in verticals)
            {
                foreach (Line v2 in verticals)
                {
                    if (v1.Equals(v2))
                        continue;

                    foreach (Line h1 in horizontals)
                    {
                        foreach (Line h2 in horizontals)
                        {
                            if (h1.Equals(h2))
                                continue;

                            double dv = Math.Abs(Math.Abs(v1.rho) - Math.Abs(v2.rho));
                            double dh = Math.Abs(Math.Abs(h1.rho) - Math.Abs(h2.rho));
                            double ratio = dv / dh;
                            Console.WriteLine("Ratio: " + ratio);
                            if (ratio > 0.52 && ratio < 0.60)
                            {
                                result.Add(v1);
                                result.Add(v2);
                                result.Add(h1);
                                result.Add(h2);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static void handleBucket(SortedSet<Line> bucket, SortedSet<Line> result)
        {
            // Loop over the lines and remove close lines.
            double currentRho = -1;
            Line? currentBest = null;
            foreach(Line parallelLine in bucket) 
            {
                if(currentRho != -1 && Math.Abs(currentRho - parallelLine.rho) >= 15) 
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

