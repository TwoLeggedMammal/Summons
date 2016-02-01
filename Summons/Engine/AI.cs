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
        Dictionary<Monster, bool> movedThisTurn;

        public AI(Player player)
        {
            this.player = player;
            delay = 0.0;
            movedThisTurn = new Dictionary<Monster, bool>();
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
            int minMana = this.player.summonOptions.Min(x => x.manaCost);

            if (this.player.mana >= minMana)
            {
                Map map = Map.getInstance();
                map.LoadSummonOverlay(this.player);

                // Make sure there are valid places to summon
                if (map.summonLocations.Count > 0)
                {
                    // TODO: randomize on which location the monster is summoned
                    // TODO: randomize which monster is summoned
                    Monster summon = this.player.summonOptions.Where(x => x.manaCost <= this.player.mana).First();

                    MonsterManager.getInstance().Spawn(summon.GetType(), 
                        Convert.ToInt32(map.summonLocations.First().x), 
                        Convert.ToInt32(map.summonLocations.First().y), 
                        this.player);

                    return true;
                }
            }

            return false;
        }

        bool FindMove(Monster monster)
        {
            // Grab move options for monster

            // Decide on priority
            // 1. Occupy tower
            // 2. Kill enemies
            // 3. Move toward enemy summoner
            if (monster.remainingMovement > 0)
            {
                double[,] map = Map.getInstance().MapForAI(monster);
                movedThisTurn[monster] = true;
            }
            
            return false;
        }

        public void Delay()
        {
            // We don't want to make actions too quickly to be seen
            this.delay += Settings.AI_ACTION_DELAY; 
        }

        public void StartTurn()
        {
            Delay();

            movedThisTurn.Clear();
            foreach (Monster monster in this.player.monsterCollection)
            {
                movedThisTurn.Add(monster, false);
            }
        }

        void EndTurn()
        {
            PlayerManager.getInstance().EndTurn();
        }
    }
}
