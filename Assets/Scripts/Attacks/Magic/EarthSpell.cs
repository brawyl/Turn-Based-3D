using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthSpell : BaseAttack
{
    public EarthSpell()
    {
        attackName = "Earth";
        attackDescription = "Basic Earth spell which splits the ground beneath the target.";
        attackDamage = 20f;
        attackCost = 10f;
    }
}
