﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Summons.Engine
{
    class UI
    {
        static UI instance = new UI();
        public Queue<TextDialog> textDialogCollection;
        public List<MonsterStatusDialog> monsterStatusDialogCollection;
        public GraphicsDevice graphics;

        private UI() 
        {
            textDialogCollection = new Queue<TextDialog>();
            monsterStatusDialogCollection = new List<MonsterStatusDialog>();
        }

        public static UI getInstance() 
        {
            return instance;
        }

        public void OpenTextDialog(int x, int y, int width, String text)
        {
            // width and hight should be multiples of the UI_TILE_SIZE
            textDialogCollection.Enqueue(new TextDialog(x, y, width, text));
        }

        public MonsterStatusDialog MakeMonsterStatusDialog(Monster monster)
        {
            MonsterStatusDialog dialog = new MonsterStatusDialog(monster);
            monsterStatusDialogCollection.Add(dialog);
            return dialog;
        }

        public void Update(double timeSinceLastFrame)
        {
            if (textDialogCollection.Count > 0)
            {
                TextDialog dialog = textDialogCollection.Peek();
                dialog.Update(timeSinceLastFrame);

                if (dialog.complete)
                    textDialogCollection.Dequeue();
            }
        }

        public void Draw()
        {
            if (textDialogCollection.Count > 0)
            {
                textDialogCollection.Peek().Draw();
            }

            foreach (MonsterStatusDialog dialog in monsterStatusDialogCollection)
            {
                if (dialog.monster.Selected)
                    dialog.Draw();
            }
        }

        public bool Click(MouseState mouseState)
        {
            bool clicked = false;

            if (textDialogCollection.Count > 0)
                clicked = textDialogCollection.Peek().Click(mouseState) || clicked;

            return clicked;
        }
    }

    class Dialog
    {
        public int x, y, width, height;
        public int tileHeight, tileWidth;
        public int padding;
        public bool visible;
        public SpriteBatch dialogSprite;

        public Dialog(int x, int y, int width, int height)
        {
            this.visible = true;
            this.padding = 1;  // How much extra space we leave around the text in tiles
            this.x = x;
            this.y = y;
            this.width = width;
            this.tileWidth = Convert.ToInt32(width / Settings.UI_TILE_SIZE) + padding;
            this.height = height;
            this.tileHeight = Convert.ToInt32(height / Settings.UI_TILE_SIZE) + padding;
            this.dialogSprite = new SpriteBatch(UI.getInstance().graphics);
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

        public virtual bool Click(MouseState mouseState)
        {
            return false;
        }
    }

    class MonsterStatusDialog : Dialog
    {
        public Monster monster;
        static int statusWidth = 384;
        static int statusHeight = 160;

        public MonsterStatusDialog(Monster monster)
            : base(Settings.SCREEN_WIDTH - (MonsterStatusDialog.statusWidth + 32), 
                Settings.SCREEN_HEIGHT - (MonsterStatusDialog.statusHeight + 32),
                MonsterStatusDialog.statusWidth,
                MonsterStatusDialog.statusHeight)
        {
            this.monster = monster;
        }

        public override void Draw()
        {
            base.Draw();

            dialogSprite.Begin();

            dialogSprite.DrawString(Assets.mainFont, this.monster.name, new Vector2(this.x, this.y), Color.White);
            String xpText = String.Format("XP {0}/{1}", this.monster.XP.ToString(), this.monster.maxXP.ToString());
            double xpTextWidth = Assets.mainFont.MeasureString(xpText).Length();
            dialogSprite.DrawString(Assets.mainFont, xpText, new Vector2(this.x + this.width - Convert.ToInt32(xpTextWidth), this.y), Color.White);

            dialogSprite.End();
        }
    }

    class TextDialog : Dialog
    {
        public String text;
        public double elapsedTime;
        public double textSpeed;
        public bool complete;
        public double lifespan;

        public TextDialog(int x, int y, int width, String text) : base(x, y, width, 0)
        {
            this.complete = false;
            this.textSpeed = 32.0;
            this.lifespan = 3.0;
            this.text = this.WrapText(text);
            int lines = this.text.Split('\n').Length;
            this.height = lines * Settings.UI_TILE_SIZE;
            this.tileHeight = Convert.ToInt32(height / Settings.UI_TILE_SIZE) + padding;
        }

        public override void Update(double timeSinceLastFrame)
        {
            this.elapsedTime += timeSinceLastFrame;
            if (this.elapsedTime > this.lifespan + (this.text.Length / this.textSpeed))
                this.complete = true;
        }

        public override void Draw()
        {
            base.Draw();

            int stringEndpoint = Convert.ToInt32(elapsedTime * textSpeed) > text.Length ? text.Length : Convert.ToInt32(elapsedTime * textSpeed);

            dialogSprite.Begin();
            dialogSprite.DrawString(Assets.mainFont, this.text.Substring(0, stringEndpoint), new Vector2(this.x, this.y), Color.White);
            dialogSprite.End();
        }

        public override bool Click(MouseState mouseState)
        {
            if ((mouseState.X > this.x - (padding * Settings.UI_TILE_SIZE / 2)) &&
                (mouseState.X < this.x + (padding * Settings.UI_TILE_SIZE / 2) + this.width) &&
                (mouseState.Y > this.y - (padding * Settings.UI_TILE_SIZE / 2)) &&
                (mouseState.Y < this.y + (padding * Settings.UI_TILE_SIZE / 2) + this.height))
            {
                // If clicked on a dialog when it's still writing, then we just speed up the process
                if (Convert.ToInt32(elapsedTime * textSpeed) < text.Length)
                {
                    elapsedTime = text.Length / textSpeed;
                }
                else
                {
                    // And if it's done, then we mark it for deletion
                    this.complete = true;
                }
                
                return true;
            }

            return false;
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
