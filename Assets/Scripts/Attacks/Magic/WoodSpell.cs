using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodSpell : BaseAttack
{
    public WoodSpell()
    {
        attackName = "Wood";
        attackDescription = "Basic Wood spell which smashes into the target.";
        attackDamage = 20f;
        attackCost = 10f;
    }
}
