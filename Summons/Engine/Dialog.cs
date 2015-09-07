using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Summons.Engine
{
    public class Dialog
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

    public class MonsterStatusDialog : Dialog
    {
        public Monster monster;
        static int statusWidth = 384;
        static int statusHeight = 128;

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

            // Name and XP
            String xpText = String.Format("XP {0}/{1}", this.monster.XP.ToString(), this.monster.maxXP.ToString());
            double xpTextWidth = Assets.mainFont.MeasureString(xpText).Length();
            dialogSprite.DrawString(Assets.mainFont, xpText, new Vector2(this.x + this.width - Convert.ToInt32(xpTextWidth), this.y), Color.White);

            // Health and defense
            dialogSprite.Draw(Assets.hpIcon, new Rectangle(this.x, this.y + 32, 24, 24), Color.White);
            String hpText = String.Format("{0}/{1}", this.monster.HP.ToString(), this.monster.maxHP.ToString());
            dialogSprite.DrawString(Assets.mainFont, hpText, new Vector2(this.x + 32, this.y + 32), Color.White);
            double hpTextWidth = Assets.mainFont.MeasureString(hpText).Length();
            dialogSprite.Draw(Assets.defenseIcon, new Rectangle(this.x + 64 + Convert.ToInt32(hpTextWidth), this.y + 32, 24, 24), Color.White);
            dialogSprite.DrawString(Assets.mainFont, this.monster.armor.ToString(), new Vector2(this.x + 96 + Convert.ToInt32(hpTextWidth), this.y + 32), Color.White);

            // Melee attack
            dialogSprite.Draw(Assets.meleeIcon, new Rectangle(this.x, this.y + 64, 24, 24), Color.White);
            String meleeText = String.Format("{0}x{1} ({2}%)", this.monster.meleeAP.ToString(), this.monster.meleeAttacks.ToString(), this.monster.meleeAccuracy.ToString());
            dialogSprite.DrawString(Assets.mainFont, meleeText, new Vector2(this.x + 32, this.y + 64), Color.White);

            // Ranged attack
            dialogSprite.Draw(Assets.rangedIcon, new Rectangle(this.x, this.y + 96, 24, 24), Color.White);
            String rangedText = String.Format("{0}x{1} ({2}%)", this.monster.rangedAP.ToString(), this.monster.rangedAttacks.ToString(), this.monster.rangedAccuracy.ToString());
            dialogSprite.DrawString(Assets.mainFont, rangedText, new Vector2(this.x + 32, this.y + 96), Color.White);

            dialogSprite.End();
        }
    }

    public class PlayerStatusDialog : Dialog
    {
        static int statusWidth = 402;
        static int statusHeight = 96;

        public PlayerStatusDialog()
            : base(Settings.SCREEN_WIDTH - (PlayerStatusDialog.statusWidth + 32),
                32,
                PlayerStatusDialog.statusWidth,
                PlayerStatusDialog.statusHeight)
        { }

        public override void Draw()
        {
            base.Draw();

            dialogSprite.Begin();

            List<Player> players = PlayerManager.getInstance().playerCollection;

            for (int i = 0; i < players.Count; i++)
            {
                int xPos = this.x;

                // Player Icon
                dialogSprite.Draw
                        (
                            players[i].flag,
                            new Rectangle
                            (
                                xPos,
                                this.y + (i * 50),
                                Settings.PLAYER_SYMBOL_SIZE,
                                Settings.PLAYER_SYMBOL_SIZE
                            ),
                            players[i].symbolColor
                        );
                xPos += Settings.PLAYER_SYMBOL_SIZE + 10;

                // Player Name
                dialogSprite.DrawString(Assets.mainFont, players[i].name, new Vector2(this.x + Settings.PLAYER_SYMBOL_SIZE + 10, this.y + (i * 50)), Color.White);
                xPos += Convert.ToInt32(Assets.mainFont.MeasureString(players[i].name).Length()) + 10;

                // Mana info
                dialogSprite.Draw
                        (
                            Assets.manaIcon,
                            new Rectangle
                            (
                                xPos,
                                this.y + (i * 50),
                                24,
                                24
                            ),
                            Color.White
                        );
                xPos += 24 + 10;

                dialogSprite.DrawString(Assets.mainFont, Convert.ToString(players[i].mana), new Vector2(xPos, this.y + (i * 50)), Color.White);
                xPos += Convert.ToInt32(Assets.mainFont.MeasureString(players[i].mana.ToString()).Length()) + 10;

                // Tower info
                dialogSprite.Draw
                        (
                            Assets.towerIcon,
                            new Rectangle
                            (
                                xPos,
                                this.y + (i * 50),
                                24,
                                24
                            ),
                            Color.White
                        );
                xPos += 24 + 10;

                dialogSprite.DrawString(Assets.mainFont, Convert.ToString(players[i].towersOwned), new Vector2(xPos, this.y + (i * 50)), Color.White);
            }

            dialogSprite.End();
        }
    }

    public class PlayerActionDialog : Dialog
    {
        static int width = 64;
        static int height = 192;
        public List<Button> buttonCollection;

        public PlayerActionDialog()
            : base(32,
                Convert.ToInt32((Settings.SCREEN_HEIGHT - PlayerActionDialog.height) / 2.0),
                PlayerActionDialog.width,
                PlayerActionDialog.height)
        {
            this.buttonCollection = new List<Button>();

            // Summon button
            this.buttonCollection.Add(new SummonButton(this));
        }

        public override void Draw()
        {
            base.Draw();

            dialogSprite.Begin();

            foreach (Button b in this.buttonCollection)
            {
                b.Draw(this.dialogSprite);
            }

            dialogSprite.End();
        }
    }

    public class TextDialog : Dialog
    {
        public String text;
        public double elapsedTime;
        public double textSpeed;
        public bool complete;
        public double lifespan;

        public TextDialog(int x, int y, int width, String text)
            : base(x, y, width, 0)
        {
            this.complete = false;
            this.textSpeed = 48.0;
            this.lifespan = 1.0;
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
