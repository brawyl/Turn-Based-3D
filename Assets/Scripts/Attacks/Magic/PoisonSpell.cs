using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSpell : BaseAttack
{
    public PoisonSpell()
    {
        attackName = "Poison";
        attackDescription = "Basic Poison spell which poisons the target.";
        attackDamage = 5f;
        attackCost = 5f;
    }
}
