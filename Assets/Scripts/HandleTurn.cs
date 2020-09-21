using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandleTurn
{
    public string attacker; //name of attacker
    public string type; //enemy or hero
    public GameObject attackerGameObject; //who is attacking
    public GameObject targetGameObject; //who is attacked

    //which attack is performed
    public BaseAttack chosenAttack;
}
