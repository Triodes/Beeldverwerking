using System;
using System.Collections.Generic;

namespace INFOIBV.ShapeOperations {

    public enum Directions
    {
        NORTH = 0,
        NORHT_EAST,
        EAST,
        SOUTH_EAST,
        SOUTH,
        SOUTH_WEST,
        WEST,
        NORTH_WEST,
        MAX_DIRECTION
    }

    public static class Perimeter {

        public static IList<Directions> walkPerimeter(int[,] image, int startX, int startY)
        {
            IList<Directions> result = new List<Directions>();
            Directions currentDirection = Directions.EAST;

            int width = image.GetLength(0);
            int height = image.GetLength(1);

            int currX = startX;
            int currY = startY;

            // Walk until we reach the starting position again
            do 
            {
                if(isPartOfShape(image, currentDirection, currX, currY))
                {
                    // The current direction is still part of the shape, try turning left.
                    do 
                    {
                        currentDirection = (Directions)(((int)currentDirection - 1 + (int)Directions.MAX_DIRECTION) % (int)Directions.MAX_DIRECTION);
                    } while(isPartOfShape(image, currentDirection, currX, currY));
                    currentDirection = (Directions)(((int)currentDirection + 1) % (int)Directions.MAX_DIRECTION);
                }
                else
                {
                    // The current direction is no longer part of the shape, try turning right.
                    do 
                    {
                        currentDirection = (Directions)(((int)currentDirection + 1) % (int)Directions.MAX_DIRECTION);
                    } while(!isPartOfShape(image, currentDirection, currX, currY));
                }

                // Append to the path.
                position(currentDirection, ref currX, ref currY);
                Console.WriteLine("Going {0} [{1}x{2}] - [{3}x{4}]", currentDirection, currX, currY, startX, startY);
                result.Add(currentDirection);
            } while(currX != startX || currY != startY);

            return result;
        }

        public static double computeLength(IList<Directions> path) 
        {
            double result = 0;
            foreach(Directions direction in path) 
            {
                if((int)direction % 2 == 0) 
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

        public static void position(Directions direction, ref int x, ref int y) 
        {
            switch(direction) 
            {
            case Directions.NORTH:
                y -= 1;
                break;
            case Directions.NORHT_EAST:
                y -= 1;
                x += 1;
                break;
            case Directions.EAST:
                x += 1;
                break;
            case Directions.SOUTH_EAST:
                y += 1;
                x += 1;
                break;
            case Directions.SOUTH:
                y += 1;
                break;
            case Directions.SOUTH_WEST:
                y += 1;
                x -= 1;
                break;
            case Directions.WEST:
                x -= 1;
                break;
            case Directions.NORTH_WEST:
                y -= 1;
                x -= 1;
                break;
            }
        }

        public static bool isPartOfShape(int[,] image, Directions direction, int x, int y)
        {
            position(direction, ref x, ref y);          

            // FIXME: Check bounds
            return image[x, y] > 0;
        }

    }
}

