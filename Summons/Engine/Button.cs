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
        public FontSize fontSize;
        public enum FontSize
        {
            SMALL,
            NORMAL
        }

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

        public Button(Dialog parent, Texture2D icon, String text, int x = 0, int y = 0, FontSize fontSize = FontSize.NORMAL)
        {
            this.parent = parent;
            this.x = x;  // The value passed in is relative to the parent
            this.y = y;  // The value passed in is relative to the parent
            this.icon = icon;
            this.text = text;
            this.fontSize = fontSize;

            int iconWidth = this.icon == null ? 0 : icon.Width;
            int textWidth = this.text == null ? 0 : Convert.ToInt32(Assets.mainFont.MeasureString(this.text).Length() * this.FontScale());

            this.width = Math.Max(iconWidth, textWidth);
            this.height = this.icon == null ? Convert.ToInt32(32 * this.FontScale()) : Convert.ToInt32(icon.Height);
            UI.getInstance().AddButton(this);
        }

        public abstract void ClickHandler();

        public virtual void Update() { }

        public void Draw(SpriteBatch sprite)
        {
            this.Update();
            Color color = this.hovered ? new Color(230, 230, 230) : Color.White;

            if (this.icon != null)
            {
                sprite.Draw
                            (
                                icon,
                                new Rectangle
                                (
                                    this._x + ((this.width - this.icon.Width) / 2), // Center the icon in case the text is wider
                                    this._y,
                                    this.icon.Width,
                                    this.icon.Height
                                ),
                                color
                            );
            }

            if (this.text != null)
            {
                float textWidth = Assets.mainFont.MeasureString(this.text).Length() * this.FontScale();
                
                sprite.DrawString(Assets.mainFont, 
                    this.text,
                    new Vector2(this._x + ((this.width - textWidth) / 2), // Center the text in case the icon is wider
                        this._y + (this.icon == null ? 0 : this.height)), 
                    color, 
                    0, 
                    new Vector2(0f, 0f), 
                    new Vector2(this.FontScale(), this.FontScale()),
                    SpriteEffects.None,
                    0f);
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

        public float FontScale()
        {
            return this.fontSize == FontSize.NORMAL ? 1.0f : this.fontSize == FontSize.SMALL ? 0.5f : 1.0f;
        }
    }

    public class CloseButton : Button
    {
        public CloseButton(Dialog parent)
            : base(parent, 
            null, 
            "Close", 
            parent.width - 105, 
            parent.height - 30)
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
            : base(parent, Assets.summonIcon, "Summon", x, y, FontSize.SMALL)
        { }

        public override void ClickHandler()
        {
            // Open the monster summon dialog
            UI.getInstance().OpenSummonDialog();
        }
    }

    public class PassTurnButton : Button
    {
        public PassTurnButton(Dialog parent, int x = 0, int y = 0)
            : base(parent, Assets.passIcon, "Pass", x, y, FontSize.SMALL)
        { }

        public override void ClickHandler()
        {
            EventsManager.getInstance().CurrentScene = EventsManager.Scene.MOVEMENT;
        }
    }

    public class GoButton : Button
    {
        public GoButton(Dialog parent, int x = 0, int y = 0)
            : base(parent, Assets.goIcon, "Move", x, y, FontSize.SMALL)
        { }

        public override void ClickHandler()
        {
            // Have all monsters move as far as they can
        }
    }

    public class SummonMonsterButton : Button
    {
        public Monster monster;
        public MonsterStatusDialog status;

        public SummonMonsterButton(Dialog parent, int x, int y, Monster monster)
            : base(parent, monster.texture, String.Format("{0} MP", monster.manaCost), x, y, FontSize.NORMAL)
        {
            this.monster = monster;
            this.status = UI.getInstance().MakeMonsterStatusDialog(monster);
            this.height += Convert.ToInt32(monster.yOffset);
        }

        public override void ClickHandler()
        {
            if (monster.player.mana >= monster.manaCost)
            {
                Map.getInstance().LoadSummonOverlay(PlayerManager.getInstance().currentPlayer);
                Input.getInstance().clickAction = Input.ClickAction.SUMMON_MONSTER;
                Input.getInstance().summonType = this.monster.GetType();
                this.status.visible = false;
                this.parent.visible = false;
            }
            else
            {
                EventsManager.getInstance().RecordEvent(EventsManager.Event.NOT_ENOUGH_MANA);
            }
        }

        public override void Update()
        {
            this.status.visible = this.hovered;
            base.Update();
        }
    }
}
