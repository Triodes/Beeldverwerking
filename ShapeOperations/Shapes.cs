using System;
using System.Collections.Generic;

namespace INFOIBV.ShapeOperations {

    public static class Perimeter {

        public const int NORTH = 0;
        public const int NORHT_EAST = 1;
        public const int EAST = 2;
        public const int SOUTH_EAST = 3;
        public const int SOUTH = 4;
        public const int SOUTH_WEST = 5;
        public const int WEST = 6;
        public const int NORTH_WEST = 7;

        public static IList<int> WalkPerimeter(int[,] image, int startX, int startY)
        {
            IList<int> result = new List<int>();
            int currentDirection = NORHT_EAST;

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

                while(!IsPartOfShape(image, currentDirection, currX, currY))
                {
                    currentDirection = (currentDirection + 1) % 8; 
                }

                // Append to the path.
                Position(currentDirection, ref currX, ref currY);
                Console.WriteLine("Going {0} [{1}x{2}] - [{3}x{4}]", currentDirection, currX, currY, startX, startY);
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
                case NORHT_EAST:
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

        public static void Position(int direction, ref int x, ref int y) 
        {
            switch(direction) 
            {
            case NORTH:
                y -= 1;
                break;
            case NORHT_EAST:
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

