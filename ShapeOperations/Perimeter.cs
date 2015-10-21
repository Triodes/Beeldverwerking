using System;
using System.Collections.Generic;
using System.Drawing;

namespace INFOIBV.ShapeOperations 
{
    
    public static class Perimeter 
    {

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
            } 
            while(currX != startX || currY != startY);

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

        public static double ComputeArea(IList<int> path)
        {
            double area = 1;
            int yLevel = 0;

            foreach(int direction in path) 
            {
                //area += 1;
                switch(direction) {
                case NORTH:
                    area += 1;
                    yLevel--;
                    break;
                case NORTH_EAST:
                    yLevel--;
                    area -= yLevel;
                    break;
                case EAST:
                    area += 1;
                    area -= yLevel;
                    break;
                case SOUTH_EAST:
                    area -= yLevel;
                    yLevel++;
                    break;
                case SOUTH:
                    yLevel++;
                    break;
                case SOUTH_WEST:
                    yLevel++;
                    area += yLevel;
                    break;
                case WEST:
                    area += yLevel;
                    break;
                case NORTH_WEST:
                    area += yLevel;
                    yLevel--;
                    break;
                }
            }
            return area;
        }

        public static int[] BoundingBox(IList<int> perimeter, int x, int y) 
        {
            int minX = x;
            int minY = y;
            int maxX = x;
            int maxY = y;

            foreach(int direction in perimeter) 
            {
                Position(direction, ref x, ref y);
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }

            return new int[]{minX, minY, maxX, maxY};
        }

        public static void Colour(int[,] image, int x, int y, int value)
        {
            if(image[x, y] <= value)
                return;
            image[x, y] = value;
            Colour(image, x - 1, y - 1, value);
            Colour(image, x, y - 1, value);
            Colour(image, x + 1, y - 1, value);
            Colour(image, x - 1, y, value);
            Colour(image, x + 1, y, value);
            Colour(image, x - 1, y + 1, value);
            Colour(image, x, y + 1, value);
            Colour(image, x + 1, y + 1, value);
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

