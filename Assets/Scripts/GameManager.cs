using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
	
public class GameManager : MonoBehaviour {

    public bool testingLoadLevelButton; // allows using B button to load next level
    public bool ballIsReleased; // the ball is currently released, not being grabbed
    public bool ballIsInZone; // the ball is in the allowed zone for releasing/winning level
    public bool ballWasReleasedInZone; // The ball was in the allowed zone when it was released
    public bool levelWon;
    public int collectiblesCount;
    public int collectiblesNeeded;

	public float fadeSpeed = 2f;
	private float fadeInterval;
	public GameObject camToFade;
	private int sceneCount;

	// Loading Progress: private setter, public getter
	private float _loadingProgress;
	public float LoadingProgress { get { return _loadingProgress; } }

    // Loading Complete: private setter, public getter
    private bool _loadingComplete;
    public bool LoadingComplete { get { return _loadingComplete; } }

    private BlackEffect blackEffect;

    // Level specific data
    private Vector3 ballStartPosition;
    private GameObject ball;
    private TextMeshPro levelText;
    private TextMeshPro instructions;
    private GameObject goal;

    private ObjectMenuManager objectMenuManager;

    public Transform levelParent; // parent to this to return objects to the current level, after parenting to the hands has moved them to the persistent level (See HandInteraction)
    public GameObject message;

    void Awake()
	{
		fadeInterval = 1f / (90f * fadeSpeed);
	}

	void Start () {
        ballIsReleased = true;
        ballWasReleasedInZone = true; // initial state, the ball was released onto the pedastal
        ballIsInZone = true;
        collectiblesCount = 0;
        collectiblesNeeded = 2;
		sceneCount = SceneManager.sceneCountInBuildSettings;
		blackEffect = camToFade.GetComponent<BlackEffect> ();
        _loadingComplete = true;

        objectMenuManager = GameObject.FindGameObjectWithTag("ObjectMenu").GetComponent<ObjectMenuManager>();

        // Load Level 1 to start with
        LoadNextLevel();
        
    }

    public void ResetCurrentLevel()
    {
        
        ball.transform.position = ballStartPosition; //Vector3.Lerp(ball.transform.position, ballStartPosition, 0.1f); 
        ball.transform.position = new Vector3(ballStartPosition.x, ballStartPosition.y, ballStartPosition.z);
        ballIsReleased = true;
        ballWasReleasedInZone = true;
        ballIsInZone = true;
        if (message)
        {
            message.SetActive(false);
        }
        ResetCollectibles();
        ResetInstructions();
        // Don't reset the inventory count of playable Rube Goldberg objects
        // the ones they have already instantiated can be left in their current positions and used again by the player
    }

    private void ResetInstructions()
    {
        // goalChild(1) is the GameObject that holds the instructions
        if (goal)
        {
            GameObject goalChild = goal.transform.GetChild(1).gameObject;
            goalChild.SetActive(true);
        }
    }

    public void ResetCollectibles()
    {
        // First, stars have to be SetActive, otherwise they can't be accessed
        // Use their parent to get a reference
        GameObject collectibles = GameObject.FindGameObjectWithTag("CollectiblesParent");

        for (int i = 0; i < collectibles.transform.childCount; i++)
        {
            GameObject star = collectibles.transform.GetChild(i).gameObject;
            star.SetActive(true);
            Collectible collectible = star.GetComponent<Collectible>();
            collectible.ResetStar();
        }
        collectiblesCount = 0;
    }


	public void LoadNextLevel()
	{
		StartCoroutine (FadeAndLoad());
	}

	private IEnumerator FadeAndLoad() {
        _loadingComplete = false;
		blackEffect.enabled = true;
		yield return StartCoroutine (FadeToBlack());
			
		Scene currentScene = SceneManager.GetActiveScene ();
		int nextIndex = currentScene.buildIndex + 1;
		if (nextIndex > sceneCount - 1) 
		{
			nextIndex = 1; // never load the scene at build index 0 because it is the persistent scene
		}

		// need to unload the current scene (if it is not the persistent scene)
		if (currentScene.buildIndex > 0) {
            Destroy(ball); // avoid an extra ball sometimes getting put into persistent scene
			SceneManager.UnloadSceneAsync (currentScene);
		}

		yield return StartCoroutine (LoadSceneAsync (nextIndex));

		Scene nextScene = SceneManager.GetSceneByBuildIndex (nextIndex);
		SceneManager.SetActiveScene (nextScene);

		yield return StartCoroutine (FadeFromBlack());
		blackEffect.enabled = false;
        _loadingComplete = true;
        levelWon = false;

        // Get references to objects in the level now that the level has finished loading
        // ensure that the ball and the message for the level are found
        ball = GameObject.FindGameObjectWithTag("Throwable");
        //ball.GetComponent<BallReleaseCheck>().message = GameObject.FindGameObjectWithTag("Message");
        ballStartPosition = ball.transform.position;
        // levelParent facilitates moving objects to the current scene
        levelParent = GameObject.FindGameObjectWithTag("LevelParent").transform;
        levelText = GameObject.FindGameObjectWithTag("LevelText").GetComponent<TextMeshPro>();
        levelText.text = "Level " + SceneManager.GetActiveScene().buildIndex;
        goal = GameObject.FindGameObjectWithTag("Goal");
        ResetInstructions();
        if (message)
        {
            message.SetActive(false);
        }
        objectMenuManager.SetInventoryLevels();
       
    }

	// adapted from http://myriadgamesstudio.com/how-to-use-the-unity-scenemanager/
	private IEnumerator LoadSceneAsync(int sceneBuildIndex)
	{
		var asyncScene = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);

		// this value stops the scene from displaying when it's finished loading
		asyncScene.allowSceneActivation = false;

		while (!asyncScene.isDone)
		{
			// loading bar progress // if needed // or other action, such as fade
			// note: 0.9 = fully loaded. 1.0 = ready to activate
			_loadingProgress = Mathf.Clamp01(asyncScene.progress / 0.9f) * 100;

			// scene has loaded as much as possible, the last 10% can't be multi-threaded
			if (asyncScene.progress >= 0.9f)
			{
				// we finally show the scene
				asyncScene.allowSceneActivation = true;
			}

			yield return null;
		}
	}

	private IEnumerator FadeToBlack () {
		while (blackEffect.intensity < 1f)
		{
			blackEffect.intensity += fadeInterval;
			yield return null;
		}
	}

	private IEnumerator FadeFromBlack () {
		while (blackEffect.intensity > 0f)
		{
			blackEffect.intensity -= fadeInterval;
			yield return null;
		}
	}
}
