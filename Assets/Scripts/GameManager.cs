using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //hero
    public GameObject heroCharacter;

    //positions
    public Vector3 nextHeroPosition;
    public Vector3 lastHeroPosition; //for battles

    //scenes
    public string sceneToLoad;
    public string lastScene; //for battles

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

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
