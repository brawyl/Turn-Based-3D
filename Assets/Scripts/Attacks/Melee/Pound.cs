using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pound : BaseAttack
{
    public Pound()
    {
        attackName = "Pound";
        attackDescription = "Explode outward into the target like a cannon.";
        attackDamage = 10f;
        attackCost = 0f;
    }
}
