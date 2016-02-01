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
        public AI ai;

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

                this.summonOptions = new List<Monster>()
                {
                    new BlueDragon(0, 0, this),
                    new Archer(0, 0, this),
                    new Assassin(0, 0, this)
                };
            }
            else
            {
                this.flag = Assets.playerTwoSymbol;
                this.symbolColor = Color.Red;
                this.isAi = true;
                this.name = "CPU";

                this.summonOptions = new List<Monster>()
                {
                    new HeavyKnight(0, 0, this)
                };
            }
        }

        public void StartTurn()
        {
            // Give the player mana based on how many towers they control
            this.mana += 20 + (this.towersOwned * 10);

            // Reset all moster movement points
            foreach (Monster monster in this.monsterCollection)
            {
                monster.remainingMovement = monster.movement;
            }

            if (this.ai != null)
            {
                this.ai.StartTurn();
            }
        }
    }

    public class PlayerManager
    {
        public List<Player> playerCollection;
        static PlayerManager instance = new PlayerManager();
        public Player currentPlayer;
        public int turnCount = 1;

        public static PlayerManager getInstance()
        {
            return instance;
        }

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
            Monster blackMage = MonsterManager.getInstance().Spawn(typeof(BlackMage), Convert.ToInt32(player1Spawn.x), Convert.ToInt32(player1Spawn.y), this.playerCollection[0], true);
            player1.summoner = blackMage;
            foreach (Tower tower in Map.getInstance().towers)
            {
                if (tower.X == player1Spawn.x && tower.Y == player1Spawn.y)
                    tower.Capture(blackMage, false);
            }

            Coordinate player2Spawn = Map.getInstance().GetSpawnPoint(2);
            Player player2 = new Player(2, true);
            this.playerCollection.Add(player2);  // player 2 is ai
            Monster blackKnight = MonsterManager.getInstance().Spawn(typeof(BlackKnight), Convert.ToInt32(player2Spawn.x), Convert.ToInt32(player2Spawn.y), this.playerCollection[1], true);
            player2.summoner = blackKnight;
            foreach (Tower tower in Map.getInstance().towers)
            {
                if (tower.X == player2Spawn.x && tower.Y == player2Spawn.y)
                    tower.Capture(blackKnight, false);
            }

            foreach (Player player in this.playerCollection)
            {
                if (player.isAi)
                {
                    AI ai = new AI(player);
                    player.ai = ai;
                }
            }

            // Player 1 goes first
            this.currentPlayer = player1;

            UI.getInstance().monsterSummonDialog = new MonsterSummonDialog();
        }

        public void EndTurn()
        {
            // Make it the next players turn
            this.currentPlayer = this.playerCollection[(this.playerCollection.IndexOf(this.currentPlayer) + 1) % this.playerCollection.Count];
            StartTurn();
        }

        public void StartTurn()
        {
            EventsManager.getInstance().RecordEvent(EventsManager.Event.START_TURN);
            UI.getInstance().monsterSummonDialog.BuildControls(this.currentPlayer);
            EventsManager.getInstance().CurrentScene = EventsManager.Scene.OVERWORLD;

            this.currentPlayer.StartTurn();
        }
    }
}
