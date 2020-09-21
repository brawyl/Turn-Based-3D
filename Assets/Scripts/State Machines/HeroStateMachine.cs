﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStateMachine : MonoBehaviour
{
    private BattleStateMachine battleSM;
    public BaseHero hero;

    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;
    //for the progress bar
    private float currCooldown = 0f;
    private float maxCooldown = 5f;
    public Image ProgressBar;
    public GameObject selector;
    //IEnumerator
    public GameObject actionTarget;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 10f;

    void Start()
    {
        startPosition = transform.position;
        //can use speed stat instead of 2.5f
        currCooldown = Random.Range(0, 2.5f);
        selector.SetActive(false);
        battleSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;
    }

    void Update()
    {
        //Debug.Log(currentState);

        switch (currentState)
        {
            case TurnState.PROCESSING:
                UpdateProgressBar();
                break;
            case TurnState.ADDTOLIST:
                battleSM.heroesToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;
            case TurnState.WAITING:
                //idle
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
        float calcCooldown = currCooldown / maxCooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calcCooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);

        if (currCooldown >= maxCooldown)
        {
            currentState = TurnState.ADDTOLIST;
        }
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;
        //animate the enemy near the target to attack
        Vector3 targetPosition = new Vector3(actionTarget.transform.position.x + 1.5f, actionTarget.transform.position.y, actionTarget.transform.position.z);
        while (MoveTowardsTarget(targetPosition)) { yield return null; }

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
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    public void TakeDamage(float damageAmount)
    {
        hero.currHP -= damageAmount;
        if (hero.currHP <= 0)
        {
            currentState = TurnState.DEAD;
        }
    }
}
