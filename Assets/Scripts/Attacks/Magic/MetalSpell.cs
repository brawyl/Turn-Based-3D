using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalSpell : BaseAttack
{
    public MetalSpell()
    {
        attackName = "Metal";
        attackDescription = "Basic Metal spell which cuts into the target.";
        attackDamage = 20f;
        attackCost = 5f;
    }
}
