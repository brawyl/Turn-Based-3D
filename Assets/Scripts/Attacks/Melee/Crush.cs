using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crush : BaseAttack
{
    public Crush()
    {
        attackName = "Crush";
        attackDescription = "Collapse the target like a building collapsing in on itself.";
        attackDamage = 10f;
        attackCost = 0f;
    }
}
