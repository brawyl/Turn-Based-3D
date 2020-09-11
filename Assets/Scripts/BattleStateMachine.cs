using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStateMachine : MonoBehaviour
{
    public enum PerformAction
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION
    }

    public PerformAction battleState;

    public List<HandleTurn> performList = new List<HandleTurn>();
    public List<GameObject> heroesInBattle = new List<GameObject>();
    public List<GameObject> enemiesInBattle = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        battleState = PerformAction.WAIT;
        enemiesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        heroesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
    }

    // Update is called once per frame
    void Update()
    {
        switch (battleState)
        {
            case PerformAction.WAIT:
                if (performList.Count > 0)
                {
                    battleState = PerformAction.PERFORMACTION;
                }
                break;
            case PerformAction.TAKEACTION:

                break;
            case PerformAction.PERFORMACTION:

                break;
        }
    }

    public void CollectActions(HandleTurn action)
    {
        performList.Add(action);
    }
}
