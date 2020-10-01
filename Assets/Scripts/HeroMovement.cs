using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    float moveSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = GameManager.instance.nextHeroPosition;
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        GetComponent<Rigidbody>().velocity = movement * moveSpeed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnterTown")
        {
            CollisionHandler collisionHandler = other.gameObject.GetComponent<CollisionHandler>();
            GameManager.instance.nextHeroPosition = collisionHandler.spawnPoint.transform.position;
            GameManager.instance.sceneToLoad = collisionHandler.sceneToLoad;
            GameManager.instance.LoadNextScene();
        }

        if (other.tag == "LeaveTown")
        {
            CollisionHandler collisionHandler = other.gameObject.GetComponent<CollisionHandler>();
            GameManager.instance.nextHeroPosition = collisionHandler.spawnPoint.transform.position;
            GameManager.instance.sceneToLoad = collisionHandler.sceneToLoad;
            GameManager.instance.LoadNextScene();
        }
    }
}
