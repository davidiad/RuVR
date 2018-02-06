using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusHandInteraction : MonoBehaviour {

    private GameManager gameManager;
    public float throwForce = 1.5f;

    private OVRInput.Controller thisController;
    public bool leftHand; //if true this is the left hand controller
    //Swipe
    public float swipeSum;
    public float touchLast;
    public float touchCurrent;
    public float distance;
    public bool hasSwipedLeft;
    public bool hasSwipedRight;
    public ObjectMenuManager objectMenuManager;
    private bool menuIsSwipable;
    private float menuStickX;
    private bool handEmpty; // Flag to keep one hand from grabbing >1 object at a time
    

	void Start () {
        
        // this code is not working to distinguish between controllers for OVR
        // I believe, because both controllers have this script, they can both be sending a signal at the same time
        if (leftHand)
        {
            thisController = OVRInput.Controller.LTouch;
        }
        else
        {
            thisController = OVRInput.Controller.RTouch;
        }
        handEmpty = true;
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
	}
	
	void Update () {

        if (!leftHand)
        {
            menuStickX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick,
                thisController).x;
            if(menuStickX < 0.45f && menuStickX > -0.45f)
            {
                menuIsSwipable = true;
            }
            if (menuIsSwipable)
            {
                if(menuStickX >= 0.45f)
                {
                    //fire function that looks at menuList,
                    //disables current item, and enables next item
                    objectMenuManager.MenuRight();
                    menuIsSwipable = false;
                }
                else if(menuStickX <= -0.45f)
                {
                    objectMenuManager.MenuLeft();
                    menuIsSwipable = false;
                }
            }
        }

        // Use thumbstick Y axis to show or hide the menu
        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y > 0.45f)
        {
            objectMenuManager.ShowMenu(true);
        }
        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y < -0.45f)
        {
            objectMenuManager.ShowMenu(false);
        }


        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, thisController))
        {
            if (!leftHand && objectMenuManager.menuIsShowing) // only spawn when the current object is visible
            {
                if (!gameManager.ballWasReleasedInZone || gameManager.ballIsInZone) // don't allow spawning if the ball is in play -- could be used to cheat
                {
                    objectMenuManager.SpawnCurrentObject();
                }
                else
                {
                    // display warning that you can't spawn while ball is in play
                    objectMenuManager.DisplayWarning();
                }
            }
        }
      }


    void SpawnObject()
    {
        objectMenuManager.SpawnCurrentObject();
    }

    void SwipeLeft()
    {
        objectMenuManager.MenuLeft();
    }
    void SwipeRight()
    {
        objectMenuManager.MenuRight();
    }
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("Throwable")  || col.gameObject.CompareTag("Structure")) // avoid picking up anything but the ball or the Rube Goldberg objects
        {
            // Use boolean to avoid cross signals between controllers
            // With OVR, it seems that pressing a button on either controller sends the same message
            // One controller trigger being <0.1 while the other is > 0.1 causes problems
            // So first, determine which controller is being used
            if (leftHand)
            {
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) < 0.1f)
                {
                    if (!handEmpty) // Do not throw or place object unless it has been picked up
                    {
                        if (col.gameObject.CompareTag("Throwable"))
                        {
                            ThrowObject(col, OVRInput.Controller.LTouch);
                        }
                        else if (col.gameObject.CompareTag("Structure"))
                        {
                            PlaceObject(col, OVRInput.Controller.LTouch);
                        }
                    }
                }
                else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger,
                  OVRInput.Controller.LTouch) > 0.1f)
                {
                    if (handEmpty) // avoid more than 1 object held at a time by the same hand
                    {
                        GrabObject(col);
                        handEmpty = false;
                    }
                }
            }
            else  // right controller
            {
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) < 0.1f)
                {
                    if (!handEmpty) // Do not throw or place object unless it has been picked up
                    {
                        if (col.gameObject.CompareTag("Throwable"))
                        {
                            ThrowObject(col, OVRInput.Controller.RTouch);
                        }
                        else if (col.gameObject.CompareTag("Structure"))
                        {
                            PlaceObject(col, OVRInput.Controller.RTouch);
                        }
                    }
                }
                else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger,
                  OVRInput.Controller.RTouch) > 0.1f)
                {
                    if (handEmpty)
                    {
                        GrabObject(col);
                        handEmpty = false;
                    }
                }                                                                                                                                                                                                                                                                 
            }
        }
    }

    // Deal with the case that the collider may be a child in a hierarchy, with the Rigidbody on the parent (e.g., the fan)
    private GameObject GetColliderGameObject(Collider other)
    {
        if (other.gameObject.transform.parent == null)
        {
            return other.gameObject;
        }
        else if (other.gameObject.transform.parent.CompareTag("Hand")) // The parent is the hand, so the object is being grabbed, and we want to return the object, not the hand
        {
            return other.gameObject;
        }
        else // There's a parent, and it's not the hand, so this must be the fan (only object with a hierarchy currently being used as Structure)
        {
            return other.gameObject.transform.parent.gameObject;
        }      
    }

    void GrabObject(Collider other)
    {
        // Check if the object grabbed is the ball, and if so, flag ball released as false
        if (other.gameObject.CompareTag("Throwable"))
        {
            gameManager.ballWasReleasedInZone = false; // even if ball is in zone, this is still false, because the ball is not in release state
            // Reset the collectibles count. This prevents cheating by doing a run to collect the stars, and then a separate run to reach the goal
            gameManager.ResetCollectibles();
        }

        // if the ball has been released in the zone, then don't allow the Structure objects to be moved because
        // that could be used to cheat (the ball could be carried on a structure to the goal)
        if (!gameManager.ballWasReleasedInZone || gameManager.ballIsInZone)
        {
            GameObject obj = GetColliderGameObject(other);

            if (obj.GetComponent<Rigidbody>())
            {
                Rigidbody rigidBody = obj.GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;
                obj.transform.SetParent(gameObject.transform); // parents the object to the hand that's grabbing
                handEmpty = false;
            }
        }
    }

    // Throw, or drop, the ball
    void ThrowObject(Collider other, OVRInput.Controller controller)
    {
        // Unparenting from hand would leave the object in the Persistent scene
        // So first parent it to an object in the current level to bring back to the scene, before setting parent to null to release it from the hand
        other.transform.SetParent(gameManager.levelParent);
        other.transform.SetParent(null);
        Rigidbody rigidBody = other.GetComponent<Rigidbody>();
        rigidBody.isKinematic = false;
        rigidBody.velocity = OVRInput.GetLocalControllerVelocity(controller) * throwForce;
        // correct throw direction: rotate 90 degrees to the right
        rigidBody.velocity = Quaternion.Euler(0, 90, 0) * rigidBody.velocity;
        rigidBody.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(controller);//.eulerAngles;
        handEmpty = true;

        // Flag that the ball has been released
        gameManager.ballIsReleased = true; // needed???
        if (gameManager.ballIsInZone)
        {
            gameManager.ballWasReleasedInZone = true;
        }

        // Turn off instructions if they are on
        GameObject instructions = GameObject.FindGameObjectWithTag("Instructions");
        
        if (instructions)
        {
            instructions.SetActive(false);
        }
    }

    // non-Kinematic objects that can be moved, not thrown
    void PlaceObject(Collider other, OVRInput.Controller controller)
    {
        //TODO: may need an additional check to ensure that PlaceObject is not called unless the object is in the hand
        // (in theory, if a different object is being held, setting handEmpty to false, and then the hand collider
        // entered the fan, which has multiple collders, it could set a child element in the fan's parent to null


        // When parented to the camera rig hand, in the persistent scene, then unparented, the objects are left in the persistent scene
        // Solution: First set the parent to an empty game object in the current level. And then unparent.

        // Account for colliders with parents, eg, the fan. Also accounts for the case where an object is temp. parented to a hand
        GameObject obj = GetColliderGameObject(other);
        // Unparenting from hand would leave the object in the Persistent scene
        // So first parent it to an object in the current level to bring back to the scene, before setting parent to null
        obj.transform.SetParent(gameManager.levelParent);
        obj.transform.SetParent(null);
        handEmpty = true;
    }   
}