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

        public Player(int playerNumber, bool isAI)
        {
            monsterCollection = new List<Monster>();

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

        public static PlayerManager getInstance()
        {
            return instance;
        }
    }
}
