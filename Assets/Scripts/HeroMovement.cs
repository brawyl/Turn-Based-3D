using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    float moveSpeed = 10f;

    Vector3 currPosition, lastPosition;

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

        currPosition = transform.position;

        if (currPosition == lastPosition)
        {
            GameManager.instance.isWalking = false;
        }
        else
        {
            GameManager.instance.isWalking = true;
        }

        lastPosition = currPosition;
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

        if (other.tag == "region1")
        {
            GameManager.instance.currRegion = 0;
        }

        if (other.tag == "region2")
        {
            GameManager.instance.currRegion = 1;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "region1" || other.tag == "region2")
        {
            GameManager.instance.canGetEncounter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "region1" || other.tag == "region2")
        {
            GameManager.instance.canGetEncounter = false;
        }
    }
}
