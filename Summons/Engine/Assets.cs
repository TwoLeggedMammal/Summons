using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Summons
{
    /// <summary>
    /// Holds all the global variables we'll want to use through the project.
    /// Created as a Singleton for easy access.
    /// </summary>
    class Assets
    {
        static Assets instance = new Assets();
        public static Texture2D waterTile, grassTile, mountainTile, swampTile;
        public static Texture2D summonerActor;
        public static Texture2D uiTexture;
        public static SpriteFont mainFont;

        private Assets() { }

        public static Assets getInstance()
        {
            return instance;
        }

        public void LoadTextures(ContentManager content)
        {
            // Load our map tiles
            waterTile = content.Load<Texture2D>("Tiles/water64");
            grassTile = content.Load<Texture2D>("Tiles/grass64");
            mountainTile = content.Load<Texture2D>("Tiles/mountain64");
            swampTile = content.Load<Texture2D>("Tiles/swamp64");

            // Load our actors
            summonerActor = content.Load<Texture2D>("Actors/blackmage");

            // Load UI textures
            uiTexture = content.Load<Texture2D>("UI/dialog_border");

            // Load our fonts
            mainFont = content.Load<SpriteFont>("Fonts/Visitor");
        }
    }
}
