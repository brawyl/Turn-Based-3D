using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpell : BaseAttack
{
    public FireSpell()
    {
        attackName = "Fire";
        attackDescription = "Basic Fire spell which burns the target.";
        attackDamage = 20f;
        attackCost = 5f;
    }
}
