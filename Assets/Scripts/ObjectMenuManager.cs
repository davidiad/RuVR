 //using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ObjectMenuManager : MonoBehaviour {

    public GameObject instantiationPoint;
    public GameObject[] inventoryCountTMP;
    public List<GameObject> objectList; //handled automatically at start
    public List<GameObject> objectPrefabList; //set manually in inspector and MUST match order
                                              //of scene menu objects
    public int currentObject = 0;
    public bool menuIsShowing;

    public int[] inventory;

	void Start () {
		foreach(Transform child in transform)
        {
            objectList.Add(child.gameObject);
        }
        menuIsShowing = false;

        // init inventory with values of 2 and length of object list
        inventory = new int[objectList.Count];
        // values will also be set after each level load, and after time a level has been lost and reset
        SetInventoryLevels();
	}

    // initially, set all inventory values to 2 for simplicity
    public void SetInventoryLevels()
    {
        for (int i=0; i<inventory.Length; i++)
        {
            // Set the max number of objects per item 
            inventory[i] = SceneManager.GetActiveScene().buildIndex + 2; // 2 items, plus 1 additional # items with each level
        }
    }

    public void MenuLeft()
    {
        ShowMenu(false);
        currentObject--;
        if(currentObject < 0)
        {
            currentObject = objectList.Count - 1;
        }
        ShowMenu(true);
    }
    public void MenuRight()
    {
        ShowMenu(false);
        currentObject++;
        if (currentObject > objectList.Count - 1)
        {
            currentObject = 0;
        }
       ShowMenu(true);
    }
    public void SpawnCurrentObject()
    {
        if (inventory[currentObject] > 0)
        {
            Instantiate(objectPrefabList[currentObject], instantiationPoint.transform.position, objectList[currentObject].transform.rotation);
            ShowMenu(false);

            inventory[currentObject] -= 1;
        }
    }

    public void ShowMenu(bool show)
    {
        objectList[currentObject].SetActive(show);
        menuIsShowing = show;

        // consider running this code on when an object is spawned, not every time the menu is shown
        // Set the correct updated text for the inventory levels
        // Get a reference to the inventory text for the current object
        //GameObject inventoryCount = GameObject.FindGameObjectWithTag("InventoryCount"); // assumes only one is active at a time, which should be the case
        //objectList[currentObject].transform.FindChild(name:"InventoryCount").gameObject;
        TextMeshPro inventoryTextMesh = inventoryCountTMP[currentObject].GetComponent<TextMeshPro>();

            if (inventory[currentObject] > 0)
            {
                 string replaceText = inventory[currentObject].ToString() + " Left";
                    inventoryTextMesh.SetText(replaceText);
            }
            else if (inventory[currentObject] == 0)
            {
                inventoryTextMesh.SetText("None left");
            }
    }

    // Trying to instantiate mesh under disallowed conditions
    public void DisplayWarning()
    {
        TextMeshPro inventoryTextMesh = inventoryCountTMP[currentObject].GetComponent<TextMeshPro>();
        inventoryTextMesh.SetText("No new objects while ball is in play");
    }
}
