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
        public int x, y;
        public int width, height;
        public Texture2D icon;
        public Dialog parent;
        public bool hovered = false;

        public Button(Dialog parent, Texture2D icon, int x = 0, int y = 0)
        {
            this.parent = parent;
            this.x = parent.x + x;  // The value passed in is relative to the parent
            this.y = parent.y + y;  // The value passed in is relative to the parent
            this.icon = icon;
            this.width = icon.Width;
            this.height = icon.Height;
            UI.getInstance().AddButton(this);
        }

        public abstract void ClickHandler();

        public void Draw(SpriteBatch sprite)
        {
            sprite.Draw
                        (
                            icon,
                            new Rectangle
                            (
                                this.x,
                                this.y,
                                this.width,
                                this.height
                            ),
                            this.hovered ? new Color(230, 230, 230) : Color.White
                        );
        }

        public void Click(MouseState mouseState)
        {
            if (this.parent.visible &&
                mouseState.X >= this.x &&
                mouseState.X <= this.x + this.width &&
                mouseState.Y >= this.y &&
                mouseState.Y <= this.y + this.height)
            {
                this.ClickHandler();
            }
        }
    }

    public class SummonButton : Button
    {
        public SummonButton(Dialog parent, int x = 0, int y = 0)
            : base(parent, Assets.summonIcon, x, y)
        { }

        public override void ClickHandler()
        {
            Console.WriteLine("Clicked!");
        }
    }    
}
