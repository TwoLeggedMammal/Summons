using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq.Expressions;

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
            if (Settings.MOVE_DURING_END_OF_TURN)
            {
                // If there are no more monsters left to move then we pass the turn
                if (!MoveMonsters(timeSinceLastFrame))
                {
                    PlayerManager.getInstance().EndTurn();
                }
            }
            else
            {
                MoveMonsters(timeSinceLastFrame);
            }
        }

        public bool MoveMonsters(double timeSinceLastFrame)
        {
            bool moved = false;

            // Move each monster one at a time, and only during the movement phase
            foreach (Monster monster in monsterCollection)
            {
                if (monster.player == PlayerManager.getInstance().currentPlayer &&
                    monster.remainingMovement > 0 &&
                    monster.route != null &&
                    monster.route.routeData.Count > 0)
                {
                    monster.Update(timeSinceLastFrame);
                    moved = true;
                    break;
                }
            }

            return moved;
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
            monsterCollection.Sort((a, b) => a.Y.CompareTo(b.Y));  // Higher up monsters get drawn first, for proper layering
            foreach (Monster monster in monsterCollection)
            {
                monster.Draw(graphics);
            }
        }

        public virtual bool Click(MouseState mouseState)
        {
            bool actorClicked = false;

            foreach (Monster monster in this.monsterCollection)
            {
                actorClicked = monster.Click(mouseState) || actorClicked;
            }

            return actorClicked;
        }

        public void Kill(Monster monster)
        {
            this.monsterCollection.Remove(monster);
        }

        public Monster Spawn(Type monsterType, int x, int y, Player player, bool free=false)
        {
            Monster monster = null;

            // This acts as a factory. Any new monsters need a block of logic here
            if (monsterType == typeof(BlackMage))
                monster = new BlackMage(x, y, player);
            else if (monsterType == typeof(BlueDragon))
                monster = new BlueDragon(x, y, player);
            else if (monsterType == typeof(HeavyKnight))
                monster = new HeavyKnight(x, y, player);
            else if (monsterType == typeof(Archer))
                monster = new Archer(x, y, player);
            else if (monsterType == typeof(BlackKnight))
                monster = new BlackKnight(x, y, player);
            else if (monsterType == typeof(Assassin))
                monster = new Assassin(x, y, player);
            else
                throw new System.ArgumentException("Monster type not registered in the MonsterManager.Spawn factory", "monsterType");

            // The UI is responsible for ensuring we have enough mana because we can do this
            if (!free)
                player.mana -= monster.manaCost;

            this.monsterCollection.Add(monster);
            player.monsterCollection.Add(monster);
            return monster;
        }

        public void UnselectMonsters(Actor ignore)
        {
            foreach (Actor actor in this.monsterCollection)
                if (actor != ignore)
                    actor.Selected = false;
        }
    }

    public class Actor
    {
        public Texture2D texture;
        public double X;
        public double Y;
        public double xOffset = 0.0;
        public double yOffset = 0.0;
        protected Camera camera;
        protected SpriteBatch actorSprite;
        public bool Selected = false;
        public bool Hovered = false;
        public Route route;
        public double[,] moveMap;
        double speed = 300.0;
        public Player player;
        public double movement = 5.0;
        public double remainingMovement;

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
            this.player = player;
            this.remainingMovement = this.movement;
        }

        public virtual void Update(double timeSinceLastFrame)
        {
            if (route != null && route.routeData.Count > 0 && remainingMovement >= Map.getInstance().GetTileFactor(Convert.ToInt32(route.routeData.Peek().x), Convert.ToInt32(route.routeData.Peek().y)))
            {
                double xDiff = (route.routeData.Peek().x * Settings.TILE_SIZE) - X;
                double yDiff = (route.routeData.Peek().y * Settings.TILE_SIZE) - Y;
                double moveAmount = (speed * timeSinceLastFrame);

                // Slow down the movement based on the tile over which they are moving
                moveAmount /= Map.getInstance().GetTileFactor(TileX, TileY);

                // If we've just got a little bit left to move, then we only move that little bit this frame
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

                // We've moved completely into the tile
                if (xDiff == 0 && yDiff == 0)
                {
                    route.routeData.Pop();
                    this.remainingMovement -= Map.getInstance().GetTileFactor(TileX, TileY);

                    // Check to see if we've moved onto a tower
                    foreach (Tower tower in Map.getInstance().towers)
                    {
                        if (tower.X == this.TileX && tower.Y == this.TileY)
                        {
                            tower.Capture(this);
                        }
                    }
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
                            Selected ? new Color(highlightColor, highlightColor, Convert.ToInt32(highlightColor * 0.8)) : 
                                this.remainingMovement == 0 ? Settings.OUT_OF_MOVEMENT_COLOR :
                                    Color.White
                        );

            actorSprite.End();
        }

        public void Select()
        {
            // We can only select one at a time, so let's unselect other monsters which may be selected
            Selected = !Selected;

            if (Selected)
            {
                Input.getInstance().clickAction = Input.ClickAction.MOVE_MONSTER;
                Input.getInstance().selectedMonster = (Monster)this;
                MonsterManager.getInstance().UnselectMonsters(this);
                
                // When clicked we build a movement map to refer to when planning a move
                this.moveMap = Map.getInstance().GetMovementMap(this.TileX, 
                    this.TileY, 
                    null, 
                    50.0, 
                    Map.getInstance().GetActorLocations(this.TileX, this.TileY, this.player));
                Console.Write(this.moveMap);
            }
            else
                Input.getInstance().clickAction = Input.ClickAction.NO_ACTION;

            EventsManager.getInstance().RecordEvent(EventsManager.Event.ACTOR_CLICKED);
        }

        public void SetDestination(int x, int y)
        {
            Selected = false;
            route = new Route(this, x, y);
            
            // Did we click on an inaccessible location?
            if (route.routeData.Count == 0)
                EventsManager.getInstance().RecordEvent(EventsManager.Event.INVALID_ACTOR_DESTINATION);
        }

        public virtual bool Click(MouseState mouseState)
        {
            // Override in child classes
            return false;
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
        public int manaCost = 20;
        public bool prefersMelee = true;
        public int monsterLevel;

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
            this.monsterLevel = 1;  // Used in deciding relative XP gain from battles with this monster
            this.texture = Assets.blackMageActor;
            this.status = UI.getInstance().MakeMonsterStatusDialog(this);
        }

        public int BonusAccuracy()
        {
            return Convert.ToInt32(Convert.ToDouble(this.XP) / 20.0);
        }

        public int BonusCrit()
        {
            return Convert.ToInt32(Convert.ToDouble(this.XP) / 20.0);
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
            // Check to see if we've engaged in battle or bumped into a teammate
            foreach (Monster monster in MonsterManager.getInstance().monsterCollection)
            {
                if (monster != this && this.route != null && this.route.routeData.Count > 0 &&
                    ((monster.TileX == this.TileX && monster.TileY == this.TileY) ||
                    (monster.TileX == this.route.routeData.Peek().x && monster.TileY == this.route.routeData.Peek().y)))
                {
                    if (this.player == monster.player)
                    {
                        // We're friends!
                        this.route.routeData.Clear();
                    }
                    else
                    {
                        // Fight it out!
                        this.route.routeData.Clear();
                        EventsManager.getInstance().RecordEvent(EventsManager.Event.BATTLE_ENGAGED);
                        Combat.getInstance().FightItOut(this, monster);
                    }
                }
            }

            base.Update(timeSinceLastFrame);

            this.status.visible = this.Hovered;
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
            UI.getInstance().monsterStatusDialogCollection.Remove(this.status);
            MonsterManager.getInstance().Kill(this);
            this.player.monsterCollection.Remove(this);
            EventsManager.getInstance().RecordEvent(EventsManager.Event.MONSTER_DIED);
        }

        public override bool Click(MouseState mouseState)
        {
            // You can only click on human controlled monsters
            if (this.player == PlayerManager.getInstance().currentPlayer)
            {
                if (mouseState.X + this.camera.X > (this.TileX * Settings.TILE_SIZE) && mouseState.X + this.camera.X < ((this.TileX + 1) * Settings.TILE_SIZE) &&
                    mouseState.Y + this.camera.Y > (this.TileY * Settings.TILE_SIZE) && mouseState.Y + this.camera.Y < ((this.TileY + 1) * Settings.TILE_SIZE))
                {
                    this.Select();
                    return true;
                }
            }

            return false;
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
            this.manaCost = 30;
            this.prefersMelee = false;
            this.monsterLevel = 2;
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
            this.manaCost = 70;
            this.monsterLevel = 3;
            this.movement = this.remainingMovement = 7;
        }
    }

    public class HeavyKnight : Monster
    {
        public HeavyKnight(int x, int y, Player player)
            : base(x, y, player)
        {
            this.texture = Assets.heavyKnightActor;
            this.yOffset = -18.0;
            this.name = "Heavy Knight";
            this.armor = 2;
            this.HP = this.maxHP = this.previousHP = 50;
            this.meleeAccuracy = 75;
            this.meleeAP = 6;
            this.rangedAccuracy = 50;
            this.rangedAP = 5;
            this.manaCost = 40;
            this.monsterLevel = 2;
        }
    }

    public class BlackKnight : Monster
    {
        public BlackKnight(int x, int y, Player player)
            : base(x, y, player)
        {
            this.texture = Assets.blackKnightActor;
            this.yOffset = -12.0;
            this.name = "Black Knight";
            this.armor = 2;
            this.HP = this.maxHP = this.previousHP = 75;
            this.meleeAccuracy = 80;
            this.meleeAP = 8;
            this.rangedAccuracy = 50;
            this.rangedAP = 5;
            this.manaCost = 60;
            this.monsterLevel = 3;
        }
    }

    public class Archer : Monster
    {
        public Archer(int x, int y, Player player)
            : base(x, y, player)
        {
            this.texture = Assets.archerActor;
            this.yOffset = -19.0;
            this.name = "Archer";
            this.armor = 0;
            this.HP = this.maxHP = this.previousHP = 45;
            this.meleeAccuracy = 75;
            this.meleeAP = 5;
            this.rangedAccuracy = 70;
            this.rangedAP = 8;
            this.manaCost = 20;
            this.prefersMelee = false;
        }
    }

    public class Assassin : Monster
    {
        public Assassin(int x, int y, Player player)
            : base(x, y, player)
        {
            this.texture = Assets.assassinActor;
            this.yOffset = -6.0;
            this.name = "Assassin";
            this.armor = 0;
            this.HP = this.maxHP = this.previousHP = 40;
            this.meleeAccuracy = 90;
            this.meleeAP = 2;
            this.meleeAttacks = 7;
            this.rangedAccuracy = 80;
            this.rangedAP = 4;
            this.manaCost = 25;
            this.critRate = 40;
        }
    }
}
