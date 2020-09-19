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
    private float maxCooldown = 10f;
    //this gameobject
    private Vector3 startPosition;
    public GameObject selector;

    //time for action
    private bool actionStarted = false;
    public GameObject actionTarget;
    private float animSpeed = 10f;

    void Start()
    {
        selector.SetActive(false);
        currCooldown = Random.Range(0, 2.5f);
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
        myAttack.attacker = enemy.theName;
        myAttack.type = "Enemy";
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
        //animate the enemy near the target to attack
        Vector3 targetPosition = new Vector3(actionTarget.transform.position.x - 1.5f, actionTarget.transform.position.y, actionTarget.transform.position.z);
        while(MoveTowardsTarget(targetPosition)) {  yield return null; }

        //wait
        yield return new WaitForSeconds(0.5f);

        //do damage

        //animate back to start position
        Vector3 firstPosition = startPosition;
        while (MoveTowardsTarget(firstPosition)) { yield return null; }

        //remove this performer from the list in battleSM
        battleSM.performList.RemoveAt(0);

        //reset the battleSM
        battleSM.battleState = BattleStateMachine.PerformAction.WAIT;

        //end coroutine
        actionStarted = false;

        //reset this enemy state
        currCooldown = 0f;
        currentState = TurnState.PROCESSING;
    }

    private bool MoveTowardsTarget(Vector3 target)
    {
        return target != ( transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime) );
    }
}
