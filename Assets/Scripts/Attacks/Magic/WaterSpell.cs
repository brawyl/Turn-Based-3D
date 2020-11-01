using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpell : BaseAttack
{
    public WaterSpell()
    {
        attackName = "Water";
        attackDescription = "Basic Water spell which drills into the target.";
        attackDamage = 20f;
        attackCost = 5f;
    }
}
