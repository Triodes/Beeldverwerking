using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace INFOIBV.ShapeOperations
{
    // Class holding 
    public class ShapeInfo
    {
        // The area of the same measured in the number of pixels
        private readonly double area;
        // The axis-aligned bounding values of the shape (minX, minY, maxX, maxY)
        private readonly int[] bounding;

        public ShapeInfo(double area, int[] bounding)
        {
            this.area = area;
            this.bounding = bounding;
        }

        public double Area
        {
            get { return area; }
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

        // The center point of the axis-aligned bounding box of the shape
        public Point Center
        {
            get { return new Point(bounding[0] + Width / 2, bounding[1] + Height / 2); }
        }

        // The ratio between the width and the height always expressed as a value >= 1.0
        public double Ratio
        {
            get { return Height > Width ? Height / Width : Width / Height; }
        }
    }
}
