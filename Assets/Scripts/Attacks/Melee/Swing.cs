using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swing : BaseAttack
{
    public Swing()
    {
        attackName = "Swing";
        attackDescription = "A swinging attack that smashes into the target.";
        attackDamage = 15f;
        attackCost = 0f;
    }
}
