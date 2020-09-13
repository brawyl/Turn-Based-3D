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
                    battleState = PerformAction.TAKEACTION;
                }
                break;
            case PerformAction.TAKEACTION:
                GameObject performer = GameObject.Find(performList[0].attacker);
                if (performList[0].type == "Enemy")
                {
                    EnemyStateMachine enemySM = performer.GetComponent<EnemyStateMachine>();
                    enemySM.actionTarget = performList[0].targetGameObject;
                    enemySM.currentState = EnemyStateMachine.TurnState.ACTION;
                }
                if (performList[0].type == "Hero")
                {

                }
                battleState = PerformAction.PERFORMACTION;
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
