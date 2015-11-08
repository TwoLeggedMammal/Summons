using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summons.Engine;
using Microsoft.Xna.Framework.Input;

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
        public static SpriteBatch mapSprite;
        public List<Tower> towers;
        public List<Coordinate> summonLocations;
        public Route previewRoute;

        static Dictionary<char, double> tileMoveCost = new Dictionary<char, double>()
        {
            {'0', 100.0},  // water
            {'1', 1.0},  // grass
            {'2', 4.0}, // mountain
            {'3', 2.0}, // swamp
            {'t', 1.0}, // tower
            {'a', 1.0}, // tower + player 1 starting location
            {'b', 1.0}, // tower + player 2 starting location
            {'c', 1.0}, // tower + player 3 starting location
            {'d', 1.0} // tower + player 4 starting location

        };

        private Map() 
        {
            this.summonLocations = new List<Coordinate>();
        }

        public static Map getInstance()
        {
            return instance;
        }

        public void LoadMap(string mapTextFile, GraphicsDevice graphics)
        {
            // Load the file
            using (StreamReader sr = new StreamReader(Settings.MAPS_ROOT + mapTextFile))
            {
                this.mapData = sr.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                
                // Set the dimensions of our map
                this.height = mapData.Length;
                this.width = mapData[0].Length;
            }

            // Extract tower locations
            towers = new List<Tower>();

            for (int y = 0; y < this.mapData.Length; y++)
            {
                for (int x = 0; x < this.mapData[y].Length; x++)
                {
                    if (this.mapData[y][x] == 't' || (this.mapData[y][x] >= 'a' && this.mapData[y][x] <= 'd'))
                    {
                        towers.Add(new Tower(x, y));
                    }
                }
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
            textureDict.Add('a', Assets.towerTile);  // Player 1 starting location
            textureDict.Add('b', Assets.towerTile);  // Player 2 starting location
            textureDict.Add('c', Assets.towerTile);  // Player 3 starting location
            textureDict.Add('d', Assets.towerTile);  // Player 4 starting location

            mapSprite = new SpriteBatch(graphics);

            mapSprite.Begin();

            int highlightColor = Convert.ToInt32(210.0 + Math.Abs(((DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond * 10)) % 80) - 40));

            for (int y = 0; y < mapData.Length; y++)
            {
                for (int x = 0; x < mapData[y].Length; x++)
                {
                    Color color = Color.White;

                    // Check to see if this is a summoning location
                    if (Input.getInstance().clickAction == Input.ClickAction.SUMMON_MONSTER)
                    {
                        if (this.IsSummonLocation(x, y))
                            color = new Color(highlightColor, highlightColor, Convert.ToInt32(highlightColor * 0.8));
                    }

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
                            color
                        );
                }
            }

            foreach (Tower tower in this.towers)
            {
                tower.Draw(graphics, mapSprite);
            }

            if (previewRoute != null)
            {
                previewRoute.Draw(graphics, mapSprite);
            }
            
            mapSprite.End();
        }

        public bool IsSummonLocation(int x, int y)
        {
            foreach (Coordinate coord in this.summonLocations)
            {
                if (coord == new Coordinate(x, y))
                    return true;
            }
            return false;
        }

        public Stack<Coordinate> FindPath(int startX, int startY, int destX, int destY, Player player)
        {
            Stack<Coordinate> path = new Stack<Coordinate>();
            bool[,] teammateMap = GetActorLocations(startX, startY, player);
            double[,] moveMap = GetMovementMap(startX, startY, null, 30.0, teammateMap);
            return ExtractPath(startX, startY, moveMap, destX, destY);
        }

        public Coordinate GetSpawnPoint(int playerNumber)
        {
            char spawnChar = playerNumber == 1 ? 'a' : playerNumber == 2 ? 'b' : playerNumber == 3 ? 'c' : 'd';

            for (int y = 0; y < this.mapData.Length; y++)
            {
                for (int x = 0; x < this.mapData[y].Length; x++ )
                {
                    if (mapData[y][x] == spawnChar)
                    {
                        return new Coordinate(x, y);
                    }
                }
            }

            return null;
        }

        public bool[,] GetActorLocations(int startX, int startY, Player player, bool myTeamOnly = true)
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
                if ((actor.TileX != startX || actor.TileY != startY) && (!myTeamOnly || actor.player == player))
                {
                    teammateMap[actor.TileY, actor.TileX] = true;
                }
            }

            return teammateMap;
        }

        public double[,] GetMovementMap(int x, int y, double[,] moveMap, double movement, bool[,] teammateMap)
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

        Stack<Coordinate> ExtractPath(int startX, int startY, double[,] moveMap, int x, int y, Stack<Coordinate> path = null)
        {
            if (path == null)
                path = new Stack<Coordinate>();

            if (moveMap[y, x] > -1)
            {
                if (!(x == startX && y == startY))
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
                        path = ExtractPath(startX, startY, moveMap, x, y - 1, path);
                    else if (down == maxMove)
                        path = ExtractPath(startX, startY, moveMap, x, y + 1, path);
                    else if (left == maxMove)
                        path = ExtractPath(startX, startY, moveMap, x - 1, y, path);
                    else if (right == maxMove)
                        path = ExtractPath(startX, startY, moveMap, x + 1, y, path);
                }
            }

            return path;
        }

        public double GetTileFactor(int x, int y)
        {
            return tileMoveCost[mapData[y][x]];
        }

        public void LoadSummonOverlay(Player player)
        {
            Monster summoner = player.summoner;
            this.summonLocations = new List<Coordinate>();
            bool[,] actorLocations = this.GetActorLocations(-1, -1, player, false);

            for (int i = summoner.TileX - 1; i < summoner.TileX + 2; i++)
            {
                for (int j = summoner.TileY - 1; j < summoner.TileY + 2; j++)
                {
                    // See if this is an elligible location for a spawn point
                    if (!actorLocations[j, i] && Map.tileMoveCost[this.mapData[j][i]] <= 1.0)
                    {
                        summonLocations.Add(new Coordinate(i, j));
                    }
                }
            }
        }

        public void PlanRoute(Monster monster, int x, int y)
        {
            if (x >= 0 && x < this.width && y >= 0 && y < this.height)
            {
                previewRoute = new Route(monster, x, y);
            }
        }
    }

    public class Route
    {
        public Stack<Coordinate> routeData;
        public Actor monster;

        public Route(Actor monster, int x, int y)
        {
            this.monster = monster;
            this.routeData = new Stack<Coordinate>();
            if (monster.moveMap != null && !(monster.TileX == x && monster.TileY == y))
            {
                if (monster.moveMap[y, x] > -1)
                {
                    this.routeData = Map.getInstance().FindPath(monster.TileX, monster.TileY, x, y, monster.player);
                }
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sprite)
        {
            // And draw a little player symbol on that flag
            for (int i = 0; i < this.routeData.Count; i++)
            {
                Texture2D texture = null;

                if (i == this.routeData.Count - 1)
                    texture = Assets.mapDestinationFlag;
                else if (this.routeData.ElementAt(i).x < this.routeData.ElementAt(i + 1).x)
                    texture = Assets.mapArrowRight;
                else if (this.routeData.ElementAt(i).x > this.routeData.ElementAt(i + 1).x)
                    texture = Assets.mapArrowLeft;
                else if (this.routeData.ElementAt(i).y < this.routeData.ElementAt(i + 1).y)
                    texture = Assets.mapArrowDown;
                else if (this.routeData.ElementAt(i).y > this.routeData.ElementAt(i + 1).y)
                    texture = Assets.mapArrowUp;

                double currentMoveMapValue = monster.moveMap[monster.TileY, monster.TileX];
                double tileMoveMapValue = monster.moveMap[Convert.ToInt32(this.routeData.ElementAt(i).y), Convert.ToInt32(this.routeData.ElementAt(i).x)];
                bool moveableThisTurn = monster.remainingMovement >= currentMoveMapValue - tileMoveMapValue;

                if (texture != null)
                    sprite.Draw
                        (
                            texture,
                            new Rectangle
                            (
                                Convert.ToInt32(this.routeData.ElementAt(i).x) * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().X),
                                Convert.ToInt32(this.routeData.ElementAt(i).y) * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().Y),
                                Settings.TILE_SIZE,
                                Settings.TILE_SIZE
                            ),
                            moveableThisTurn ? Color.Blue : Color.White
                        );
            }
        }
    }

    public class Tower
    {
        public Player owner;
        public int X;
        public int Y;
        int xOffset, yOffset;

        public Tower(int x, int y)
        {
            owner = null;
            this.X = x;
            this.Y = y;
            this.xOffset = 25;
            this.yOffset = -15;
        }

        public void Capture(Actor monster, bool alert = true)
        {
            if (this.owner != monster.player)
            {
                if (this.owner != null)
                {
                    this.owner.towersOwned--;  // Remove from the old owner's tower count
                }
                this.owner = monster.player;
                this.owner.towersOwned++;  // Add to the new owner's tower count
                if (alert)
                    EventsManager.getInstance().RecordEvent(EventsManager.Event.TOWER_CAPTURED, monster);
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sprite)
        {
            if (this.owner != null)
            {
                // Draw the flag on this tower
                sprite.Draw
                    (
                        Assets.towerFlag,
                        new Rectangle
                        (
                            this.X * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().X) + this.xOffset,
                            this.Y * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().Y) + this.yOffset,
                            32,
                            24
                        ),
                        Color.White
                    );

                // And draw a little player symbol on that flag
                sprite.Draw
                    (
                        this.owner.flag,
                        new Rectangle
                        (
                            this.X * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().X) + this.xOffset + 7,
                            this.Y * Settings.TILE_SIZE - Convert.ToInt32(Camera.getInstance().Y) + this.yOffset + 3,
                            16,
                            16
                        ),
                        this.owner.symbolColor
                    );
            }
        }
    }
}
