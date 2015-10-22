using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Summons.Engine
{
    public class UI
    {
        static UI instance = new UI();
        public Queue<TextDialog> textDialogCollection;
        public PlayerStatusDialog playerStatusDialog;
        public PlayerActionDialog playerActionDialog;
        public MonsterSummonDialog monsterSummonDialog;
        public List<MonsterStatusDialog> monsterStatusDialogCollection;
        public Queue<FloatingMessage> floatingMessageCollection;
        public List<Button> buttonCollection;
        public GraphicsDevice graphics;

        private UI() 
        {
            this.buttonCollection = new List<Button>();
            this.textDialogCollection = new Queue<TextDialog>();
            this.monsterStatusDialogCollection = new List<MonsterStatusDialog>();
            this.floatingMessageCollection = new Queue<FloatingMessage>();
        }

        public static UI getInstance() 
        {
            return instance;
        }

        public void Initialize(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            this.playerStatusDialog = new PlayerStatusDialog();
            this.playerActionDialog = new PlayerActionDialog();
        }

        public void OpenTextDialog(int x, int y, int width, String text)
        {
            // Width and hight should be multiples of the UI_TILE_SIZE
            textDialogCollection.Enqueue(new TextDialog(x, y, width, text));
        }

        public void OpenSummonDialog()
        {
            this.monsterSummonDialog.visible = true;
        }

        public void ShowMessage(String text, FloatingMessage.TransitionType transition = FloatingMessage.TransitionType.FIXED, Actor actor = null)
        {
            floatingMessageCollection.Enqueue(new FloatingMessage(text, transition, actor));
        }

        public MonsterStatusDialog MakeMonsterStatusDialog(Monster monster)
        {
            MonsterStatusDialog dialog = new MonsterStatusDialog(monster);
            monsterStatusDialogCollection.Add(dialog);
            return dialog;
        }

        public void AddButton(Button button)
        {
            // Register the button so it can be registered for input
            this.buttonCollection.Add(button);
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


            foreach (FloatingMessage message in floatingMessageCollection)
            {
                message.Update(timeSinceLastFrame);

                if (message.complete)
                {
                    floatingMessageCollection.Dequeue();
                    break;
                }
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
                if (dialog.visible)
                    dialog.Draw();
            }

            foreach (FloatingMessage message in floatingMessageCollection)
            {
                message.Draw();
            }

            playerStatusDialog.Draw();
            playerActionDialog.Draw();
            monsterSummonDialog.Draw();
        }

        public bool Click(MouseState mouseState)
        {
            bool clicked = false;

            // Check to see if we clicked on the currently visible text dialog
            if (textDialogCollection.Count > 0)
                clicked = textDialogCollection.Peek().Click(mouseState) || clicked;

            // Check to see if we've clicked on any dialogs/buttons
            clicked = this.monsterSummonDialog.Click(mouseState) || clicked;
            clicked = this.playerActionDialog.Click(mouseState) || clicked;

            return clicked;
        }
    }

    public class FloatingMessage
    {
        public String text = "";
        public double elapsedTime = 0.0;
        public double floatAmout = 50.0;
        public bool complete = false;
        public double lifespan = 0.5;
        public Actor attachedMonster;
        public SpriteBatch dialogSprite;
        public TransitionType transitionType;

        public enum TransitionType
        {
            FIXED,
            EXPANDING,
            FLOATING
        }

        public FloatingMessage(String text, TransitionType transition = TransitionType.FIXED, Actor attachedMonster = null)
        {
            this.text = text;
            this.attachedMonster = attachedMonster;
            this.dialogSprite = new SpriteBatch(UI.getInstance().graphics);
            this.transitionType = transition;
        }

        public void Update(double timeSinceLastFrame)
        {
            this.elapsedTime += timeSinceLastFrame;
            if (this.elapsedTime > this.lifespan)
                this.complete = true;
        }

        public void Draw()
        {
            double scale = this.transitionType == TransitionType.EXPANDING? 0.5 + (elapsedTime / lifespan) : 1.0;
            double textWidth = Assets.mainFont.MeasureString(this.text).Length() * scale;
            double yModifier = this.transitionType == TransitionType.FLOATING ? floatAmout * (elapsedTime / lifespan) : 0.0;
            double xPos = this.attachedMonster == null? (Settings.SCREEN_WIDTH / 2) - (textWidth / 2.0) : this.attachedMonster.X - (textWidth / 2.0) + (Settings.TILE_SIZE / 2) - Camera.getInstance().X;
            double yPos = this.attachedMonster == null ? (Settings.SCREEN_HEIGHT / 2.0) - ((32 * scale) / 2.0) : this.attachedMonster.Y - Camera.getInstance().Y - ((32 * scale) / 2.0) + 16;
                
            dialogSprite.Begin();
            dialogSprite.DrawString(Assets.mainFont, 
                this.text, 
                new Vector2(Convert.ToInt32(xPos), 
                    Convert.ToInt32(yPos - yModifier)), 
                    Color.White,
                    (float)0.0,  // Rotation
                    new Vector2(0, 0),  // Origin?
                    new Vector2((float)scale, (float)scale),  // Scale
                    SpriteEffects.None,
                    (float)1.0);  // Layer depth?
            dialogSprite.End();
        }
    }
}
