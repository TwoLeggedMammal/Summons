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
        public List<Monster> summonOptions;

        public Player(int playerNumber, bool isAI)
        {
            monsterCollection = new List<Monster>();
            this.mana = 100;
            this.towersOwned = 0;
            this.summonOptions = new List<Monster>()
            {
                new BlueDragon(0, 0, this),
                new Archer(0, 0, this),
                new HeavyKnight(0, 0, this),
                new BlackMage(0, 0, this)
            };

            if (playerNumber == 1)
            {
                this.flag = Assets.playerOneSymbol;
                this.symbolColor = Color.DeepSkyBlue;
                this.isAi = false;
                this.name = "Player 1";
                UI.getInstance().monsterSummonDialog = new MonsterSummonDialog(this);
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
            Player player1 = new Player(1, false);
            this.playerCollection.Add(player1);  // player 1 is human
            Monster blackMage = MonsterManager.getInstance().Spawn(typeof(BlackMage), Convert.ToInt32(player1Spawn.x), Convert.ToInt32(player1Spawn.y), this.playerCollection[0]);
            player1.summoner = blackMage;

            Coordinate player2Spawn = Map.getInstance().GetSpawnPoint(2);
            Player player2 = new Player(2, true);
            this.playerCollection.Add(player2);  // player 2 is ai
            Monster heavyKnight = MonsterManager.getInstance().Spawn(typeof(HeavyKnight), Convert.ToInt32(player2Spawn.x), Convert.ToInt32(player2Spawn.y), this.playerCollection[1]);
            player2.summoner = heavyKnight;
        }
        
        public static PlayerManager getInstance()
        {
            return instance;
        }
    }
}
