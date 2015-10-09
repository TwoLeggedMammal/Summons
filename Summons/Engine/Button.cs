using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Summons.Engine
{
    public abstract class Button
    {
        public int width, height;
        public Texture2D icon;
        public String text;
        public Dialog parent;
        public bool hovered = false;
        private int _x, _y;

        public int x
        {
            get
            {
                return _x;
            }
            set
            {
                _x = this.parent.x + value;
            }
        }

        public int y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = this.parent.y + value;
            }
        }

        public Button(Dialog parent, Texture2D icon, String text, int x = 0, int y = 0)
        {
            this.parent = parent;
            this.x = x;  // The value passed in is relative to the parent
            this.y = y;  // The value passed in is relative to the parent
            this.icon = icon;
            this.text = text;
            this.width = icon != null? icon.Width : Convert.ToInt32(Assets.mainFont.MeasureString(this.text).Length());
            this.height = icon != null? icon.Height : 32;
            UI.getInstance().AddButton(this);
        }

        public abstract void ClickHandler();

        public void Draw(SpriteBatch sprite)
        {
            Color color = this.hovered ? new Color(230, 230, 230) : Color.White;

            if (this.icon != null)
            {
                sprite.Draw
                            (
                                icon,
                                new Rectangle
                                (
                                    this._x,
                                    this._y,
                                    this.width,
                                    this.height
                                ),
                                color
                            );
            }

            if (this.text != null)
            {
                double textWidth = Assets.mainFont.MeasureString(this.text).Length();
                sprite.DrawString(Assets.mainFont, this.text, new Vector2(this._x + this.width - Convert.ToInt32(textWidth), this._y), color);
            }
        }

        public void Click(MouseState mouseState)
        {
            if (this.parent.visible &&
                mouseState.X >= this._x &&
                mouseState.X <= this._x + this.width &&
                mouseState.Y >= this._y &&
                mouseState.Y <= this._y + this.height)
            {
                this.ClickHandler();
            }
        }
    }

    public class CloseButton : Button
    {
        public CloseButton(Dialog parent, int x = 0, int y = 0)
            : base(parent, null, "Close", x, y)
        { }

        public override void ClickHandler()
        {
            // Close the parent dialog
            this.parent.visible = false;
        }
    }

    public class SummonMenuButton : Button
    {
        public SummonMenuButton(Dialog parent, int x = 0, int y = 0)
            : base(parent, Assets.summonIcon, null, x, y)
        { }

        public override void ClickHandler()
        {
            // Open the monster summon dialog
            UI.getInstance().OpenSummonDialog();
        }
    }

    public class SummonMonsterButton : Button
    {
        public SummonMonsterButton(Dialog parent, int x, int y, Monster monster)
            : base(parent, monster.texture, null, x, y)
        { }

        public override void ClickHandler()
        {

        }
    }    
}
