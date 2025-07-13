using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class defines a player that can detect objects in front of it and interact with them. This is only for demo purposes
/// </summary>
public class LPDemoPlayer : MonoBehaviour
{
    internal Transform thisTransform;
    internal Rigidbody thisRigidbody;
    internal bool mobileControls = false;

    [Header("PLAYER MOVEMENT")]

    [Tooltip("The forward and backward movement speed of the player, when pressing 'w' or 's'")]
    public float forwardSpeed = 10;

    [Tooltip("The left and right movement speed of the player, when pressing 'a' or 'd'")]
    public float sideSpeed = 5;

    // The current total movement speed of the player both in forward and right axis
    internal Vector3 currentSpeed = new Vector3(0, 0, 0);

    [Tooltip("The turning speed of the player, controlled with the mouse")]
    public float turnSpeed = 10;

    [Tooltip("The player head object ( usually the camera ) which turns with the mouse to look at things")]
    public Transform playerHead;

    [Header("PLAYER CONTROLS")]
    public string forwardButton = "w";
    public string backwardButton = "s";
    public string rightButton = "d";
    public string leftButton = "a";

    [Tooltip("The button we need to press in order to activate the object we are aiming at")]
    public string activateButton = "Fire1";

    [Header("PLAYER INTERACTION")]

    [Tooltip("The detection range for objects we can interact with")]
    public float detectRange = 5;

    [Tooltip("The function we run when we click on an object we detected")]
    public string activateFunction = "ActivateObject";

    [Tooltip("The icon that appears when we are aiming at an object we can interact with")]
    public Texture2D activateIcon;
    internal bool showIcon;

    [Tooltip("The size of the activate icon")]
    public float iconSize = 50;

    void Start()
    {
        // Hold the transform and rigidbody for quicker access
        thisTransform = this.transform;
        thisRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Lock the cursor while using the player script
        Cursor.lockState = CursorLockMode.Locked;

        if (mobileControls == false) PlayerMoveControls();

        // Calculate the total movement speed in both forward and right axis
        currentSpeed = Vector3.Lerp(currentSpeed, Vector3.zero, Time.deltaTime * 10);

        // Disable keyboard/mouse controls if mobile controls are on
        if (mobileControls == false) PlayerTurnControls();
    }

    public void PlayerMoveControls()
    {
        // Calculate the forward and backward movement speed
        if (Input.GetKey(forwardButton)) currentSpeed += thisTransform.forward * forwardSpeed;
        else if (Input.GetKey(backwardButton)) currentSpeed -= thisTransform.forward * forwardSpeed;

        // Calculate the left and right movement speed
        if (Input.GetKey(rightButton)) currentSpeed += thisTransform.right * sideSpeed;
        else if (Input.GetKey(leftButton)) currentSpeed -= thisTransform.right * sideSpeed;
    }

    public void PlayerTurnControls()
    {
        // If we have a player head defined, allow it to look up and down with the mouse
        if (playerHead) playerHead.localEulerAngles += Vector3.left * Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime;

        // Allow the player to look left and right with the mouse
        thisTransform.eulerAngles += Vector3.up * Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime;
    }

    void FixedUpdate()
    {
        RaycastHit hit;

        // Constantly check if we are detecting an object in front of us that we can interact with
        if (Physics.Raycast(playerHead.position, playerHead.TransformDirection(Vector3.forward), out hit, detectRange))
        {
            // Show the activate icon 
            showIcon = true;

            // If we press the activate button, send a function to the object we are aiming at
            if (Input.GetButtonDown(activateButton) && !EventSystem.current.IsPointerOverGameObject())
            {
                // Send a function to the object we are aiming at
                hit.transform.gameObject.SendMessage(activateFunction, 0, SendMessageOptions.DontRequireReceiver);

                // Hide the icon
                showIcon = false;
            }
        }
        else if (showIcon == true) showIcon = false;

        // Limit the movement speed so we don't accelerate to infinity and beyond
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, forwardSpeed);

        // APply the current movement speed to the rigidbody
        thisRigidbody.MovePosition(thisTransform.position + currentSpeed * Time.deltaTime);
    }

    public void OnGUI()
    {
        // Show the icon at the center of the screen when we are aiming at an object we can interact with
        if (showIcon == true) GUI.Label(new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 50, 50), activateIcon);
    }
}

