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
    private float maxCooldown = 8f;
    private Image progressBar;
    private Image healthBar;
    private Image profileImage;
    public GameObject selector;
    public GameObject damageText;
    public GameObject elementDamage;

    //IEnumerator
    public GameObject actionTarget;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 15f;

    private bool alive = true;
    private bool regen = true;
    //hero panel
    private HeroPanelStats stats;
    public GameObject heroPanel;
    private Transform heroPanelSpacer;

    public bool activeTurn = false;
    public float delayValue = 0f;
    private Text timerText;

    void Start()
    {
        //find spacer
        heroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");

        //create panel and fill in info
        CreateHeroPanel();

        startPosition = transform.position;
        selector.SetActive(false);
        damageText.SetActive(false);
        battleSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;
    }

    void Update()
    {
        if (!alive) { currentState = TurnState.DEAD; }
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
                if (regen)
                {
                    HealDamage(hero.currREGEN);
                }
                if (activeTurn)
                {
                    //full opacity text on active turn
                    timerText.color = new Color(255, 255, 255, 1f);
                    UpdateDelayTimer();
                }
                else
                {
                    timerText.color = new Color(255, 255, 255, 0.5f);
                }
                break;

            case TurnState.ACTION:
                StartCoroutine(TimeForAction());
                regen = true;
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
                battleSM.magicPanel.SetActive(false);

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
                    battleSM.UpdateTurnOrder();
                }

                //change color / play death animation
                this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(150, 150, 150, 255);

                battleSM.heroInput = BattleStateMachine.HeroGUI.DONE;

                battleSM.heroesToManage.Remove(this.gameObject);

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
            delayValue = 0f;
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

        string attackingElement = battleSM.performList[0].chosenAttack.attackName.ToUpper();
        bool showParticles = true;
        ParticleSystem elementParticles = gameObject.GetComponentInParent<ParticleSystem>();
        ParticleSystemRenderer elementRender = elementParticles.GetComponent<ParticleSystemRenderer>();
        ParticleSystem damageParticles = elementDamage.GetComponentInParent<ParticleSystem>();
        ParticleSystemRenderer damageRender = damageParticles.GetComponent<ParticleSystemRenderer>();

        switch (attackingElement)
        {
            case "WOOD":
                elementRender.material = new Material(Resources.Load<Material>("WoodParticle"));
                damageRender.material = new Material(Resources.Load<Material>("WoodParticle"));
                break;
            case "FIRE":
                elementRender.material = new Material(Resources.Load<Material>("FireParticle"));
                damageRender.material = new Material(Resources.Load<Material>("FireParticle"));
                break;
            case "EARTH":
                elementRender.material = new Material(Resources.Load<Material>("EarthParticle"));
                damageRender.material = new Material(Resources.Load<Material>("EarthParticle"));
                break;
            case "METAL":
                elementRender.material = new Material(Resources.Load<Material>("MetalParticle"));
                damageRender.material = new Material(Resources.Load<Material>("MetalParticle"));
                break;
            case "WATER":
                elementRender.material = new Material(Resources.Load<Material>("WaterParticle"));
                damageRender.material = new Material(Resources.Load<Material>("WaterParticle"));
                break;
            default:
                showParticles = false;
                break;
        }

        //turn on particle effect
        if (showParticles) { elementParticles.Play(); damageParticles.Stop(); }

        //animate the enemy near the target to attack
        Vector3 targetPosition = new Vector3(actionTarget.transform.position.x + 1.5f, actionTarget.transform.position.y, actionTarget.transform.position.z);
        while (MoveTowardsTarget(targetPosition)) { yield return null; }

        if (showParticles) { damageParticles.Play(); }

        //wait
        yield return new WaitForSeconds(0.5f);

        if (showParticles) { damageParticles.Stop(); }

        //do damage
        DoDamage();

        //turn off particle effect
        if (showParticles) { elementParticles.Stop(); }

        //animate back to start position
        Vector3 firstPosition = startPosition;
        while (MoveTowardsTarget(firstPosition)) { yield return null; }

        //remove this performer from the list in battleSM
        battleSM.performList.RemoveAt(0);

        //reset the battleSM
        if (battleSM.battleState != BattleStateMachine.PerformAction.WIN && battleSM.battleState != BattleStateMachine.PerformAction.LOSE)
        {
            battleSM.battleState = BattleStateMachine.PerformAction.WAIT;
            battleSM.UpdateTurnOrder();
            //reset this state
            float delayFactor = 5f - delayValue;
            currCooldown = Mathf.Clamp(delayFactor, 0, 4);
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
        float calculatedDamage = Mathf.RoundToInt(damageAmount);
        damageText.GetComponent<TextMesh>().text = calculatedDamage.ToString();
        damageText.SetActive(true);
        StartCoroutine(HideDamageText());

        hero.currHP -= calculatedDamage;
        if (hero.currHP <= 0)
        {
            hero.currHP = 0;
            currentState = TurnState.DEAD;
        }
        UpdateHeroPanel();
    }

    public void HealDamage(float damageAmount)
    {
        hero.currHP += damageAmount;
        if (hero.currHP > hero.baseHP)
        {
            hero.currHP = hero.baseHP;
        }
        regen = false;
        UpdateHeroPanel();
    }

    void DoDamage()
    {
        float baseDamage = hero.currATK + battleSM.performList[0].chosenAttack.attackDamage;
        string targetElement = actionTarget.GetComponent<EnemyStateMachine>().enemy.enemyType.ToString().ToUpper();
        string attackingElement = battleSM.performList[0].chosenAttack.attackName.ToUpper();

        float environmentDamage = 1.0f;
        if (attackingElement.Equals(battleSM.environmentElement))
        {
            environmentDamage = 1.2f; //extra 20% dmg on matching element w environment
        }

        float calculatedDamage = baseDamage * elementalDamage(attackingElement, targetElement) * environmentDamage;

        actionTarget.GetComponent<EnemyStateMachine>().TakeDamage(calculatedDamage);
        if (battleSM.performList[0].chosenAttack.attackCost > 0)
        {
            TakeDamage(battleSM.performList[0].chosenAttack.attackCost);
        }
    }

    void CreateHeroPanel()
    {
        heroPanel = Instantiate(heroPanel) as GameObject;
        stats = heroPanel.GetComponent<HeroPanelStats>();
        stats.heroName.text = hero.theName;
        stats.heroHP.text = "HP: " + hero.currHP + "/" + hero.baseHP;
        timerText = stats.delayTimer;
        timerText.text = "0.00";

        currCooldown = Random.Range(1f, Mathf.Clamp(hero.currSPD, 0, 8));

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

    void UpdateDelayTimer()
    {
        delayValue += Time.deltaTime;
        timerText.text = delayValue.ToString("F2");
    }

    IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(1);

        damageText.SetActive(false);
    }

    float elementalDamage(string attack, string target)
    {
        //min, less, normal, more, max
        float[] damageMultiplier = { 0.6f, 0.8f, 1.0f, 1.5f, 2.0f };
        switch (attack)
        {
            case "WOOD":
                if (target.Equals("WOOD")) { return damageMultiplier[2]; }
                else if (target.Equals("FIRE")) { return damageMultiplier[1]; }
                else if (target.Equals("EARTH")) { return damageMultiplier[3]; }
                else if (target.Equals("METAL")) { return damageMultiplier[0]; }
                else if (target.Equals("WATER")) { return damageMultiplier[4]; }
                break;
            case "FIRE":
                if (target.Equals("WOOD")) { return damageMultiplier[4]; }
                else if (target.Equals("FIRE")) { return damageMultiplier[2]; }
                else if (target.Equals("EARTH")) { return damageMultiplier[1]; }
                else if (target.Equals("METAL")) { return damageMultiplier[3]; }
                else if (target.Equals("WATER")) { return damageMultiplier[0]; }
                break;
            case "EARTH":
                if (target.Equals("WOOD")) { return damageMultiplier[0]; }
                else if (target.Equals("FIRE")) { return damageMultiplier[4]; }
                else if (target.Equals("EARTH")) { return damageMultiplier[2]; }
                else if (target.Equals("METAL")) { return damageMultiplier[1]; }
                else if (target.Equals("WATER")) { return damageMultiplier[3]; }
                break;
            case "METAL":
                if (target.Equals("WOOD")) { return damageMultiplier[3]; }
                else if (target.Equals("FIRE")) { return damageMultiplier[0]; }
                else if (target.Equals("EARTH")) { return damageMultiplier[4]; }
                else if (target.Equals("METAL")) { return damageMultiplier[2]; }
                else if (target.Equals("WATER")) { return damageMultiplier[1]; }
                break;
            case "WATER":
                if (target.Equals("WOOD")) { return damageMultiplier[1]; }
                else if (target.Equals("FIRE")) { return damageMultiplier[3]; }
                else if (target.Equals("EARTH")) { return damageMultiplier[0]; }
                else if (target.Equals("METAL")) { return damageMultiplier[4]; }
                else if (target.Equals("WATER")) { return damageMultiplier[2]; }
                break;
        }
        return 1.0f;
    }
}
