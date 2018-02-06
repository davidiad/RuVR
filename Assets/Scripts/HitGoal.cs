using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitGoal : MonoBehaviour {

    public bool testing;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        testing = false;
    }

    public void OnCollisionEnter (Collision col)
    {
        if (col.gameObject.name == "Ball")
        {
            if ((gameManager.ballWasReleasedInZone == true) || (testing == true))
            {
                if (gameManager && !gameManager.levelWon)
                {
                    if (gameManager.LoadingComplete)
                    {
                        if (gameManager.collectiblesCount >= gameManager.collectiblesNeeded)
                        {
                            gameManager.levelWon = true; // prevent levels from loading multiple times with multiple collisions
                            gameManager.LoadNextLevel();
                        }
                        else
                        {
                            Debug.Log("You need to collect all the stars first!" + " # Collectibles: " + gameManager.collectiblesCount);
                            
                        }
                    }
                }
            }
        }
    }
}
