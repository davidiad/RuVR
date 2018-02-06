using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitFloor : MonoBehaviour {

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void OnCollisionEnter (Collision col)
    {

        if (col.gameObject.name == "Ball")
        {
            if (gameManager) // ensure that game manager has been assigned
            {
                
                if (gameManager.LoadingComplete && !gameManager.levelWon) // check that we are not in the middle of loading a scene
                {
                    
                    gameManager.ResetCurrentLevel();
                }
            }
        }
    }

}
