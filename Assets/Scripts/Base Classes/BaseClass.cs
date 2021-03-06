﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseClass
{
    public string theName;

    public float baseHP;
    public float currHP;

    public float baseATK;
    public float currATK;
    public float baseDEF;
    public float currDEF;
    public float baseREGEN;
    public float currREGEN;
    public float baseSPD;
    public float currSPD;

    public List<BaseAttack> attacks = new List<BaseAttack>();
}
