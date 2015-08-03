using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Summons
{
    public class Coordinate
    {
        // Pixel coordinate
        public double x, y;

        public Coordinate()
        {
            x = 0;
            y = 0;
        }

        public Coordinate(int xPos, int yPos)
        {
            x = xPos;
            y = yPos;
        }

        public Coordinate(double xPos, double yPos)
        {
            x = xPos;
            y = yPos;
        }

        public static Coordinate operator +(Coordinate c1, Coordinate c2)
        {
            return new Coordinate(c1.x + c2.x, c1.y + c2.y);
        }

        public static Coordinate operator -(Coordinate c1, Coordinate c2)
        {
            return new Coordinate(c1.x - c2.x, c1.y - c2.y);
        }

        public static Coordinate operator /(Coordinate c, double d)
        {
            return new Coordinate(c.x / d, c.y / d);
        }

        public static Coordinate operator *(Coordinate c, double d)
        {
            return new Coordinate(c.x * d, c.y * d);
        }

        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            return ((c1.x == c2.x) && (c1.y == c2.y));
        }

        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return ((c1.x != c2.x) || (c1.y != c2.y));
        }

        // THIS METHOD ASSUMES THE 2 POINTS SHARE A AXIS, MEANING EITHER c1.x == c2.x or c1.y == c2.y!!!
        public static string GetDirection(Coordinate c1, Coordinate c2)
        {
            if ((c1.x == c2.x) || (c1.y == c2.y))
            {
                if (c1.x != c2.x)
                    if (c1.x > c2.x)
                        return "West";
                    else
                        return "East";
                else
                    if (c1.y > c2.y)
                        return "North";
                    else
                        return "South";
            }
            else return null;
        }
    }
}
