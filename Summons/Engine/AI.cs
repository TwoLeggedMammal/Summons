using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Summons.Engine
{
    public class AI
    {
        Player player;
        double delay;

        public AI(Player player)
        {
            this.player = player;
            delay = 0.0;
        }
        
        public void DoStuff(double timeSinceLastFrame)
        {
            // Order of operations
            // 1. Summon any monsters
            // 2. Find move for all monsters
            // 3. End turn

            if (delay >= 0.0)
            {
                delay -= timeSinceLastFrame;
            }
            else if (SummonMonsters())
            {
                Delay();
            }
            else
            {
                bool moved = false;

                foreach (Monster monster in this.player.monsterCollection)
                {
                    if (FindMove(monster))
                    {
                        moved = true;
                        Delay();
                        break;
                    }
                }

                if (!moved)
                {
                    EndTurn();
                }
            }
        }

        bool SummonMonsters()
        {
            // Have mana? Spend it on random monsters
            return false;
        }

        bool FindMove(Monster monster)
        {
            // Grab move options for monster

            // Decide on priority
            // 1. Occupy tower
            // 2. Kill enemies
            // 3. Move toward enemy summoner
            
            return false;
        }

        public void Delay()
        {
            // We don't want to make actions too quickly to be seen
            this.delay += Settings.AI_ACTION_DELAY; 
        }

        void EndTurn()
        {
            PlayerManager.getInstance().EndTurn();
        }
    }
}
