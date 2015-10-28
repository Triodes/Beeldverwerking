using System;
using System.Drawing;

namespace INFOIBV.CardOperations 
{

    // Struct containing the four corner points of a card
    public struct Card
    {
        // The four corner points
        public readonly Point topLeft;
        public readonly Point topRight;
        public readonly Point bottomRight;
        public readonly Point bottomLeft;

        public Card(Point topLeft, Point topRight, Point bottomRight, Point bottomLeft) 
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
        }

        public void Draw(Graphics g, Pen pen)
        {
            g.DrawLine(pen, topLeft, topRight);
            g.DrawLine(pen, topRight, bottomRight);
            g.DrawLine(pen, bottomRight, bottomLeft);
            g.DrawLine(pen, bottomLeft, topLeft);
        }

        public void Draw(Graphics g)
        {
            Draw(g, Pens.Cyan);
        }
    }

}

