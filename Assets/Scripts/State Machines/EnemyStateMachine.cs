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
    public GameObject damageText;

    //time for action
    private bool actionStarted = false;
    public GameObject actionTarget;
    private float animSpeed = 10f;

    //alive
    private bool alive = true;

    void Start()
    {
        selector.SetActive(false);
        damageText.SetActive(false);
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
                if (!alive)
                {
                    return;
                }
                else
                {
                    //change tag of enemy
                    this.gameObject.tag = "DeadEnemy";
                    //not attackable by heroes
                    battleSM.enemiesInBattle.Remove(this.gameObject);
                    //disable the selector
                    selector.SetActive(false);

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
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105, 105, 105, 255);
                    //set alive to be false
                    alive = false;
                    //reset enemy buttons
                    battleSM.EnemyButtons();
                    //check alive
                    battleSM.battleState = BattleStateMachine.PerformAction.CHECKALIVE;

                }
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

        int attackIndex = Random.Range(0, enemy.attacks.Count);
        myAttack.chosenAttack = enemy.attacks[attackIndex];
        Debug.Log(this.gameObject.name + " has chosen " + myAttack.chosenAttack.attackName + " and does " + myAttack.chosenAttack.attackDamage + " damage."); ;
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
        DoDamage();

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

    public void TakeDamage(float damageAmount)
    {
        damageText.GetComponent<TextMesh>().text = damageAmount.ToString();
        damageText.SetActive(true);
        StartCoroutine(HideDamageText());

        enemy.currHP -= damageAmount;
        if (enemy.currHP <= 0)
        {
            enemy.currHP = 0;
            currentState = TurnState.DEAD;
        }
    }

    void DoDamage()
    {
        float calculatedDamage = enemy.currATK + battleSM.performList[0].chosenAttack.attackDamage; 
        actionTarget.GetComponent<HeroStateMachine>().TakeDamage(calculatedDamage);
    }

    IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(1);

        damageText.SetActive(false);
    }
}
