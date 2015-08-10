using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Summons.Engine
{
    public class Combat
    {
        static Combat instance = new Combat();
        Monster firstCombatant, secondCombatant;
        Queue<Tuple<double, Monster>> attacks;  // Stores all the times when attacks happen
        double fightLength = 3.0;
        double elapsedTime = 0.0;
        int elapsedAttacks;

        public static Combat getInstance()
        {
            return instance;
        }

        private Combat() {}

        public void FightItOut(Monster first, Monster second)
        {
            this.firstCombatant = first;
            this.secondCombatant = second;
            this.elapsedTime = 0.0;
            this.elapsedAttacks = 0;
            this.attacks = CalculateAttacks();
        }

        Queue<Tuple<double, Monster>> CalculateAttacks()
        {
            // Figure out the timings of each attack and by whom it was struck
            Queue<double> firstAttacks = new Queue<double>();
            Queue<double> secondAttacks = new Queue<double>();
            Queue<Tuple<double, Monster>> finalAttacks = new Queue<Tuple<double,Monster>>();

            for (int i = 1; i <= this.firstCombatant.meleeAttacks; i++)
            {
                firstAttacks.Enqueue((fightLength / firstCombatant.meleeAttacks) * i);
            }

            for (int i = 1; i <= this.secondCombatant.meleeAttacks; i++)
            {
                secondAttacks.Enqueue((fightLength / secondCombatant.meleeAttacks) * i);
            }

            while (firstAttacks.Count > 0 || secondAttacks.Count > 0)
            {
                if (firstAttacks.Count > 0 && secondAttacks.Count == 0)
                {
                    finalAttacks.Enqueue(new Tuple<double,Monster> (firstAttacks.Dequeue(), firstCombatant));
                }
                else if (secondAttacks.Count > 0 && firstAttacks.Count == 0)
                {
                    finalAttacks.Enqueue(new Tuple<double, Monster>(secondAttacks.Dequeue(), secondCombatant));
                }
                else
                {
                    if (firstAttacks.Peek() <= secondAttacks.Peek())
                    {
                        finalAttacks.Enqueue(new Tuple<double, Monster>(firstAttacks.Dequeue(), firstCombatant));
                    }
                    else
                    {
                        finalAttacks.Enqueue(new Tuple<double, Monster>(secondAttacks.Dequeue(), secondCombatant));
                    }
                }
            }

            return finalAttacks;
        }

        public void PerformAttack(Tuple<double, Monster> thisAttack)
        {
            Monster attacker = thisAttack.Item2 == firstCombatant ? firstCombatant : secondCombatant;
            Monster defender = thisAttack.Item2 == firstCombatant ? secondCombatant : firstCombatant;

            // Here is the real combat math
            int damage = attacker.meleeAP - defender.armor;
            String actionText = String.Format("-{0} HP", damage.ToString());
            defender.HP -= damage;
            UI.getInstance().ShowMessage(actionText, FloatingMessage.TransitionType.FLOATING, defender);
        }

        public void Update(double timeSinceLastFrame)
        {
            elapsedTime += timeSinceLastFrame;

            if (attacks.Count > 0)
            {
                if (elapsedTime > attacks.Peek().Item1)
                {
                    PerformAttack(attacks.Dequeue());
                }
            }

            if (this.elapsedTime > this.fightLength && attacks.Count == 0)
            {
                EventsManager.getInstance().RecordEvent(EventsManager.Event.BATTLE_COMPLETED);
            }
        }
    }
}
