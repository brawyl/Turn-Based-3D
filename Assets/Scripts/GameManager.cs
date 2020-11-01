using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public RegionData currRegion;

    //spawn points
    public string nextSpawnPoint;

    //hero
    public GameObject heroCharacter;

    //positions
    public Vector3 nextHeroPosition;
    public Vector3 lastHeroPosition; //for battles

    //scenes
    public string sceneToLoad;
    public string lastScene; //for battles

    //bools
    public bool isWalking = false;
    public bool canGetEncounter = false;
    public bool gotAttacked = false;

    //enum
    public enum GameStates
    {
        WORLD_STATE,
        TOWN_STATE,
        BATTLE_STATE,
        IDLE
    }

    //battle
    public List<GameObject> enemiesToBattle = new List<GameObject>();
    public int enemyAmount;

    public GameStates gameState;

    void Awake()
    {
        //check if instance exists
        if (instance == null)
        {
            //set instance to this
            instance = this;
        }
        //exists but is another instance
        else if (instance != this)
        {
            //destroy it
            Destroy(gameObject);
        }
        //set this to not be destroyable
        DontDestroyOnLoad(gameObject);

        if (!GameObject.Find("HeroCharacter"))
        {
            GameObject hero = Instantiate(heroCharacter, nextHeroPosition, Quaternion.identity) as GameObject;
            hero.name = "HeroCharacter";
        }
    }

    void Update()
    {
        switch (gameState)
        {
            case (GameStates.WORLD_STATE):
                if (isWalking)
                {
                    RandomEncounter();
                }
                if (gotAttacked)
                {
                    gameState = GameStates.BATTLE_STATE;
                }
                break;

            case (GameStates.TOWN_STATE):

                break;

            case (GameStates.BATTLE_STATE):
                //load battle scene
                StartBattle();
                //go to idle
                gameState = GameStates.IDLE;
                break;

            case (GameStates.IDLE):

                break;
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadSceneAfterBattle()
    {
        SceneManager.LoadScene(lastScene);
    }

    void RandomEncounter()
    {
        if (isWalking && canGetEncounter)
        {
            if (Random.Range(0,1000) < 10)
            {
                //Debug.Log("Got Attacked");
                gotAttacked = true;
            }
        }
    }

    void StartBattle()
    {
        //amount of enemies to encounter
        enemyAmount = Random.Range(1, currRegion.maxAmountEnemies+1);

        //which enemies to battle
        for (int i=0; i<enemyAmount; i++)
        {
            enemiesToBattle.Add(currRegion.possibleEnemies[Random.Range(0, currRegion.possibleEnemies.Count)]);
        }

        //hero data
        lastHeroPosition = GameObject.Find("HeroCharacter").gameObject.transform.position;
        nextHeroPosition = lastHeroPosition;
        lastScene = SceneManager.GetActiveScene().name;

        //load level
        SceneManager.LoadScene(currRegion.battleScene);

        //reset hero
        isWalking = false;
        gotAttacked = false;
        canGetEncounter = false;
    }
}
