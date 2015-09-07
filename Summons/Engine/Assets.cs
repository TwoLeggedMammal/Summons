using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Summons
{
    /// <summary>
    /// Holds all the global variables we'll want to use through the project.
    /// Created as a Singleton for easy access.
    /// </summary>
    public class Assets
    {
        static Assets instance = new Assets();
        public static Texture2D waterTile, grassTile, mountainTile, swampTile, towerTile;
        public static Texture2D towerFlag;
        public static Texture2D blackMageActor, blueDragonActor, heavyKnightActor, archerActor;
        public static Texture2D uiTexture;
        public static Texture2D meleeIcon, hpIcon, rangedIcon, defenseIcon, manaIcon, towerIcon;
        public static Texture2D summonIcon;
        public static Texture2D playerOneSymbol, playerTwoSymbol;
        public static Texture2D plainTexture;
        public static SpriteFont mainFont;
        
        private Assets() { }

        public static Assets getInstance()
        {
            return instance;
        }

        public void LoadTextures(ContentManager content, GraphicsDevice graphics)
        {
            // Load our map tiles
            waterTile = content.Load<Texture2D>("Tiles/water64");
            grassTile = content.Load<Texture2D>("Tiles/grass64");
            mountainTile = content.Load<Texture2D>("Tiles/mountain64");
            swampTile = content.Load<Texture2D>("Tiles/swamp64");
            towerTile = content.Load<Texture2D>("Tiles/tower64");

            // Load our actors
            blackMageActor = content.Load<Texture2D>("Actors/blackmage");
            blueDragonActor = content.Load<Texture2D>("Actors/bluedragon");
            heavyKnightActor = content.Load<Texture2D>("Actors/heavyknight");
            archerActor = content.Load<Texture2D>("Actors/archer");

            // Load UI textures
            uiTexture = content.Load<Texture2D>("UI/dialog_border");
            meleeIcon = content.Load<Texture2D>("UI/sword_icon");
            rangedIcon = content.Load<Texture2D>("UI/ranged_icon");
            hpIcon = content.Load<Texture2D>("UI/hp_icon");
            defenseIcon = content.Load<Texture2D>("UI/shield_icon");
            manaIcon = content.Load<Texture2D>("UI/mana");
            towerIcon = content.Load<Texture2D>("UI/tower");
            towerFlag = content.Load<Texture2D>("UI/flag");
            summonIcon = content.Load<Texture2D>("UI/summon_icon");

            // Load player symbols
            playerOneSymbol = content.Load<Texture2D>("UI/player1_symbol");
            playerTwoSymbol = content.Load<Texture2D>("UI/player2_symbol");

            // Generate our plain white pixel texture for use in HP bars
            plainTexture = new Texture2D(graphics, 1, 1);
            plainTexture.SetData(new[] { Color.White });

            // Load our fonts
            mainFont = content.Load<SpriteFont>("Fonts/Visitor");
        }
    }
}
