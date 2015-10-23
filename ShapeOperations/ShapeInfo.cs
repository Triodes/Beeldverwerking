using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace INFOIBV.ShapeOperations
{
    public class ShapeInfo
    {
        private readonly double area;
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

        public Point Center
        {
            get { return new Point(bounding[0] + Width / 2, bounding[1] + Height / 2); }
        }

        public double Ratio
        {
            get { return Height > Width ? Height / Width : Width / Height; }
        }
    }
}
