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
        public bool selected;

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
                            selected ? new Color(highlightColor, highlightColor, Convert.ToInt32(highlightColor * 0.8)) : Color.White
                        );

            actorSprite.End();
        }

        public void Select()
        {
            selected = !selected;
        }
    }
}
