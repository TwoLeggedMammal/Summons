using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Summons
{
    /// <summary>
    /// Camera is the reference point when viewing all the graphics which aren't fixed to the screen, like the UI.
    /// We use the camera positions when rendering the map and actors and then every looks right as the 
    /// camera moves around.
    /// 
    /// This is a singleton.
    /// </summary>
    public class Camera
    {
        static Camera instance = new Camera();
        Coordinate pos;
        Double xMax;
        Double xMin;
        Double yMax;
        Double yMin;

        Double width = 0;
        Double height = 0;
        bool panning = false;
        public Stack<Coordinate> momentum;
        int numberOfReadings = 2;
        Coordinate speed;
        Double deceleration = 200.0;

        private Camera()
        {
            pos = new Coordinate();
            speed = new Coordinate();
            momentum = new Stack<Coordinate>();
        }

        public static Camera getInstance()
        {
            return instance;
        }

        public Coordinate Pos
        {
            get
            {
                return new Coordinate(this.X, this.Y);
            }
            set
            {
                pos = value;
            }
        }
        
        public Double Width
        {
            get
            {
                if (width < 0)
                {
                    return 0;
                }
                else
                {
                    return width;
                }
            }
            set
            {
                width = value;
            }
        }

        public Double Height
        {
            get
            {
                if (height < 0)
                {
                    return 0;
                }
                else
                {
                    return height;
                }
            }
            set
            {
                height = value;
            }
        }

        public Double X
        {
            get 
            {
                if (pos.x < XMin)
                {
                    return XMin;
                }
                else if (pos.x + Width > XMax)
                {
                    return XMax - Width;
                }
                else
                {
                    return pos.x;
                }
            }
            set { pos.x = value; }
        }

        public Double Y
        {
            get
            {
                if (pos.y < YMin)
                {
                    return YMin;
                }
                else if (pos.y + Height > YMax)
                {
                    return YMax - Height;
                }
                else
                {
                    return pos.y;
                }
            }
            set { pos.y = value; }
        }

        public Double XMin
        {
            get
            {
                if (xMin > xMax)
                {
                    return xMax;
                }
                else
                {
                    return xMin;
                }
            }
            set { xMin = value; }
        }

        public Double YMin
        {
            get
            {
                if (yMin > yMax)
                {
                    return yMax;
                }
                else
                {
                    return yMin;
                }
            }
            set { yMin = value; }
        }

        public Double XMax
        {
            get
            {
                if (xMax < xMin)
                {
                    return xMin;
                }
                else
                {
                    return xMax;
                }
            }
            set { xMax = value; }
        }

        public Double YMax
        {
            get
            {
                if (yMax < yMin)
                {
                    return yMin;
                }
                else
                {
                    return yMax;
                }
            }
            set { yMax = value; }
        }

        public bool Panning
        {
            get
            {
                return panning;
            }
            set { panning = value; }
        }

        public void calculateMomentum()
        {
            Coordinate sum = new Coordinate();
            int readings = Convert.ToInt32(numberOfReadings * (200.0 / 60.0));  // #TODO: replace hard code of 60 fps with actual

            for (int i = 0; i < readings; i++)
            {
                if (momentum.Count > 0)
                {
                    Coordinate c = momentum.Pop();
                    sum += c;
                }
            }

            momentum.Clear();

            speed = sum / Convert.ToDouble(numberOfReadings);
        }

        public void Update(double timeSinceLastFrame)
        {
            if (panning) momentum.Push(new Coordinate());

            pos -= speed;
            if (speed.x > 0)
            {
                speed.x -= deceleration / (1 / timeSinceLastFrame);
                if (speed.x < 0.0) speed.x = 0.0;
            }
            if (speed.y > 0)
            {
                speed.y -= deceleration / (1 / timeSinceLastFrame);
                if (speed.y < 0.0) speed.y = 0.0;
            }
            if (speed.x < 0)
            {
                speed.x += deceleration / (1 / timeSinceLastFrame);
                if (speed.x > 0.0) speed.x = 0.0;
            }
            if (speed.y < 0)
            {
                speed.y += deceleration / (1 / timeSinceLastFrame);
                if (speed.y > 0.0) speed.y = 0.0;
            }

        }

    }
}
