using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Summons.Engine
{
    class Player
    {
        public Texture2D flag;
        public List<Monster> monsterCollection;
        public Color symbolColor;

        public Player(int playerNumber)
        {
            monsterCollection = new List<Monster>();

            if (playerNumber == 1)
            {
                this.flag = Assets.playerOneSymbol;
                this.symbolColor = Color.Blue;
                Console.WriteLine("just set the flag");
            }
            else
            {
                Console.WriteLine("just set the other flag");
                this.flag = Assets.playerTwoSymbol;
                this.symbolColor = Color.Red;
            }
        }
    }
}
