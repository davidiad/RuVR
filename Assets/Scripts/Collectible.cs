using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour {

    public GameObject poof;

    private float starSize;
    private GameManager gameManager;
    private bool collected;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        starSize = 1f;
        collected = false;
    }
	
    // In case the level was lost, reset all collected stars
    public void ResetStar()
    {
        // GameManagers collectiblesCount also needs to be set to 0
        starSize = 1f;
        transform.localScale = new Vector3(starSize, starSize, starSize);
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = new Vector3(0.2549219f, 0.2449631f, 0.05875923f); // reset the scale of the collider
        collected = false;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (gameManager.ballWasReleasedInZone) // only allow collecting stars if the ball was released in the zone -> cheating
        {
            if (!collected && col.gameObject.name == "Ball")
            {
                collected = true;
                Object.Instantiate(poof, this.transform, false);
                StartCoroutine(SlowDeathOfAStar());
            }
        }
        else
        {
            Debug.Log("Ball was not released in zone, Stars are not collectible");
        }
    }

    private IEnumerator SlowDeathOfAStar()
    {
        // Make the star shrink so it looks like it's disappearing while the particles run
        // if the star is destroyed or made inactive, the particle effect is also. Therefore, using a coroutine to allow some time for particles to run
        while (starSize > 0.2f)
        {
            transform.localScale = new Vector3(starSize, starSize, starSize);
            starSize -= 0.01f;
            yield return null;
        }


        // Hide, rather than destroy the collectible, so that it can be reset in the case that it has been collected but the level was lost
        this.gameObject.SetActive(false);

        if (gameManager)
        {
            gameManager.collectiblesCount += 1;
        }

    }
}
