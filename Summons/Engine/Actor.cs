using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Summons.Engine
{
    class Actor
    {
        public Texture2D texture;
        public double X;
        public double Y;
        Camera camera;
        SpriteBatch actorSprite;
        public bool Selected;
        Stack<Coordinate> path;
        double speed = 300.0;

        public int TileX
        {
            get
            {
                return Convert.ToInt32(X / Convert.ToDouble(Settings.TILE_SIZE));
            }
            set
            {
                X = Convert.ToDouble(value * Settings.TILE_SIZE);
            }
        }

        public int TileY
        {
            get
            {
                return Convert.ToInt32(Y / Convert.ToDouble(Settings.TILE_SIZE));
            }
            set
            {
                Y = Convert.ToDouble(value * Settings.TILE_SIZE);
            }
        }

        public Actor(int x, int y)
        {
            TileX = x;
            TileY = y;
            texture = Assets.summonerActor;
            camera = Camera.getInstance();
            path = new Stack<Coordinate>();
        }

        public void Update(double timeSinceLastFrame)
        {
            if (path.Count > 0)
            {
                double xDiff = (path.Peek().x * Settings.TILE_SIZE) - X;
                double yDiff = (path.Peek().y * Settings.TILE_SIZE) - Y;
                double moveAmount = (speed * timeSinceLastFrame);

                // Slow down the movement based on the tile over which they are moving
                moveAmount /= Map.getInstance().GetTileFactor(TileX, TileY);

                // If we've just got a little bit to move, then we only move that little bit this frame
                if (moveAmount > Math.Max(Math.Abs(xDiff), Math.Abs(yDiff)))
                    moveAmount = Math.Max(Math.Abs(xDiff), Math.Abs(yDiff));

                if (xDiff > 0)
                    X += moveAmount;
                else if (xDiff < 0)
                    X -= moveAmount;

                if (yDiff > 0)
                    Y += moveAmount;
                else if (yDiff < 0)
                    Y -= moveAmount;

                if (xDiff == 0 && yDiff == 0)
                {
                    Console.WriteLine(path.Pop());
                }
            }
        }
        
        public void Draw(GraphicsDevice graphics)
        {
            int highlightColor = Convert.ToInt32(170.0 + Math.Abs(((DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond * 10)) % 80) - 40));

            actorSprite = new SpriteBatch(graphics);
            actorSprite.Begin();

            actorSprite.Draw
                        (
                            texture,
                            new Rectangle
                            (
                                Convert.ToInt32(X - Camera.getInstance().X),
                                Convert.ToInt32(Y - Camera.getInstance().Y - 15.0),  // Actors sit a bit higher on the tile so they look 3D
                                Settings.TILE_SIZE,
                                Settings.TILE_SIZE
                            ),
                            Selected ? new Color(highlightColor, highlightColor, Convert.ToInt32(highlightColor * 0.8)) : Color.White
                        );

            actorSprite.End();
        }

        public void Select()
        {
            Selected = !Selected;
        }

        public void SetDestination(int x, int y)
        {
            Selected = false;
            path = Map.getInstance().FindPath(TileX, TileY, x, y);
        }
    }
}
