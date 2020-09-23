using System.Collections;
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

    public GameObject actionPanel;
    public GameObject targetSelectPanel;
    public GameObject magicPanel;

    //magic attack
    public Transform actionSpacer;
    public Transform magicSpacer;
    public GameObject actionButton;
    public GameObject magicActionButton;
    private List<GameObject> attackButtons = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        battleState = PerformAction.WAIT;
        enemiesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        heroesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
        heroInput = HeroGUI.ACTIVATE;

        actionPanel.SetActive(false);
        targetSelectPanel.SetActive(false);
        magicPanel.SetActive(false);

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
                    for (int i=0; i<heroesInBattle.Count; i++)
                    {
                        if (performList[0].targetGameObject == heroesInBattle[i])
                        {
                            enemySM.actionTarget = performList[0].targetGameObject;
                            enemySM.currentState = EnemyStateMachine.TurnState.ACTION;
                            break;
                        }
                        else
                        {
                            performList[0].targetGameObject = heroesInBattle[Random.Range(0, heroesInBattle.Count)];
                            enemySM.actionTarget = performList[0].targetGameObject;
                            enemySM.currentState = EnemyStateMachine.TurnState.ACTION;
                        }
                    }
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

                    actionPanel.SetActive(true);
                    CreateAttackButtons();

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

        actionPanel.SetActive(false);
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

        //clean up attack panel
        foreach(GameObject atkBtn in attackButtons)
        {
            Destroy(atkBtn);
        }
        attackButtons.Clear();

        heroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        heroesToManage.RemoveAt(0);
        heroInput = HeroGUI.ACTIVATE;
    }

    void CreateAttackButtons()
    {
        GameObject attackButton = Instantiate(actionButton) as GameObject;
        Text attackButtonText = attackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        attackButtonText.text = "Attack";
        attackButton.GetComponent<Button>().onClick.AddListener( () => InputAction() );
        attackButton.transform.SetParent(actionSpacer, false);
        attackButtons.Add(attackButton);

        GameObject magicButton = Instantiate(actionButton) as GameObject;
        Text magicButtonText = magicButton.transform.Find("Text").gameObject.GetComponent<Text>();
        magicButtonText.text = "Magic";
        magicButton.GetComponent<Button>().onClick.AddListener(() => InputMagicAction());
        magicButton.transform.SetParent(actionSpacer, false);
        attackButtons.Add(magicButton);

        if (heroesToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks.Count > 0)
        {
            foreach(BaseAttack magicAttack in heroesToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks)
            {
                GameObject spellButton = Instantiate(magicActionButton) as GameObject;
                Text spellButtonText = spellButton.transform.Find("Text").gameObject.GetComponent<Text>();
                spellButtonText.text = magicAttack.attackName;

                AttackButton atkBtn = spellButton.GetComponent<AttackButton>();
                atkBtn.magicAttackToPerform = magicAttack;
                spellButton.transform.SetParent(magicSpacer, false);
                attackButtons.Add(spellButton);
            }
        }
        else
        {
            magicButton.GetComponent<Button>().interactable = false;
        }
    }

    public void InputMagicAction()
    {
        actionPanel.SetActive(false);
        magicPanel.SetActive(true);
    }

    public void InputMagic(BaseAttack chosenMagic)
    {
        heroChoice.attacker = heroesToManage[0].name;
        heroChoice.attackerGameObject = heroesToManage[0];
        heroChoice.type = "Hero";

        heroChoice.chosenAttack = chosenMagic;
        magicPanel.SetActive(false);
        targetSelectPanel.SetActive(true);
    }
}
