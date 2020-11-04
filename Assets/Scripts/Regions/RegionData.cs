using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionData : MonoBehaviour
{
    public int maxAmountEnemies = 4;
    public string battleScene;
    public List<GameObject> possibleEnemies = new List<GameObject>();
    public GameObject backgroundType;
    public Material floorType;
}
