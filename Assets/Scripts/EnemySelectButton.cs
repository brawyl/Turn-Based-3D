﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelectButton : MonoBehaviour
{
    public GameObject EnemyPrefab;

    public void SelectEnemy()
    {
        //save input enemy prefab
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
    }
}
