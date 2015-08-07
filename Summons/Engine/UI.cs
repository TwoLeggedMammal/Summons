using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Summons.Engine
{
    class UI
    {
        static UI instance = new UI();
        public List<Dialog> dialogCollection;
        public GraphicsDevice graphics;

        private UI() 
        {
            dialogCollection = new List<Dialog>();
        }

        public static UI getInstance() 
        {
            return instance;
        }

        public void OpenDialog(int x, int y, int width, int height, String text)
        {
            // width and hight should be multiples of the UI_TILE_SIZE
            dialogCollection.Add(new Dialog(x, y, width, height, text, graphics));
        }

        public void Draw()
        {
            foreach (Dialog d in dialogCollection)
            {
                d.Draw();
            }
        }
    }

    class Dialog
    {
        public int x, y, width, height;
        public String text;
        SpriteBatch dialogSprite;

        public Dialog(int x, int y, int width, int height, String text, GraphicsDevice graphics)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.text = text;
            dialogSprite = new SpriteBatch(graphics);
        }

        public void Draw()
        {
            dialogSprite.Begin();

            dialogSprite.DrawString(Assets.mainFont, this.text, new Vector2(this.x, this.y), Color.White);

            dialogSprite.End();
        }
    }
}
