﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summons.Engine;

namespace Summons
{
    //This is a singleton class
    public class Map
    {
        static Map instance = new Map();
        public int width = 0; // in tiles
        public int height = 0; // in tiles
        double tileSize = Settings.TILE_SIZE;
        Dictionary<char, Texture2D> textureDict;
        String[] mapData;
        private Map() {}
        public static SpriteBatch mapSprite;
        

        static Dictionary<char, double> tileMoveCost = new Dictionary<char, double>()
        {
            {'0', 100.0},  // water
            {'1', 1.0},  // grass
            {'2', 4.0}, // mountain
            {'3', 2.0}, // swamp
            {'t', 1.0} // tower
        };

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
                this.height = mapData.Length;
                this.width = mapData[0].Length;
            }
        }

        public void Draw(GraphicsDevice graphics)
        {
            textureDict = new Dictionary<char, Texture2D>();
            textureDict.Add('0', Assets.waterTile);
            textureDict.Add('1', Assets.grassTile);
            textureDict.Add('2', Assets.mountainTile);
            textureDict.Add('3', Assets.swampTile);
            textureDict.Add('t', Assets.towerTile);

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

        public Stack<Coordinate> FindPath(int startX, int startY, int destX, int destY, Player player)
        {
            Stack<Coordinate> path = new Stack<Coordinate>();
            bool[,] teammateMap = GetTeammateLocations(startX, startY, player);
            double[,] moveMap = GetMovementMap(startX, startY, null, 30.0, teammateMap);
            return ExtractPath(moveMap, destX, destY);
        }

        private bool[,] GetTeammateLocations(int startX, int startY, Player player)
        {
            // We don't want to move into the same spot as a teammate
            bool[,] teammateMap = new bool[height, width];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    teammateMap[j, i] = false;
                }
            }
            
            foreach (Monster actor in MonsterManager.getInstance().monsterCollection)
            {
                if ((actor.TileX != startX || actor.TileY != startY) && actor.player == player)
                {
                    teammateMap[actor.TileY, actor.TileX] = true;
                }
            }

            return teammateMap;
        }

        private double[,] GetMovementMap(int x, int y, double[,] moveMap, double movement, bool[,] teammateMap)
        {
            if (moveMap == null)
            {
                moveMap = new double[height, width];

                // Initialize the move map to -1 to show it's fresh
                for (int i = 0; i < moveMap.GetLength(0); i++)
                {
                    for (int j = 0; j < moveMap.GetLength(1); j++)
                    {
                        moveMap[i, j] = -1.0;
                    }
                }
            }
            else
            {
                // Don't explore this space if a teammate is standing in it
                if (teammateMap[y, x])
                {
                    return moveMap;
                }

                movement -= tileMoveCost[mapData[y][x]];  // we don't subtract movement for the starting location
            }

            if (moveMap[y, x] < movement)
            {
                moveMap[y, x] = movement;

                if (movement > 0)
                {
                    if (x > 0)
                        GetMovementMap(x - 1, y, moveMap, movement, teammateMap);
                    if (x < width - 1)
                        GetMovementMap(x + 1, y, moveMap, movement, teammateMap);
                    if (y > 0)
                        GetMovementMap(x, y - 1, moveMap, movement, teammateMap);
                    if (y < height - 1)
                        GetMovementMap(x, y + 1, moveMap, movement, teammateMap);
                }
            }

            return moveMap;
        }

        Stack<Coordinate> ExtractPath(double[,] moveMap, int x, int y, Stack<Coordinate> path = null)
        {
            if (path == null)
                path = new Stack<Coordinate>();

            if (moveMap[y, x] > -1)
            {
                path.Push(new Coordinate(x, y));

                double up, down, left, right;

                // Find out the remaining movement in each of the neighboring tiles
                up = (y > 0 && moveMap[y - 1, x] != -1 && moveMap[y - 1, x] >= moveMap[y, x]) ? moveMap[y - 1, x] : -1;
                down = (y < moveMap.GetLength(0) - 1 && moveMap[y + 1, x] != -1 && moveMap[y + 1, x] >= moveMap[y, x]) ? moveMap[y + 1, x] : -1;
                left = (x > 0 && moveMap[y, x - 1] != -1 && moveMap[y, x - 1] >= moveMap[y, x]) ? moveMap[y, x - 1] : -1;
                right = (x < moveMap.GetLength(1) - 1 && moveMap[y, x + 1] != -1 && moveMap[y, x + 1] >= moveMap[y, x]) ? moveMap[y, x + 1] : -1;

                // As long as one neighbor is valid
                if (up > -1 || down > -1 || left > -1 || right > -1)
                {
                    double maxMove = Math.Max(up, Math.Max(down, Math.Max(left, right)));

                    if (up == maxMove)
                        path = ExtractPath(moveMap, x, y - 1, path);
                    else if (down == maxMove)
                        path = ExtractPath(moveMap, x, y + 1, path);
                    else if (left == maxMove)
                        path = ExtractPath(moveMap, x - 1, y, path);
                    else if (right == maxMove)
                        path = ExtractPath(moveMap, x + 1, y, path);
                }
            }

            return path;
        }

        public double GetTileFactor(int x, int y)
        {
            return tileMoveCost[mapData[y][x]];
        }
    }
}
