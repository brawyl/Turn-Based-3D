using System.Collections;
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
    private Image progressBar;
    private Image healthBar;
    private Image profileImage;
    public GameObject selector;
    public GameObject damageText;

    //IEnumerator
    public GameObject actionTarget;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 10f;

    private bool alive = true;
    //hero panel
    private HeroPanelStats stats;
    public GameObject heroPanel;
    private Transform heroPanelSpacer;

    void Start()
    {
        //find spacer
        heroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");

        //create panel and fill in info
        CreateHeroPanel();

        startPosition = transform.position;
        //can use speed stat instead of 2.5f
        currCooldown = Random.Range(0, 2.5f);
        selector.SetActive(false);
        damageText.SetActive(false);
        battleSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;
    }

    void Update()
    {
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
                if (!alive) { return; }

                //change tag to dead
                this.gameObject.tag = "DeadHero";
                //cannot be targeted
                battleSM.heroesInBattle.Remove(this.gameObject);
                //disable the selector
                selector.SetActive(false);

                //reset GUI
                battleSM.actionPanel.SetActive(false);
                battleSM.targetSelectPanel.SetActive(false);

                if (battleSM.heroesInBattle.Count > 0)
                {
                    for (int i = 0; i < battleSM.performList.Count; i++)
                    {
                        if (battleSM.performList[i].attackerGameObject == this.gameObject)
                        {
                            battleSM.performList.Remove(battleSM.performList[i]);
                        }
                        if (battleSM.performList[i].targetGameObject == this.gameObject)
                        {
                            battleSM.performList[i].targetGameObject = battleSM.heroesInBattle[Random.Range(0, battleSM.heroesInBattle.Count)];
                        }
                    }
                }

                //change color / play death animation
                this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(150, 150, 150, 255);

                battleSM.heroInput = BattleStateMachine.HeroGUI.DONE;

                //reset heroInput
                battleSM.battleState = BattleStateMachine.PerformAction.CHECKALIVE;

                alive = false;
                break;
        }
    }

    void UpdateProgressBar()
    {
        currCooldown = currCooldown + Time.deltaTime;
        float calcCooldown = currCooldown / maxCooldown;
        progressBar.transform.localScale = new Vector3(Mathf.Clamp(calcCooldown, 0, 1), progressBar.transform.localScale.y, progressBar.transform.localScale.z);

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
        DoDamage();

        //animate back to start position
        Vector3 firstPosition = startPosition;
        while (MoveTowardsTarget(firstPosition)) { yield return null; }

        //remove this performer from the list in battleSM
        battleSM.performList.RemoveAt(0);

        //reset the battleSM
        if (battleSM.battleState != BattleStateMachine.PerformAction.WIN && battleSM.battleState != BattleStateMachine.PerformAction.LOSE)
        {
            battleSM.battleState = BattleStateMachine.PerformAction.WAIT;
            //reset this enemy state
            currCooldown = 0f;
            currentState = TurnState.PROCESSING;
        }
        else
        {
            currentState = TurnState.WAITING;
        }

        //end coroutine
        actionStarted = false;
    }

    private bool MoveTowardsTarget(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    public void TakeDamage(float damageAmount)
    {
        damageText.GetComponent<TextMesh>().text = damageAmount.ToString();
        damageText.SetActive(true);
        StartCoroutine(HideDamageText());

        hero.currHP -= damageAmount;
        if (hero.currHP <= 0)
        {
            hero.currHP = 0;
            currentState = TurnState.DEAD;
        }
        UpdateHeroPanel();
    }

    void DoDamage()
    {
        float calculatedDamage = hero.currATK + battleSM.performList[0].chosenAttack.attackDamage;
        actionTarget.GetComponent<EnemyStateMachine>().TakeDamage(calculatedDamage);
    }

    void CreateHeroPanel()
    {
        heroPanel = Instantiate(heroPanel) as GameObject;
        stats = heroPanel.GetComponent<HeroPanelStats>();
        stats.heroName.text = hero.theName;
        stats.heroHP.text = "HP: " + hero.currHP + "/" + hero.baseHP;
        
        progressBar = stats.progressBar;
        healthBar = stats.healthBar;
        float calcHealth = hero.currHP / hero.baseHP;
        healthBar.transform.localScale = new Vector3(Mathf.Clamp(calcHealth, 0, 1), healthBar.transform.localScale.y, healthBar.transform.localScale.z);

        profileImage = stats.profileImage;
        string heroImage = "profile_" + hero.theName;
        profileImage.sprite = Resources.Load<Sprite>(heroImage);
        heroPanel.transform.SetParent(heroPanelSpacer, false);
    }

    void UpdateHeroPanel()
    {
        stats.heroHP.text = "HP: " + hero.currHP + "/" + hero.baseHP;
        float calcHealth = hero.currHP / hero.baseHP;
        healthBar.transform.localScale = new Vector3(Mathf.Clamp(calcHealth, 0, 1), healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(1);

        damageText.SetActive(false);
    }
}
