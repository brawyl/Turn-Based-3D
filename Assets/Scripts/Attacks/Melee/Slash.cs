using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : BaseAttack
{
    public Slash()
    {
        attackName = "Slash";
        attackDescription = "A quick slashing attack with a bladed weapon.";
        attackDamage = 10f;
        attackCost = 0f;
    }
}
