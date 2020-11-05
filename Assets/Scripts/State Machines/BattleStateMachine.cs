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
        PERFORMACTION,
        CHECKALIVE,
        WIN,
        LOSE
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
    public GameObject resultScreen;
    public GameObject ground;
    public Camera mainCamera;
    public string environmentElement;

    //magic attacks
    public Transform actionSpacer;
    public Transform magicSpacer;
    public GameObject actionButton;
    public GameObject magicActionButton;
    private List<GameObject> attackButtons = new List<GameObject>();

    //enemy attacks
    private List<GameObject> enemyBtns = new List<GameObject>();

    //spawn points
    public List<Transform> spawnPoints = new List<Transform>();

    void Awake()
    {
        List<string> enemyNames = new List<string>();
        string[] enemyLetters = { " A", " B", " C", " D" };
        for (int i=0; i<GameManager.instance.enemyAmount; i++)
        {
            GameObject newEnemy = Instantiate(GameManager.instance.enemiesToBattle[i], spawnPoints[i].position, Quaternion.identity) as GameObject;
            string enemyName = newEnemy.GetComponent<EnemyStateMachine>().enemy.theName;

            //rename duplicate enemy names
            if (enemyNames.IndexOf(enemyName) > -1)
            {
                int letterIndex = 0;
                for (int j = 0; j < i; j++)
                {
                    if (enemyNames[j].Contains(enemyName))
                    {
                        enemiesInBattle[j].name = enemyName + enemyLetters[letterIndex];
                        enemiesInBattle[j].GetComponent<EnemyStateMachine>().enemy.theName = enemyName + enemyLetters[letterIndex];
                        letterIndex++;
                    }
                }
                enemyName = enemyName + enemyLetters[letterIndex];
            }

            newEnemy.name = enemyName;
            newEnemy.GetComponent<EnemyStateMachine>().enemy.theName = newEnemy.name;
            enemiesInBattle.Add(newEnemy);
            enemyNames.Add(enemyName);
        }

        //load region environment settings
        GameObject bgImg = Instantiate(GameManager.instance.currRegion.backgroundType);
        bgImg.GetComponent<Canvas>().worldCamera = mainCamera;
        Material floorMaterial = Instantiate(GameManager.instance.currRegion.floorType);

        ground.GetComponent<MeshRenderer>().material = floorMaterial;

        if (floorMaterial.name.Contains("Wood"))
        {
            environmentElement = "WOOD";
        }
        else if (floorMaterial.name.Contains("Water"))
        {
            environmentElement = "WATER";
        }
        else
        {
            environmentElement = "";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        battleState = PerformAction.WAIT;
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
                    battleState = PerformAction.PERFORMACTION;
                }
                if (performList[0].type == "Hero")
                {
                    HeroStateMachine heroSM = performer.GetComponent<HeroStateMachine>();
                    heroSM.actionTarget = performList[0].targetGameObject;
                    if (heroSM.currentState != HeroStateMachine.TurnState.DEAD)
                    {
                        heroSM.currentState = HeroStateMachine.TurnState.ACTION;
                        battleState = PerformAction.PERFORMACTION;
                    }
                    else
                    {
                        performList.Remove(performList[0]);
                        battleState = PerformAction.CHECKALIVE;
                    }
                }
                break;

            case PerformAction.PERFORMACTION:
                //idle
                break;
            case PerformAction.CHECKALIVE:
                if (heroesInBattle.Count < 1)
                {
                    battleState = PerformAction.LOSE;
                }
                else if (enemiesInBattle.Count < 1)
                {
                    battleState = PerformAction.WIN;
                }
                else
                {
                    ClearAttackPanel();
                    heroInput = HeroGUI.ACTIVATE;
                }
                break;

            case PerformAction.WIN:
                for (int i = 0; i < heroesInBattle.Count; i++)
                {
                    heroesInBattle[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
                }

                resultScreen.GetComponentInChildren<Text>().text = "YOU WIN";
                resultScreen.SetActive(true);

                StartCoroutine(WinBattle());
                break;

            case PerformAction.LOSE:
                resultScreen.GetComponentInChildren<Text>().text = "YOU LOSE";
                resultScreen.SetActive(true);

                StartCoroutine(LoseBattle());
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

    public void EnemyButtons()
    {
        //cleanup
        foreach(GameObject enemyBtn in enemyBtns)
        {
            Destroy(enemyBtn);
        }
        enemyBtns.Clear();
        //create buttons
        foreach(GameObject enemy in enemiesInBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

            EnemyStateMachine currEnemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.GetComponentInChildren<Text>();
            buttonText.text = currEnemy.name;

            button.enemyPrefab = enemy;

            newButton.transform.SetParent(spacer, false);
            enemyBtns.Add(newButton);
        }
    }

    public void InputAction()
    {
        heroChoice.attacker = heroesToManage[0].name;
        heroChoice.attackerGameObject = heroesToManage[0];
        heroChoice.type = "Hero";
        heroChoice.chosenAttack = heroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[0];

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
        if (heroesToManage.Count > 0)
        {
            performList.Add(heroChoice);

            //clean up attack panel
            ClearAttackPanel();
        
            heroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
            heroesToManage.RemoveAt(0);
            heroInput = HeroGUI.ACTIVATE;
        }
    }

    void ClearAttackPanel()
    {
        targetSelectPanel.SetActive(false);
        actionPanel.SetActive(false);
        magicPanel.SetActive(false);

        foreach (GameObject atkBtn in attackButtons)
        {
            Destroy(atkBtn);
        }
        attackButtons.Clear();
    }

    void CreateAttackButtons()
    {
        GameObject attackButton = Instantiate(actionButton) as GameObject;
        Text attackButtonText = attackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        attackButtonText.text = "ATTACK";
        attackButton.GetComponent<Button>().onClick.AddListener( () => InputAction() );
        attackButton.transform.SetParent(actionSpacer, false);
        attackButtons.Add(attackButton);

        GameObject magicButton = Instantiate(actionButton) as GameObject;
        Text magicButtonText = magicButton.transform.Find("Text").gameObject.GetComponent<Text>();
        magicButtonText.text = "MAGIC";
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

    IEnumerator WinBattle()
    {
        yield return new WaitForSeconds(2);

        resultScreen.SetActive(false);

        GameManager.instance.LoadSceneAfterBattle();
        GameManager.instance.gameState = GameManager.GameStates.WORLD_STATE;
        GameManager.instance.enemiesToBattle.Clear();
    }

    IEnumerator LoseBattle()
    {
        yield return new WaitForSeconds(2);

        resultScreen.SetActive(false);

        GameManager.instance.nextSpawnPoint = "SP_EnterTown";
        GameManager.instance.sceneToLoad = "TownScene";
        GameManager.instance.lastScene = "TownScene";
        GameManager.instance.LoadSceneAfterBattle();
        GameManager.instance.gameState = GameManager.GameStates.WORLD_STATE;
        GameManager.instance.enemiesToBattle.Clear();
    }
}
