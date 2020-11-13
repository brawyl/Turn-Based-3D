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
    private float maxCooldown = 8f;

    //this gameobject
    private Vector3 startPosition;
    public GameObject selector;
    public GameObject damageText;
    public GameObject critText;
    public GameObject weakText;
    public GameObject resistText;
    public GameObject stackText;

    //time for action
    private bool actionStarted = false;
    public GameObject actionTarget;
    private float animSpeed = 15f;

    //alive
    private bool alive = true;

    void Start()
    {
        selector.SetActive(false);
        damageText.SetActive(false);
        critText.SetActive(false);
        weakText.SetActive(false);
        resistText.SetActive(false);
        stackText.SetActive(false);
        currCooldown = Random.Range(0, 4f);
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
                if (!alive) { break; }
                else
                {
                    //change tag to dead
                    this.gameObject.tag = "DeadEnemy";
                    //disable the selector
                    selector.SetActive(false);
                    //cannot be targeted
                    battleSM.enemiesInBattle.Remove(this.gameObject);

                    //remove all inputs by this enemy
                    if (this.battleSM.enemiesInBattle.Count > 0)
                    {
                        for (int i = 0; i < battleSM.performList.Count; i++)
                        {
                            if (i > 0)
                            {
                                if (battleSM.performList[i].attackerGameObject == this.gameObject)
                                {
                                    battleSM.performList.Remove(battleSM.performList[i]);
                                }
                                if (battleSM.performList[i].targetGameObject == this.gameObject)
                                {
                                    battleSM.performList[i].targetGameObject = battleSM.enemiesInBattle[Random.Range(0, this.battleSM.enemiesInBattle.Count)];
                                }
                            }
                        }
                    }
                    //change the color to gray / play dead animation
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 0, 25);
                    Vector3 deadPosition = new Vector3(startPosition.x - 2f, startPosition.y, startPosition.z);
                    transform.position = deadPosition;

                    //reset enemy buttons
                    battleSM.EnemyButtons();
                    //check alive
                    battleSM.battleState = BattleStateMachine.PerformAction.CHECKALIVE;
                    //set alive to be false
                    alive = false;
                }
                break;
            default:
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
        if (battleSM.heroesInBattle.Count > 0)
        {
            myAttack.targetGameObject = battleSM.heroesInBattle[Random.Range(0, battleSM.heroesInBattle.Count)];

            int attackIndex = Random.Range(0, enemy.attacks.Count);
            myAttack.chosenAttack = enemy.attacks[attackIndex];
            battleSM.CollectActions(myAttack);
            battleSM.UpdateTurnOrder();
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
        Vector3 targetPosition = new Vector3(actionTarget.transform.position.x - 1.5f, actionTarget.transform.position.y, actionTarget.transform.position.z);
        while(MoveTowardsTarget(targetPosition)) {  yield return null; }

        //wait
        yield return new WaitForSeconds(0.6f);

        //do damage
        DoDamage();

        //animate back to start position
        Vector3 firstPosition = startPosition;
        while (MoveTowardsTarget(firstPosition)) { yield return null; }

        //remove this performer from the list in battleSM
        battleSM.performList.RemoveAt(0);

        //reset the battleSM
        battleSM.battleState = BattleStateMachine.PerformAction.WAIT;
        battleSM.UpdateTurnOrder();

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

    public void TakeDamage(float damageAmount, string damageElement, bool crit)
    {
        string enemyElement = enemy.enemyType.ToString().ToUpper();
        string audioPath = "audio/eq_hero_attack";

        //environment damage factor
        float environmentDamage = 1.0f;
        if (enemyElement.Equals(battleSM.environmentElement))
        {
            environmentDamage = 0.8f; //20% dmg reduction on matching element w environment
        }

        //stacked element factor
        float elementDamage = elementalDamage(damageElement, enemyElement);
        string lastHitElement = enemy.lastHitType;
        if (damageElement.Equals(lastHitElement) && elementDamage != 1f)
        {
            elementDamage *= elementDamage; //square the elemental damage if the same element was used back to back
            stackText.SetActive(true);
        }
        enemy.lastHitType = damageElement;

        float calculatedDamage = damageAmount * environmentDamage * elementDamage;
        float roundedDamage = Mathf.RoundToInt(calculatedDamage);

        damageText.GetComponent<TextMesh>().text = roundedDamage.ToString();
        damageText.SetActive(true);
        if (crit)
        {
            critText.SetActive(true);
            audioPath = "audio/eq_hero_crit";
        }
        if (elementDamage > 1)
        {
            weakText.SetActive(true);
            audioPath = "audio/eq_magic_weak";
        }
        else if (elementDamage < 1)
        {
            resistText.SetActive(true);
            audioPath = "audio/eq_magic_resist";
        }

        AudioClip enemyAttackAudio = (AudioClip)Resources.Load(audioPath);
        battleSM.GetComponent<AudioSource>().PlayOneShot(enemyAttackAudio, 0.5f);

        enemy.currHP -= roundedDamage;
        if (enemy.currHP <= 0)
        {
            enemy.currHP = 0;
            currentState = TurnState.DEAD;
        }

        StartCoroutine(HideDamageText(0.7f));
    }

    void DoDamage()
    {
        float baseDamage = enemy.currATK + battleSM.performList[0].chosenAttack.attackDamage;
        string enemyElement = enemy.enemyType.ToString().ToUpper();

        float environmentDamage = 1.0f;
        if (enemyElement.Equals(battleSM.environmentElement))
        {
            environmentDamage = 1.2f; //extra 20% dmg on matching element w environment
        }

        //10% plus or minus on attack damage
        float damageRange = Random.Range(0.9f, 1.1f);

        //critical hit check (10% chance)
        if (damageRange > 1.08f)
        {
            damageRange = 1.5f;
        }

        float calculatedDamage = baseDamage * environmentDamage * damageRange;

        actionTarget.GetComponent<HeroStateMachine>().TakeDamage(calculatedDamage);

        AudioClip enemyAttackAudio = (AudioClip)Resources.Load("audio/eq_enemy_attack");
        battleSM.GetComponent<AudioSource>().PlayOneShot(enemyAttackAudio, 0.5f);
    }

    IEnumerator HideDamageText(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        damageText.SetActive(false);
        critText.SetActive(false);
        weakText.SetActive(false);
        resistText.SetActive(false);
        stackText.SetActive(false);
    }

    float elementalDamage(string attack, string target)
    {
        //min, less, normal, more, max
        float[] damageMultiplier = { 0.6f, 0.8f, 1.0f, 1.5f, 2.0f };

        //don't check if element attack is neutral
        string[] elementList = { "WOOD", "FIRE", "EARTH", "METAL", "WATER" };
        if (System.Array.IndexOf(elementList, attack) > -1)
        {
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
        }
        return 1.0f;
    }
}
