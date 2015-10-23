using System;
using System.Collections.Generic;
using System.Drawing;

namespace INFOIBV.ShapeOperations {

    public class ShapeInfo
    {
        private readonly IList<int> perimeter;
        private readonly int[] bounding;

        public ShapeInfo(IList<int> perimeter, int[] bounding) 
        {
            this.perimeter = perimeter;
            this.bounding = bounding;
        }

        public IList<int> Perimeter
        {
            get { return perimeter; } 
        }

        public int X
        {
            get { return bounding[0]; }
        }

        public int Y
        {
            get { return bounding[1]; }
        }

        public int Width
        {
            get { return bounding[2] - bounding[0]; }
        }

        public int Height
        {
            get { return bounding[3] - bounding[1]; }
        }

        public Point Center
        {
            get { return new Point(bounding[0] + Width / 2, bounding[1] + Height / 2);}
        }

        public double Ratio
        {
            get { return Height > Width ? Height / Width : Width / Height; }
        }
    }


    public static class Perimeter {

        public const int NORTH = 0;
        public const int NORTH_EAST = 1;
        public const int EAST = 2;
        public const int SOUTH_EAST = 3;
        public const int SOUTH = 4;
        public const int SOUTH_WEST = 5;
        public const int WEST = 6;
        public const int NORTH_WEST = 7;

        public static IList<int> WalkPerimeter(int[,] image, int startX, int startY)
        {
            IList<int> result = new List<int>();
            int currentDirection = NORTH_EAST;

            int currX = startX;
            int currY = startY;

            // Walk until we reach the starting position again
            do 
            {
                if(currentDirection % 2 == 0) 
                {
                    currentDirection = (currentDirection + 7) % 8;
                }
                else
                {
                    currentDirection = (currentDirection + 6) % 8;
                }
                int startDirection = currentDirection;

                while(!IsPartOfShape(image, currentDirection, currX, currY))
                {
                    currentDirection = (currentDirection + 1) % 8; 
                    if(currentDirection == startDirection)
                    {
                        // Gone round, meaning this is a isolated pixel.
                        return result;
                    }
                }

                // Append to the path.
                Position(currentDirection, ref currX, ref currY);
                //Console.WriteLine("Going {0} [{1}x{2}] - [{3}x{4}]", currentDirection, currX, currY, startX, startY);
                result.Add(currentDirection);
            } while(currX != startX || currY != startY);

            return result;
        }

        public static double ComputeLength(IList<int> path) 
        {
            double result = 0;
            foreach(int direction in path) 
            {
                if(direction % 2 == 0) 
                {
                    result += 1;
                }
                else 
                {
                    result += Math.Sqrt(2);
                }
            }
            return result;
        }

        public static IList<int> To4Connected(IList<int> path) 
        {
            IList<int> result = new List<int>();

            int x = 0;
            int y = 0;
            int point = NORTH_WEST;

            foreach(int direction in path) {
                switch(direction) {
                    case NORTH:
                        if(point == NORTH_EAST) {
                            result.Add(SOUTH);
                            point = SOUTH_EAST;
                        }
                        if(point == SOUTH_EAST) {
                            result.Add(WEST);
                            point = SOUTH_WEST;
                        }
                        if(point == SOUTH_WEST) {
                            result.Add(NORTH);
                            point = NORTH_WEST;
                        }
                        // Prepare for next point/
                        point = SOUTH_WEST;
                        y -= 1;
                        break;
                    case NORTH_EAST:
                        if(point == SOUTH_EAST) {
                            result.Add(WEST);
                            point = SOUTH_WEST;
                        }
                        if(point == SOUTH_WEST) {
                            result.Add(NORTH);
                            point = NORTH_WEST;
                        }
                        if(point == NORTH_WEST) {
                            result.Add(EAST);
                            point = NORTH_EAST;
                        }
                        if(point == NORTH_EAST) {
                            result.Add(NORTH);
                        }
                        // Perpare for next point.
                        point = NORTH_WEST;
                        y -= 1;
                        x += 1;
                        break;
                    case EAST:
                        if(point == SOUTH_EAST) {
                            result.Add(WEST);
                            point = SOUTH_WEST;
                        }
                        if(point == SOUTH_WEST) {
                            result.Add(NORTH);
                            point = NORTH_WEST;
                        }
                        if(point == NORTH_WEST) {
                            result.Add(EAST);
                            point = NORTH_EAST;
                        }
                        // Perpare for next point.
                        point = NORTH_WEST;
                        x += 1;
                        break;
                    case SOUTH_EAST:
                        if(point == SOUTH_WEST) {
                            result.Add(NORTH);
                            point = NORTH_WEST;
                        }
                        if(point == NORTH_WEST) {
                            result.Add(EAST);
                            point = NORTH_EAST;
                        }
                        if(point == NORTH_EAST) {
                            result.Add(SOUTH);
                            point = SOUTH_EAST;
                        }
                        // Prepare for next point.
                        point = NORTH_WEST;
                        x += 1;
                        y += 1;
                        break;
                    case SOUTH:
                        if(point == SOUTH_WEST) {
                            result.Add(NORTH);
                            point = NORTH_WEST;
                        }
                        if(point == NORTH_WEST) {
                            result.Add(EAST);
                            point = NORTH_EAST;
                        }
                        if(point == NORTH_EAST) {
                            result.Add(SOUTH);
                            point = SOUTH_EAST;
                        }
                        // Prepare for next point.
                        point = NORTH_EAST;
                        y += 1;
                        break;
                    case SOUTH_WEST:
                        if(point == NORTH_WEST) {
                            result.Add(EAST);
                            point = NORTH_EAST;
                        }
                        if(point == NORTH_EAST) {
                            result.Add(SOUTH);
                            point = SOUTH_EAST;
                        }
                        if(point == SOUTH_EAST) {
                            result.Add(WEST);
                            point = SOUTH_WEST;
                        }
                        // Perpare for next point.
                        point = NORTH_EAST;
                        x -= 1;
                        y += 1;

                        break;
                    case WEST:
                        if(point == NORTH_WEST) {
                            result.Add(EAST);
                            point = NORTH_EAST;
                        }
                        if(point == NORTH_EAST) {
                            result.Add(SOUTH);
                            point = SOUTH_EAST;
                        }
                        if(point == SOUTH_EAST) {
                            result.Add(WEST);
                            point = SOUTH_WEST;
                        }
                        // Prepare for next point.
                        point = SOUTH_EAST;
                        x -= 1;

                        break;
                    case NORTH_WEST:
                        if(point == NORTH_EAST) {
                            result.Add(SOUTH);
                            point = SOUTH_EAST;
                        }
                        if(point == SOUTH_EAST) {
                            result.Add(WEST);
                            point = SOUTH_WEST;
                        }
                        if(point == SOUTH_WEST) {
                            result.Add(NORTH);
                            point = NORTH_WEST;
                        }
                        // Prepare for next point.
                        point = SOUTH_EAST;
                        x -= 1;
                        y -= 1;
                        break;
                }
            }

            return result;
        }

        public static double ComputeArea(IList<int> path)
        {
            double area = 0;
            int yLevel = 0;

            foreach(int direction in path) 
            {
                //area += 0.5;//(direction % 2 == 0 ? 1 : 0.5);

                switch(direction) 
                {
                    case NORTH:
                        yLevel--;
                        break;
                    case NORTH_EAST:
                        break;
                    case EAST:
                        area -= yLevel;
                        break;
                    case SOUTH_EAST:
                        area -= yLevel;
                        break;
                    case SOUTH:
                        yLevel++;
                        break;
                    case SOUTH_WEST:
                        break;
                    case WEST:
                        area += yLevel;
                        break;
                    case NORTH_WEST:
                        break;
                }
            }
            return area;
        }

        public static int[,] FillArea(int[,] image, IList<int> path, int x, int y)
        {
            double area = 1;
            int yLevel = 0;

            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];

            result[x, y] = 512;
            foreach(int direction in path) 
            {
                Position(direction, ref x, ref y);
                switch(direction) {
                case NORTH:
                    area += 1;
                    yLevel--;
                    break;
                case NORTH_EAST:
                    yLevel--;
                    area -= yLevel;
                    for(int i = 0; i <= y; i++) {
                        if(result[x - 1, y - i] == 512)
                            break;
                        result[x - 1, y - i] = 0;
                    }
                    for(int i = 0; i <= y; i++) {
                        if(result[x, y - i] == 512)
                            break;
                        result[x, y - i] = 0;
                    }
                    break;
                case EAST:
                    area += 1;
                    area -= yLevel;
                    for(int i = 0; i <= y; i++) {
                        if(result[x, y - i] == 512)
                            break;
                        result[x, y - i] = 0;
                    }
                    break;
                case SOUTH_EAST:
                    area -= yLevel;
                    yLevel++;
                    for(int i = 0; i <= y; i++) {
                        if(result[x, y - i] == 512)
                            break;
                        result[x, y - i] = 0;
                    }
                    break;
                case SOUTH:
                    yLevel++;
                    break;
                case SOUTH_WEST:
                    yLevel++;
                    area += yLevel;
                    for(int i = 1; i <= y; i++) {
                        if(result[x, y - i] == 512)
                            break;
                        result[x, y - i] = 255;
                    }
                    break;
                case WEST:
                    area += yLevel;
                    for(int i = 1; i <= y; i++) {
                        if(result[x, y - i] == 512)
                            break;
                        result[x, y - i] = 255;
                    }
                    break;
                case NORTH_WEST:
                    area += yLevel;
                    yLevel--;
                    for(int i = 0; i <= y; i++) {
                        if(result[x + 1, y - i] == 512)
                            break;
                        result[x + 1, y - i] = 255;
                    }
                    for(int i = 1; i <= y; i++) {
                        if(result[x, y - i] == 512)
                            break;
                        result[x, y - i] = 255;
                    }
                    break;
                }

                result[x, y] = 512;
            }
            return result;
        }

        public static int[] BoundingBox(IList<int> path, int x, int y) 
        {
            int minX = x;
            int minY = y;
            int maxX = x;
            int maxY = y;

            foreach(int direction in path) 
            {
                Position(direction, ref x, ref y);
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }

            return new int[]{minX, minY, maxX, maxY};
        }

        public static double Compare(int[,] image, ShapeInfo info, int[,] reference) 
        {
            // Iterate over the image.
            int result = 0;

            int referenceWidth = reference.GetLength(0);
            int referenceHeight = reference.GetLength(1);
            double scaleX = info.Width / (double)referenceWidth;
            double scaleY = info.Height / (double)referenceHeight;
            for(int x = info.X; x <= info.X + info.Width; x++) 
            {
                for(int y = info.Y; y <= info.Y + info.Height; y++) 
                {
                    int refX = Math.Min((int)((x - info.X) / scaleX), referenceWidth - 1);
                    if(refX < 0) refX = 0;
                    int refY = Math.Min((int)((y - info.Y) / scaleY), referenceHeight - 1);
                    if(refY < 0) refY = 0;

                    if(image[x, y] > 0)
                        result += reference[refX, refY] > 0 ? 1 : 0;
                    else
                        result += reference[refX, refY] > 0 ? 0 : 1;
                }
            }

            return (double)result / ((info.Width + 1) * (info.Height + 1));
        }

        public static void remove(ref int[,] image, int x, int y, int value)
        {
            if(image[x, y] <= value)
                return;
            image[x, y] = value;
            remove(ref image, x - 1, y - 1, value);
            remove(ref image, x, y - 1, value);
            remove(ref image, x + 1, y - 1, value);
            remove(ref image, x - 1, y, value);
            remove(ref image, x + 1, y, value);
            remove(ref image, x - 1, y + 1, value);
            remove(ref image, x, y + 1, value);
            remove(ref image, x + 1, y + 1, value);
        }


        public static void Position(int direction, ref int x, ref int y) 
        {
            switch(direction) 
            {
            case NORTH:
                y -= 1;
                break;
            case NORTH_EAST:
                y -= 1;
                x += 1;
                break;
            case EAST:
                x += 1;
                break;
            case SOUTH_EAST:
                y += 1;
                x += 1;
                break;
            case SOUTH:
                y += 1;
                break;
            case SOUTH_WEST:
                y += 1;
                x -= 1;
                break;
            case WEST:
                x -= 1;
                break;
            case NORTH_WEST:
                y -= 1;
                x -= 1;
                break;
            }
        }

        public static bool IsPartOfShape(int[,] image, int direction, int x, int y)
        {
            Position(direction, ref x, ref y);          

            // FIXME: Check bounds
            return image[x, y] > 0;
        }

    }
}

