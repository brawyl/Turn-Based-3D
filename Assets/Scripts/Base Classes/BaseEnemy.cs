using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseEnemy : BaseClass
{
    public enum Type
    {
        WOOD,
        FIRE,
        EARTH,
        METAL,
        WATER
    }

    public enum Rarity
    {
        COMMON,
        UNCOMMON,
        RARE,
        SUPERRARE
    }

    public Type enemyType;
    public Rarity rarity;

    public int stamina;
    public int intellect;
    public int dexterity;
    public int agility;

}
