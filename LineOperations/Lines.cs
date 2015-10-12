using System;
using System.Collections.Generic;
using System.Linq;

namespace INFOIBV.LineOperations 
{

    public struct Line : IComparable
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

        public int CompareTo(object obj) {
            if(obj.GetType() != typeof(Line)) 
                return 0;
            
            Line other = (Line)obj;
            if(theta != other.theta)
                return theta.CompareTo(other.theta);
            if(rho != other.rho)
                return rho.CompareTo(other.rho);
            return value.CompareTo(other.value);
        }
    }

    public class LineRhoComparer : IComparer<Line>
    {
        public int Compare(Line a, Line b) {
            if(a.rho != b.rho)
                return a.rho.CompareTo(b.rho);
            return a.CompareTo(b.value);
        }
    }

    public static class Lines 
    {

        public static SortedSet<Line> findLines(int[,] image, double stepSize, int maxVal)
        {
            SortedSet<Line> result = new SortedSet<Line>();

            int width = image.GetLength(0);
            int height = image.GetLength(1);

            // Loop over the hough image.
            for (int step = 0; step < width; step++)
            {
                for (int r = 0; r < height; r++)
                {
                    int value = image[step, r];
                    if(value == 0)
                        continue;

                    // Recompute the theta and rho from the pixel location.
                    double theta = ((step * stepSize) * Math.PI/180.0);
                    double rho = r - maxVal;
                    //Console.WriteLine("Theta: {0}, rho: {1}, value: {2}", theta, rho, value);
                    result.Add(new Line(theta, rho, value));
                }
            }

            return result;
        }

        public static SortedSet<Line> filterLines(SortedSet<Line> lines)
        {
            SortedSet<Line> result = new SortedSet<Line>();

            // Go over the lines, they are ordered on the angle.
            // So group them in 'buckets' of max 5 degrees.
            double bucketSize = 5 * Math.PI / 180.0;
            double currentAngle = -1;
            double currentSize = 0;

            LineRhoComparer comparer = new LineRhoComparer();
            SortedSet<Line> bucket = new SortedSet<Line>(comparer);
            foreach(Line line in lines)
            {
                if(bucket.Count == 0 || currentSize + (line.theta - currentAngle) < bucketSize) 
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
            // Handle the last bucket.
            handleBucket(bucket, result);

            return result;
        }

        private static void handleBucket(SortedSet<Line> bucket, SortedSet<Line> result)
        {
            // Loop over the lines and remove close lines.
            double currentRho = -1;
            foreach(Line parallelLine in bucket) 
            {
                if(currentRho == -1 || Math.Abs(currentRho - parallelLine.rho) >= 15) 
                {
                    result.Add(parallelLine);
                }
                currentRho = parallelLine.rho;
            }
        }



    }
}

