using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Summons
{
    //This is a singleton class
    public class Map
    {
        static Map instance = new Map();
        double width = 0; // in tiles
        double height = 0; // in tiles
        double tileSize = Settings.TILE_SIZE;
        Dictionary<char, Texture2D> textureDict;
        String[] mapData;
        private Map() {}

        public static Map getInstance()
        {
            return instance;
        }

        public void LoadMap(string mapTextFile)
        {
            // Load the file
            using (StreamReader sr = new StreamReader(Settings.MAPS_ROOT + mapTextFile))
            {
                mapData = sr.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                
                // Set the dimensions of our map
                this.height = mapData.Length;
                this.width = mapData[0].Length;
            }

            LoadTextures();
        }

        public void LoadTextures()
        {
            textureDict = new Dictionary<char, Texture2D>();

            foreach (KeyValuePair<char, String> mapping in Settings.TILE_MAPPING)
            {
                //Texture2D waterTexture = new Texture2D();
            }
        }
    }
}
