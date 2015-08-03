using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Summons
{
    //This is a singleton class
    public class Map
    {
        static Map instance = new Map();
        public double width = 0; // in tiles
        public double height = 0; // in tiles
        double tileSize = Settings.TILE_SIZE;
        Dictionary<char, Texture2D> textureDict;
        String[] mapData;
        private Map() {}
        public static SpriteBatch mapSprite;

        public static Map getInstance()
        {
            return instance;
        }

        public void LoadMap(string mapTextFile, GraphicsDevice graphics)
        {
            // Load the file
            using (StreamReader sr = new StreamReader(Settings.MAPS_ROOT + mapTextFile))
            {
                mapData = sr.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                
                // Set the dimensions of our map
                this.height = mapData.Length * Settings.TILE_SIZE;
                this.width = mapData[0].Length * Settings.TILE_SIZE;
            }

            //GenerateMapTexture(graphics);
        }

        public void Draw(GraphicsDevice graphics)
        {
            textureDict = new Dictionary<char, Texture2D>();
            textureDict.Add('0', Assets.waterTile);
            textureDict.Add('1', Assets.grassTile);
            textureDict.Add('2', Assets.mountainTile);
            textureDict.Add('3', Assets.swampTile);

            mapSprite = new SpriteBatch(graphics);

            mapSprite.Begin();

            for (int y = 0; y < mapData.Length; y++)
            {
                for (int x = 0; x < mapData[y].Length; x++)
                {
                    mapSprite.Draw
                        (
                            textureDict[mapData[y][x]], 
                            new Rectangle
                            (
                                x * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().X), 
                                y * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().Y), 
                                Settings.TILE_SIZE, 
                                Settings.TILE_SIZE
                            ), 
                            Color.White
                        );
                }
            }
            
            mapSprite.End();
        }
    }
}
