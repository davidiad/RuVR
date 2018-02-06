using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReleaseCheck : MonoBehaviour {

	public Transform platform;
    public GameObject zone;
   // public GameObject message;

    private GameManager gameManager;

	private float ballMinX;
	private float ballMaxX;
	private float ballMinY;
	private float ballMaxY;
	private float ballMinZ;
	private float ballMaxZ;


    void Start()
    {
        // get the platform transform, based on object in the scene
        // calculate the min and max allowed values for the ball transform, based on position of ball and its radius
        // absolute difference bet. platform and ball transform should be <= 0.85
        // .44 + .15 = 0.59 -- minimum height of ball relative to platform. Maximum height? TBA
        //ballMinY = platform.position.x + 0.59f;

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        platform = GameObject.FindGameObjectWithTag("Platform").transform;
        zone = GameObject.FindGameObjectWithTag("ReleaseZone");
       // message = gameManager.message;

    }

	void Update () {
        // make sure ball gets the message. Does not always get it in Start. Find message in GameManager after level is loaded

        if (platform && gameManager.message) // making sure the new platform for the level has been instantiated, and platform and message can be accessed
        {
            gameManager.ballIsInZone = IsBallInZone();

            if (!gameManager.ballIsInZone)
            {
                ShowBounds(true);
            }
            else
            {
                ShowBounds(false);
            }
        }
	}

	private bool IsBallInZone () {
		if ((Mathf.Abs (platform.position.x - transform.position.x) > 0.85f) ||
		    (Mathf.Abs (platform.position.z - transform.position.z) > 0.85f) ||
		    ((transform.position.y - platform.position.y) < 0.58f) ||
            ((transform.position.y - platform.position.y) > 2.58f)) {
            return false;
		} else {
			return true;
		}
	}

    private void ShowBounds(bool show)
    {
        zone.SetActive(show);
        if (!gameManager.ballWasReleasedInZone)
        {
            gameManager.message.SetActive(show);
        }
    }

}
