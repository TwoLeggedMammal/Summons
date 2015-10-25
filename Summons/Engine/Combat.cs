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
        double fightLength = 2.0;
        double elapsedTime = 0.0;
        double attackFrequency = 0.0;
        double attackTimer = 0.0;
        Random random;

        public static Combat getInstance()
        {
            return instance;
        }

        private Combat() 
        {
            random = new Random();
        }

        public void FightItOut(Monster first, Monster second)
        {
            this.firstCombatant = first;
            this.secondCombatant = second;
            this.elapsedTime = 0.0;
            this.attacks = CalculateAttacks();
            this.attackFrequency = fightLength / attacks.Count;
            this.attackTimer = 0.0;
        }

        Queue<Tuple<double, Monster>> CalculateAttacks()
        {
            // Figure out the timings of each attack and by whom it was struck
            // The first attacker gets their preferance at to if this is a melee or ranged battle
            Queue<double> firstAttacks = new Queue<double>();
            Queue<double> secondAttacks = new Queue<double>();
            Queue<Tuple<double, Monster>> finalAttacks = new Queue<Tuple<double,Monster>>();
            int firstCombatantAttacks = firstCombatant.prefersMelee ? this.firstCombatant.meleeAttacks : this.firstCombatant.rangedAttacks;
            int secondCombatantAttacks = firstCombatant.prefersMelee ? this.secondCombatant.meleeAttacks : this.secondCombatant.rangedAttacks;

            for (int i = 1; i <= firstCombatantAttacks; i++)
            {
                firstAttacks.Enqueue((fightLength / firstCombatantAttacks) * i);
            }

            for (int i = 1; i <= secondCombatantAttacks; i++)
            {
                secondAttacks.Enqueue((fightLength / secondCombatantAttacks) * i);
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

        // Returns false if the blow was a killing one, which ends combat immediately
        public bool PerformAttack(Tuple<double, Monster> thisAttack)
        {
            Monster attacker = thisAttack.Item2 == firstCombatant ? firstCombatant : secondCombatant;
            Monster defender = thisAttack.Item2 == firstCombatant ? secondCombatant : firstCombatant;

            // Here is the real combat math
            int accuracy = firstCombatant.prefersMelee ? attacker.meleeAccuracy : attacker.rangedAccuracy;
            int AP = firstCombatant.prefersMelee ? attacker.meleeAP : attacker.rangedAP;
            bool missed = random.Next(0, 100) > accuracy;
            bool crit = random.Next(0, 100) <= attacker.critRate;
            int damage = missed? 0 : crit? AP * 2 : AP - defender.armor;
            String actionText = String.Format("-{0} HP", damage.ToString());

            if (missed)
            {
                actionText = String.Format("MISSED!");
            }
            else if (crit)
            {
                actionText = String.Format("CRITICAL!!! -{0} HP", damage.ToString());
            }
            else if (damage == 0)
            {
                actionText = String.Format("ABSORBED!");
            }

            defender.ReceiveDamage(damage);
            UI.getInstance().ShowMessage(actionText, FloatingMessage.TransitionType.FLOATING, defender);

            if (defender.HP <= 0)
            {
                AwardXP(attacker, defender);
                defender.Die();
                return false;
            }

            return true;
        }

        public void Update(double timeSinceLastFrame)
        {
            elapsedTime += timeSinceLastFrame;
            attackTimer += timeSinceLastFrame;

            if (attacks.Count > 0)
            {
                if (attackTimer > attackFrequency)
                {
                    
                    // Perform the attack, and if false, then the defender has been killed
                    if (!PerformAttack(attacks.Dequeue()))
                    {
                        this.elapsedTime = this.fightLength;
                        attacks.Clear();
                    }
                    attackTimer = 0.0;
                }
            }

            if (this.elapsedTime > this.fightLength && attacks.Count == 0)
            {
                EventsManager.getInstance().RecordEvent(EventsManager.Event.BATTLE_COMPLETED);
            }
        }

        public void AwardXP(Monster winner, Monster loser)
        {
            // TODO: Implement better XP logic
            winner.XP = winner.XP + 10 > winner.maxXP ? winner.maxXP : winner.XP + 10;
        }
    }
}
