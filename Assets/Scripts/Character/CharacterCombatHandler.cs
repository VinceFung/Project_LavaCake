using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterCombatHandler : MonoBehaviour
{
    public Entity entity;
    public int MaxHeat = 6;
    public List<NpcCombatAI> Opponents = new List<NpcCombatAI>();
    public List<NpcCombatAI> currentlyAttacking = new List<NpcCombatAI>();

    void Update()
    {
        currentlyAttacking.Clear();
        if (Opponents.Count > 0)
        {
            int currentHeat = 0;
            int lowestHeat = 999999;
            List<NpcCombatAI> npcsToRemove = new List<NpcCombatAI>();
            foreach (NpcCombatAI npc in Opponents)
            {
                if (npc == null)
                {
                    npcsToRemove.Add(npc);
                }
                else if(npc.Target != this)
                {
                    npcsToRemove.Add(npc);
                }
                else
                {
                    if (npc.heat < lowestHeat)
                    {
                        lowestHeat = npc.heat;
                    }
                }
            }

            foreach (var item in npcsToRemove)
            {
                Opponents.Remove(item);
            }

            while (currentHeat < MaxHeat && lowestHeat + currentHeat <= MaxHeat)
            {
                lowestHeat = 999999;
                float highestAttackScore = -99999f;
                NpcCombatAI highestAttackScoreNpc = null;
                foreach (NpcCombatAI npc in Opponents)
                {
                    if (currentHeat + npc.heat <= MaxHeat && !currentlyAttacking.Contains(npc) && npc.attackScore > highestAttackScore)
                    {
                        highestAttackScore = npc.attackScore;
                        highestAttackScoreNpc = npc;
                    }
                }

                if (highestAttackScoreNpc != null)
                {
                    currentlyAttacking.Add(highestAttackScoreNpc);
                    currentHeat += highestAttackScoreNpc.heat;
                }

                foreach (NpcCombatAI npc in Opponents)
                {
                    if (npc.heat < lowestHeat && !currentlyAttacking.Contains(npc))
                    {
                        lowestHeat = npc.heat;
                    }
                }
            }
        }
    }
}