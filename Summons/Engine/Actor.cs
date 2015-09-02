using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Summons.Engine
{
    public class MonsterManager
    {
        public List<Monster> monsterCollection;
        static MonsterManager instance = new MonsterManager();

        private MonsterManager()
        {
            monsterCollection = new List<Monster>();
        }

        public static MonsterManager getInstance()
        {
            return instance;
        }

        public void Update(double timeSinceLastFrame)
        {
            foreach (Monster monster in monsterCollection)
            {
                monster.Update(timeSinceLastFrame);
            }
        }

        public void UIUpdate(double timeSinceLastFrame)
        {
            foreach (Monster monster in monsterCollection)
            {
                monster.UIUpdate(timeSinceLastFrame);
            }
        }

        public void Draw(GraphicsDevice graphics)
        {
            foreach (Monster monster in monsterCollection)
            {
                monster.Draw(graphics);
            }
        }

        public void Kill(Monster monster)
        {
            this.monsterCollection.Remove(monster);
        }
    }

    public class Actor
    {
        public Texture2D texture;
        public double X;
        public double Y;
        protected double xOffset = 0.0;
        protected double yOffset = 0.0;
        Camera camera;
        protected SpriteBatch actorSprite;
        public bool Selected = false;
        public bool Hovered = false;
        protected Stack<Coordinate> path;
        double speed = 300.0;
        public Player player;

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

        public Actor(int x, int y, Player player)
        {
            TileX = x;
            TileY = y;
            texture = Assets.blackMageActor;
            camera = Camera.getInstance();
            path = new Stack<Coordinate>();
            this.player = player;
        }

        public virtual void Update(double timeSinceLastFrame)
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
        
        public virtual void Draw(GraphicsDevice graphics)
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
            path = Map.getInstance().FindPath(TileX, TileY, x, y, player);
            
            // Did we click on an inaccessible location?
            if (path.Count == 0)
                EventsManager.getInstance().RecordEvent(EventsManager.Event.INVALID_ACTOR_DESTINATION);
        }
    }

    public class Monster : Actor
    {
        public String name;
        public int HP, maxHP, XP, maxXP;
        public int rangedAP, rangedAccuracy, rangedAttacks; 
        public int meleeAP, meleeAccuracy, meleeAttacks;
        public int critRate;
        public int armor;
        public MonsterStatusDialog status;
        double damageTimer = 0.0;
        double fullDamageTime = 0.5;
        protected int previousHP = -1;

        public Monster(int x, int y, Player player) 
            : base(x, y, player)
        {
            // Default values for everything in case we don't define in a subclass
            this.name = "???";
            this.XP = 0;
            this.maxXP = 100;
            this.HP = this.maxHP = this.previousHP = 10;
            this.rangedAP = 1;
            this.rangedAccuracy = 50;
            this.rangedAttacks = 3;
            this.meleeAP = 2;
            this.meleeAccuracy = 75;
            this.meleeAttacks = 2;
            this.armor = 0;
            this.critRate = 5;
            this.texture = Assets.blackMageActor;
            this.status = UI.getInstance().MakeMonsterStatusDialog(this);
        }

        public override void Draw(GraphicsDevice graphics)
        {
            base.Draw(graphics);

            actorSprite.Begin();

            // Draw the HP bar for the Monster
            double hpBonus = this.damageTimer >= this.fullDamageTime ? 0.0 : (1.0 - (this.damageTimer / this.fullDamageTime)) * (Convert.ToDouble(this.previousHP - this.HP));
            double hpPercentage = (Convert.ToDouble(this.HP + hpBonus) / Convert.ToDouble(this.maxHP));
            Color hpBarColor = hpPercentage == 1.0 ? Color.LimeGreen : hpPercentage > 0.75 ? Color.Blue : hpPercentage > 0.5 ? Color.Yellow : Color.Red;

            actorSprite.Draw
                        (
                            Assets.plainTexture,
                            new Rectangle
                            (
                                Convert.ToInt32(X - Camera.getInstance().X + 3),
                                Convert.ToInt32(Y - Camera.getInstance().Y + Settings.TILE_SIZE - 15),
                                Convert.ToInt32((Settings.TILE_SIZE - 6) * hpPercentage),
                                5
                            ),
                            hpBarColor
                        );

            // Draw the player symbol next to the Monster
            actorSprite.Draw
                        (
                            this.player.flag,
                            new Rectangle
                            (
                                Convert.ToInt32(X - Camera.getInstance().X + Settings.TILE_SIZE - Settings.PLAYER_SYMBOL_SIZE),
                                Convert.ToInt32(Y - Camera.getInstance().Y + Settings.TILE_SIZE - Settings.PLAYER_SYMBOL_SIZE - 15),
                                Settings.PLAYER_SYMBOL_SIZE,
                                Settings.PLAYER_SYMBOL_SIZE
                            ),
                            player.symbolColor
                        );

            actorSprite.End();
        }

        public override void Update(double timeSinceLastFrame)
        {
            base.Update(timeSinceLastFrame);

            // Check to see if we've engaged in battle or bumped into a teammate
            foreach (Monster monster in MonsterManager.getInstance().monsterCollection)
            {
                if (monster != this && this.path.Count > 0 &&
                    ((monster.TileX == this.TileX && monster.TileY == this.TileY) ||
                    (monster.TileX == this.path.Peek().x && monster.TileY == this.path.Peek().y)))
                {
                    if (this.player == monster.player)
                    {
                        // We're friends!
                        this.path.Clear();
                    }
                    else
                    {
                        // Fight it out!
                        this.path.Clear();
                        EventsManager.getInstance().RecordEvent(EventsManager.Event.BATTLE_ENGAGED);
                        Combat.getInstance().FightItOut(this, monster);
                    }
                }
            }

            this.UIUpdate(timeSinceLastFrame);
        }

        public void UIUpdate(double timeSinceLastFrame)
        {
            // Add time to our hp bar animation
            this.damageTimer += timeSinceLastFrame;
        }

        public void ReceiveDamage(int hpLoss)
        {
            this.previousHP = this.HP;  // For the hp bar animation
            this.HP -= hpLoss;
            this.damageTimer = 0.0;
        }

        public void Die()
        {
            // TODO: Add death animation
            MonsterManager.getInstance().Kill(this);
            this.player.monsterCollection.Remove(this);
        }
    }

    public class BlackMage : Monster
    {
        public BlackMage(int x, int y, Player player) 
            : base(x, y, player)
        {
            this.texture = Assets.blackMageActor;
            this.name = "Black Mage";
            this.HP = this.maxHP = this.previousHP = 25;
            this.meleeAccuracy = 70;
            this.meleeAP = 3;
            this.rangedAccuracy = 66;
            this.rangedAP = 12;
        }
    }

    public class BlueDragon : Monster
    {
        public BlueDragon(int x, int y, Player player) 
            : base(x, y, player)
        {
            this.texture = Assets.blueDragonActor;
            this.xOffset = -10.0;
            this.name = "Blue Dragon";
            this.armor = 1;
            this.HP = this.maxHP = this.previousHP = 80;
            this.meleeAccuracy = 85;
            this.meleeAP = 8;
            this.rangedAccuracy = 60;
            this.rangedAP = 6;
        }
    }

    public class HeavyKnight : Monster
    {
        public HeavyKnight(int x, int y, Player player)
            : base(x, y, player)
        {
            this.texture = Assets.heavyKnightActor;
            this.yOffset = -20.0;
            this.name = "Heavy Knight";
            this.armor = 2;
            this.HP = this.maxHP = this.previousHP = 50;
            this.meleeAccuracy = 75;
            this.meleeAP = 6;
            this.rangedAccuracy = 50;
            this.rangedAP = 5;
        }
    }

    public class Archer : Monster
    {
        public Archer(int x, int y, Player player)
            : base(x, y, player)
        {
            this.texture = Assets.archerActor;
            this.yOffset = -20.0;
            this.name = "Heavy Knight";
            this.armor = 0;
            this.HP = this.maxHP = this.previousHP = 45;
            this.meleeAccuracy = 75;
            this.meleeAP = 5;
            this.rangedAccuracy = 70;
            this.rangedAP = 8;
        }
    }
}
