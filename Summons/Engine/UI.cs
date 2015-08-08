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

        public void OpenDialog(int x, int y, int width, String text)
        {
            // width and hight should be multiples of the UI_TILE_SIZE
            dialogCollection.Add(new TextDialog(x, y, width, text, graphics));
        }

        public void Update(double timeSinceLastFrame)
        {
            foreach (Dialog d in dialogCollection)
            {
                d.Update(timeSinceLastFrame);
            }
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
        public int tileHeight, tileWidth;
        public int padding;
        public SpriteBatch dialogSprite;

        public Dialog(int x, int y, int width, int height, GraphicsDevice graphics)
        {
            this.padding = 1;  // How much extra space we leave around the text in tiles
            this.x = x;
            this.y = y;
            this.width = width;
            this.tileWidth = Convert.ToInt32(width / Settings.UI_TILE_SIZE) + padding;
            this.height = height;
            this.tileHeight = Convert.ToInt32(height / Settings.UI_TILE_SIZE) + padding;
            this.dialogSprite = new SpriteBatch(graphics);
        }

        public virtual void Update(double timeSinceLastFrame) { }
        public virtual void Draw() 
        {
            this.dialogSprite.Begin();
            
            // Draw the window in which our content goes
            for (int i = 0; i < tileWidth; i++)
            {
                for (int j = 0; j < tileHeight; j++)
                {
                    // Figure out which part of the textbox texture to use, which is a 3x3 grid of textures rolled up together
                    int textureX = i == 0 ? 0 : i == tileWidth - 1 ? 2 : 1;
                    int textureY = j == 0 ? 0 : j == tileHeight - 1 ? 2 : 1;

                    dialogSprite.Draw(Assets.uiTexture,
                        new Vector2(this.x + (i * Settings.UI_TILE_SIZE) - (this.padding * Settings.UI_TILE_SIZE / 2), this.y + (j * Settings.UI_TILE_SIZE) - (this.padding * Settings.UI_TILE_SIZE / 2)),
                        new Rectangle(textureX * Settings.UI_TILE_SIZE, textureY * Settings.UI_TILE_SIZE, Settings.UI_TILE_SIZE, Settings.UI_TILE_SIZE),
                        Color.White);
                }
            }

            this.dialogSprite.End();
        }
    }


    class TextDialog : Dialog
    {
        public String text;
        public double elapsedTime;
        public double textSpeed;

        public TextDialog(int x, int y, int width, String text, GraphicsDevice graphics) : base(x, y, width, 0, graphics)
        {
            this.textSpeed = 32.0;
            this.text = this.WrapText(text);
            int lines = this.text.Split('\n').Length;
            this.height = lines * Settings.UI_TILE_SIZE;
            this.tileHeight = Convert.ToInt32(height / Settings.UI_TILE_SIZE) + padding;
        }

        public override void Update(double timeSinceLastFrame)
        {
            elapsedTime += timeSinceLastFrame;
        }

        public override void Draw()
        {
            base.Draw();

            int stringEndpoint = Convert.ToInt32(elapsedTime * textSpeed) > text.Length ? text.Length : Convert.ToInt32(elapsedTime * textSpeed);

            dialogSprite.Begin();
            dialogSprite.DrawString(Assets.mainFont, this.text.Substring(0, stringEndpoint), new Vector2(this.x, this.y), Color.White);
            dialogSprite.End();
        }

        // Taken from https://danieltian.wordpress.com/2008/12/24/xna-tutorial-typewriter-text-box-with-proper-word-wrapping-part-2/
        private String WrapText(String text)
        {
            String line = String.Empty;
            String returnString = String.Empty;
            String[] wordArray = text.Split(' ');

            foreach (String word in wordArray)
            {
                if (Assets.mainFont.MeasureString(line + word).Length() > this.width)
                {
                    returnString = returnString + line + '\n';
                    line = String.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }
    }
}
