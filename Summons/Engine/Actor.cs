﻿using System;
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
        protected double xOffset = 0.0;
        protected double yOffset = 0.0;
        Camera camera;
        protected SpriteBatch actorSprite;
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
            texture = Assets.blackMageActor;
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
                                Convert.ToInt32(X - Camera.getInstance().X + this.xOffset),
                                Convert.ToInt32(Y - Camera.getInstance().Y - 15.0 + this.yOffset),  // Actors sit a bit higher on the tile so they look 3D
                                texture.Width,
                                texture.Height
                            ),
                            Selected ? new Color(highlightColor, highlightColor, Convert.ToInt32(highlightColor * 0.8)) : Color.White
                        );

            actorSprite.End();
        }

        public void Select()
        {
            Selected = !Selected;
            EventsManager.getInstance().RecordEvent(EventsManager.Event.ACTOR_CLICKED);
        }

        public void SetDestination(int x, int y)
        {
            Selected = false;
            path = Map.getInstance().FindPath(TileX, TileY, x, y);
            
            // Did we click on an inaccessible location?
            if (path.Count == 0)
                EventsManager.getInstance().RecordEvent(EventsManager.Event.INVALID_ACTOR_DESTINATION);
        }
    }

    class Monster : Actor
    {
        public String name;
        public int HP, maxHP, XP, maxXP;
        public int rangedAP, rangedAccuracy, rangedAttacks; 
        public int meleeAP, meleeAccuracy, meleeAttacks;
        public int armor;
        public MonsterStatusDialog status;

        public Monster(int x, int y) 
            : base(x, y)
        {
            // Default values for everything in case we don't define in a subclass
            this.name = "???";
            this.XP = 0;
            this.maxXP = 100;
            this.HP = this.maxHP = 10;
            this.rangedAP = 1;
            this.rangedAccuracy = 50;
            this.rangedAttacks = 3;
            this.meleeAP = 2;
            this.meleeAccuracy = 75;
            this.meleeAttacks = 2;
            this.armor = 0;
            this.texture = Assets.blackMageActor;
            this.status = UI.getInstance().MakeMonsterStatusDialog(this);
        }
    }

    class BlackMage : Monster
    {
        public BlackMage(int x, int y) 
            : base(x, y)
        {
            this.texture = Assets.blackMageActor;
            this.name = "Black Mage";
            this.HP = this.maxHP = 25;
        }
    }

    class BlueDragon : Monster
    {
        public BlueDragon(int x, int y) 
            : base(x, y)
        {
            this.texture = Assets.blueDragonActor;
            this.xOffset = -10.0;
            this.name = "Blue Dragon";
            this.armor = 1;
            this.HP = this.maxHP = 80;
        }
    }

    class HeavyKnight : Monster
    {
        public HeavyKnight(int x, int y)
            : base(x, y)
        {
            this.texture = Assets.heavyKnightActor;
            this.yOffset = -20.0;
            this.name = "Heavy Knight";
            this.armor = 2;
            this.HP = this.maxHP = 50;
        }
    }
}
