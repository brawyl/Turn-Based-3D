﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public enum HeroGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        INPUT2,
        DONE
    }

    public HeroGUI heroInput;

    public List<GameObject> heroesToManage = new List<GameObject>();
    private HandleTurn heroChoice;

    public GameObject enemyButton;
    public Transform spacer;

    public GameObject attackPanel;
    public GameObject targetSelectPanel;

    // Start is called before the first frame update
    void Start()
    {
        battleState = PerformAction.WAIT;
        enemiesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        heroesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
        heroInput = HeroGUI.ACTIVATE;

        attackPanel.SetActive(false);
        targetSelectPanel.SetActive(false);

        EnemyButtons();
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
                    HeroStateMachine heroSM = performer.GetComponent<HeroStateMachine>();
                    heroSM.actionTarget = performList[0].targetGameObject;
                    heroSM.currentState = HeroStateMachine.TurnState.ACTION;
                }
                battleState = PerformAction.PERFORMACTION;
                break;

            case PerformAction.PERFORMACTION:
                //idle
                break;
        }

        switch (heroInput)
        {
            case HeroGUI.ACTIVATE:
                if (heroesToManage.Count > 0)
                {
                    heroesToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    heroChoice = new HandleTurn();

                    attackPanel.SetActive(true);
                    heroInput = HeroGUI.WAITING;
                }
                break;

            case HeroGUI.WAITING:
                //idle
                break;

            case HeroGUI.DONE:
                HeroInputDone();
                break;
        }
    }

    public void CollectActions(HandleTurn action)
    {
        performList.Add(action);
    }

    void EnemyButtons()
    {
        foreach(GameObject enemy in enemiesInBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

            EnemyStateMachine currEnemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.GetComponentInChildren<Text>();
            buttonText.text = currEnemy.name;

            button.enemyPrefab = enemy;

            newButton.transform.SetParent(spacer, false);
        }
    }

    public void InputAction()
    {
        heroChoice.attacker = heroesToManage[0].name;
        heroChoice.attackerGameObject = heroesToManage[0];
        heroChoice.type = "Hero";

        attackPanel.SetActive(false);
        targetSelectPanel.SetActive(true);
    }

    public void InputTarget(GameObject target)
    {
        heroChoice.targetGameObject = target;
        heroInput = HeroGUI.DONE;
    }

    void HeroInputDone()
    {
        performList.Add(heroChoice);
        targetSelectPanel.SetActive(false);
        heroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        heroesToManage.RemoveAt(0);
        heroInput = HeroGUI.ACTIVATE;
    }
}
