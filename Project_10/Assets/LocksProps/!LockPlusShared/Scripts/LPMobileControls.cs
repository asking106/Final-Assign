using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines a player that can detect objects in front of it and interact with them. This is only for demo purposes
/// </summary>
public class LPMobileControls : MonoBehaviour
{
    [Tooltip("The player controller which must be assigned from the scene to make it controllable with mobile")]
    public LPDemoPlayer playerController;

    // The right mobile stick that moves the player in 4 directions
    internal Vector2 rightStickPosition;
    internal bool rightStickDown = false;

    // The left mobile stick that rotates the player in 4 directions
    internal Vector2 leftStickPosition;
    internal bool leftStickDown = false;

    void Start()
    {
        // If no player controller is assigned, show a warning and disable mobile controls
        if ( playerController == null )
        {
            Debug.LogWarning("Must assign player controller from the scene (LPDemoPlayer)");

            this.enabled = false;
        }
        else if ( Application.isMobilePlatform ) // Turn on mobile controls if they are assigned
        {
            playerController.mobileControls = true;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Apply mobile controls while we are holding down the right stick or left stick
        if (rightStickDown == true) RightStick();
        if (leftStickDown == true) LeftStick();
    }

    /// <summary>
    /// Checks if we started holding the right stick down and registers the touch position
    /// </summary>
    public void RightStickDown()
    {
        rightStickPosition = Input.mousePosition;

        rightStickDown = true;
    }

    /// <summary>
    /// Checks if we released the right stick
    /// </summary>
    public void RightStickUp()
    {
        rightStickDown = false;
    }

    /// <summary>
    /// Tracks the movement changes while holding the right stick down
    /// </summary>
    public void RightStick()
    {
        // Track the horizontal and vertical movement of the moues or touch
        playerController.currentSpeed += 0.01f * (Input.mousePosition.y - rightStickPosition.y) * playerController.thisTransform.forward * playerController.forwardSpeed;
        playerController.currentSpeed += 0.01f * (Input.mousePosition.x - rightStickPosition.x) * playerController.thisTransform.right * playerController.sideSpeed;
    }

    /// <summary>
    /// Checks if we started holding the left stick down and registers the touch position
    /// </summary>
    public void LeftStickDown()
    {
        leftStickPosition = Input.mousePosition;

        leftStickDown = true;
    }

    /// <summary>
    /// Checks if we released the left stick
    /// </summary>
    public void LeftStickUp()
    {
        leftStickDown = false;
    }

    /// <summary>
    /// Tracks the movement changes while holding the left stick down
    /// </summary>
    public void LeftStick()
    {
        // If we have a player head defined, allow it to look up and down with the mouse
        if (playerController.playerHead) playerController.playerHead.localEulerAngles += Vector3.left * (Input.mousePosition.y - leftStickPosition.y) * playerController.turnSpeed * Time.deltaTime * 0.01f;
        if (playerController) playerController.transform.localEulerAngles += Vector3.up * (Input.mousePosition.x - leftStickPosition.x) * playerController.turnSpeed * Time.deltaTime * 0.01f;
    }
}
