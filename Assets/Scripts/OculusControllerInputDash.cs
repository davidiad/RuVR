using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusControllerInputDash : MonoBehaviour {

    public GameObject laserObject;

    //Bounds for Camera Rig to avoid going thru walls etc
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float minZ;
    public float maxZ;

    //Teleporter
    private LineRenderer laser;
    public GameObject teleportAimerObject;
    public Vector3 teleportLocation;
    public float teleportHeight = 3f; // player position above floor after teleporting
    public float objectGap = 0.8f; // gap between teleport location and object
    public float wallGap = 2.1f; // a larger gap between teleport location and object, specific to walls and ceiling
    private float gap; // set based on which collider is hit by raycast
    public GameObject player;
    public LayerMask laserMask;
    public float yNudgeAmount = 0f; //specific to teleportAimerObject height

	// Dash
	public float dashSpeed = 20f;
	private bool isDashing;
	private float lerpTime;
	private Vector3 dashStartPosition;

    // Walking
    private float menuStickX;
    private float menuStickY;
	public float floorHeight = 1f;
	public Transform playerCam;
	public float moveSpeed = 4f;
	private Vector3 movementDirectionX;
    private Vector3 movementDirectionY;

    // access the GameManager to go to the next level for testing
    private GameManager gm;

    // Use this for initialization
    void Start () {
        laser = laserObject.GetComponent<LineRenderer>();
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }


    void Update()
    {

        // Walking

        // conditional so walking does not interfere when teleporting is happening
        if (!OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
        { 
            // Don't allow 'walking' if out of bounds
            if ((player.transform.position.y > 0.625f) && (player.transform.position.y < 7f))
            { 
                if ((player.transform.position.x > minX) && (player.transform.position.x < maxX))
                {
                     if ((player.transform.position.z > minZ && (player.transform.position.z < maxZ)))
                     {
                            menuStickX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).x;
                            menuStickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).y;

                            if (menuStickX > 0.05f || menuStickX < 0.05f)
                            {
                                movementDirectionX = playerCam.transform.right;
                                movementDirectionX *= moveSpeed * menuStickX * Time.deltaTime;
                                player.transform.position += movementDirectionX;
                            }
                            if (menuStickY > 0.05f || menuStickY < 0.05f)
                            {
                                movementDirectionY = playerCam.transform.forward;
                                movementDirectionY *= moveSpeed * menuStickY * Time.deltaTime;
                                player.transform.position += movementDirectionY;
                            }
                        }
                    }
                }
            }



        // Teleporting
            if (isDashing)
            {
                lerpTime += Time.deltaTime * dashSpeed;
                player.transform.position = Vector3.Lerp(dashStartPosition, teleportLocation, lerpTime);
                if (lerpTime >= 1f)
                {
                    isDashing = false;
                    lerpTime = 0f;
                }
            }
            else
            {
                if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
                {
                    laser.gameObject.SetActive(true);
                    teleportAimerObject.SetActive(true);

                    laser.SetPosition(0, gameObject.transform.position);
                    RaycastHit hit;

                    // The raycast has hit an object
                    if (Physics.Raycast(transform.position, transform.forward, out hit, 15, laserMask))
                    {
                        if (hit.collider.gameObject.CompareTag("NoTeleportZone"))
                        {
                            gap = wallGap;
                        } else
                        {
                            gap = objectGap;
                        }
                        teleportLocation = hit.point;
                        laser.SetPosition(1, teleportLocation);
                        //aimer position
                        teleportAimerObject.transform.position = teleportLocation;
                        if (hit.collider.gameObject.CompareTag("Ground"))
                        {
                            yNudgeAmount = 1.8f;
                        } else if (hit.collider.gameObject.CompareTag("Collectible"))
                        {
                            yNudgeAmount = 0.65f;
                        }
                        else
                        {
                            yNudgeAmount = 1.25f;
                        }
                            teleportLocation = new Vector3(hit.point.x, hit.point.y + yNudgeAmount, hit.point.z); // 1f should be a variable, could be different depending on object, e.g. more for floor, less for a small object like a star
                    }
                    else
                    // The raycast has *not* hit an object, so cast another ray toward the floor to determine teleport position
                    {
                        teleportLocation = transform.forward * 5 + transform.position;
                        laser.SetPosition(1, teleportLocation);
                        // aimer position (to same as end of laser)
                        teleportAimerObject.transform.position = teleportLocation;

                        RaycastHit groundRay;
                        if (Physics.Raycast(teleportLocation, -Vector3.up, out groundRay, 17, laserMask))
                        {
                            teleportLocation = groundRay.point;
                        }

                        // move the actual teleport location up by nudge amount, so we aren't lookiung below the floor
                        teleportLocation = teleportLocation + new Vector3(0, yNudgeAmount, 0);
                    }

                    // If the laser is pointing up, rotate the aimer object 180 degrees so we see the top of it instead of the bottom
                    if (transform.forward.y > 0.3f)
                    {
                        teleportAimerObject.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
                    }
                    else
                    {
                        teleportAimerObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
                    }

                    // Reduce the magnitude of teleport location, so that the camera doesn't land inside the targeted object
                    Vector3 heading = teleportLocation - transform.position;
                    float fullDistance = heading.magnitude;
                    Vector3 amountToMove = heading * ((fullDistance - gap) / fullDistance); // could be negative if object is too close to camera
                    teleportLocation = transform.position + amountToMove;

                    // Enforce bounds for teleportation
                    // ensure that teleportation doesn't go through the floor or the ceiling or walls
                    teleportLocation = CheckBounds(teleportLocation);

                }
                if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
                {
                    laser.gameObject.SetActive(false);
                    teleportAimerObject.SetActive(false);
                    dashStartPosition = player.transform.position;
                    isDashing = true;

                }

                // Use button to go to next level (testing only)
                if (OVRInput.GetUp(OVRInput.Button.One))
                {
                    if (gm.testingLoadLevelButton && gm.LoadingComplete)
                    {
                        gm.LoadNextLevel();
                    } 
                }
        }
    }

    // Enforce bounds for teleportation to ensure that teleportation doesn't go through the floor or the ceiling or walls
    private Vector3 CheckBounds(Vector3 teleportLocation)
    {
       
        if (teleportLocation.x < minX)
        {
            teleportLocation = new Vector3(minX, teleportLocation.y, teleportLocation.z);
        }
        if (teleportLocation.x > maxX)
        {
            teleportLocation = new Vector3(maxX, teleportLocation.y, teleportLocation.z);
        }
        if (teleportLocation.y < minY)
        {
            teleportLocation = new Vector3(teleportLocation.x, minY, teleportLocation.z);
        }
        if (teleportLocation.y > maxY)
        {
            teleportLocation = new Vector3(teleportLocation.x, maxY, teleportLocation.z);
        }
        if (teleportLocation.z < minZ)
        {
            teleportLocation = new Vector3(teleportLocation.x, teleportLocation.y, minZ);
        }
        if (teleportLocation.z > maxZ)
        {
            teleportLocation = new Vector3(teleportLocation.x, teleportLocation.y, maxZ);
        }
        return teleportLocation;
    }
}
