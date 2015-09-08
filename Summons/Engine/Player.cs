using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Summons.Engine
{
    public class Player
    {
        public Texture2D flag;
        public List<Monster> monsterCollection;
        public Color symbolColor;
        public bool isAi;
        public String name;
        public int mana;
        public int towersOwned;
        public Monster summoner;

        public Player(int playerNumber, bool isAI)
        {
            monsterCollection = new List<Monster>();
            this.mana = 100;
            this.towersOwned = 0;

            if (playerNumber == 1)
            {
                this.flag = Assets.playerOneSymbol;
                this.symbolColor = Color.DeepSkyBlue;
                this.isAi = false;
                this.name = "Player 1";
            }
            else
            {
                this.flag = Assets.playerTwoSymbol;
                this.symbolColor = Color.Red;
                this.isAi = true;
                this.name = "CPU";
            }
        }
    }

    public class PlayerManager
    {
        public List<Player> playerCollection;
        static PlayerManager instance = new PlayerManager();

        private PlayerManager()
        {
            playerCollection = new List<Player>();
        }

        public void SpawnPlayers()
        {
            // We look at the map data to determine where to spawn players. Chars a-d are designated spawn points, but also towers.
            // Currently capped at 2 players.
            Coordinate player1Spawn = Map.getInstance().GetSpawnPoint(1);
            this.playerCollection.Add(new Player(1, false));  // player 1 is human
            Monster blackMage = new BlackMage(Convert.ToInt32(player1Spawn.x), Convert.ToInt32(player1Spawn.y), this.playerCollection[0]);

            Coordinate player2Spawn = Map.getInstance().GetSpawnPoint(2);
            this.playerCollection.Add(new Player(2, true));  // player 2 is ai
            Monster heavyKnight = new HeavyKnight(Convert.ToInt32(player2Spawn.x), Convert.ToInt32(player2Spawn.y), this.playerCollection[1]);
            MonsterManager.getInstance().monsterCollection.Add(blackMage);
            MonsterManager.getInstance().monsterCollection.Add(heavyKnight);
        }
        
        public static PlayerManager getInstance()
        {
            return instance;
        }
    }
}
