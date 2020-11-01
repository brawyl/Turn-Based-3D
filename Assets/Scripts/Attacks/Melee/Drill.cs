using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : BaseAttack
{
    public Drill()
    {
        attackName = "Drill";
        attackDescription = "Drill forward horizontally into the target like a geyser.";
        attackDamage = 10f;
        attackCost = 0f;
    }
}
