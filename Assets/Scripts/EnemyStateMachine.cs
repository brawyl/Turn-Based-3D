using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private BattleStateMachine battleSM;
    public BaseEnemy enemy;

    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    private float currCooldown = 0f;
    private float maxCooldown = 5f;
    //this gameobject
    private Vector3 startPosition;

    //time for action
    private bool actionStarted = false;
    private GameObject actionTarget;

    void Start()
    {
        currentState = TurnState.PROCESSING;
        battleSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startPosition = transform.position;
    }

    void Update()
    {
        //Debug.Log(currentState);

        switch (currentState)
        {
            case TurnState.PROCESSING:
                UpdateProgressBar();
                break;
            case TurnState.CHOOSEACTION:
                ChooseAction();
                currentState = TurnState.WAITING;
                break;
            case TurnState.WAITING:

                break;
            case TurnState.ACTION:
                StartCoroutine(TimeForAction());
                break;
            case TurnState.DEAD:

                break;
        }
    }

    void UpdateProgressBar()
    {
        currCooldown = currCooldown + Time.deltaTime;

        if (currCooldown >= maxCooldown)
        {
            currentState = TurnState.CHOOSEACTION;
        }
    }

    void ChooseAction()
    {
        HandleTurn myAttack = new HandleTurn();
        myAttack.attacker = enemy.name;
        myAttack.attackerGameObject = this.gameObject;
        myAttack.targetGameObject = battleSM.heroesInBattle[ Random.Range(0, battleSM.heroesInBattle.Count) ];

        battleSM.CollectActions(myAttack);
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;
        //animate the enemy near the hero to attack
        Vector3 targetPosition = actionTarget.transform.position;
        while(MoveTowardsTarget(targetPosition))
        {

        }

        //wait

        //do damage

        //animate back to start position

        //remove this performer from the list in battleSM

        //reset the battleSM

        actionStarted = false;
        //reset this enemy state
        currCooldown = 0f;
        currentState = TurnState.PROCESSING;
    }

    private bool MoveTowardsTarget(Vector3 target)
    {
        return true;
    }
}
