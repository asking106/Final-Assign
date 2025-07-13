using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class defines an activator which is any object the player can interact with and activate a lock with
/// </summary>
public class LPLockActivator : MonoBehaviour
{
    // The player object in the scene, tagged "Player"
    internal GameObject playerObject;

    // Several arrays used to hold objects from the RFPS package, and hide them when interacting with a lock
    internal MonoBehaviour[] enemiesRFPS;
    internal MonoBehaviour[] cameraRFPS;
    internal MonoBehaviour[] weaponPivot;
    internal MonoBehaviour[] playerRFPS;

    [Tooltip("The lock object that will be activated when we interact with this activator")]
    public GameObject lockObject;

    [Tooltip("A list of actions that will be triggered when we succesfully unlock the lock ")]
    public UnityEvent winEvents;

    [Tooltip("A list of actions that will be triggered when we fail unlocking the lock ")]
    public UnityEvent loseEvents;

    [System.Serializable]
    public class TargetAction
    {
        [Tooltip("The object that will be targeted by the action")]
        public GameObject actionTarget;

        [Tooltip("The name of the function that will be triggered")]
        public string actionFunction;

        [Tooltip("An optional parameter that can be passed along to the triggered function")]
        public float actionParameter;
    }

    [Tooltip("If the lock is locked, we interact with it and try to unlock it. If we succeed it becomes unlocked. If it is unlocked the Win Actions are triggered when we interact with it")]
    public bool isLocked = true;

    [Tooltip("Always show the lock object ( Don't hide it when at the start of the game or when we close it ). This is good if you want to put the lock in world space, like the Big Red Button example")]
    public bool alwaysShowLock = false;

    [Tooltip("Reposition the player and rotate it to face the lock object. This is useful if you want the player to look in a certain direction when interacting with a lock in the world space, like the Big Red Button")]
    public Transform playerDummy;

    [Tooltip("Checks if this project is used inside Realistic FPS Prefab (RFPS) and applies the relevant changes accordingly")]
    public static bool isRFPS = false;

    internal int index;

    // Use this for initialization
    void Start()
    {
        // Hide the player position object while playing the game
        if (playerDummy != null) playerDummy.gameObject.SetActive(false);

        // Deactivate the lock object at the start of the game
        if (lockObject && alwaysShowLock == false) lockObject.SetActive(false);

        try
        {
            // Check if there is at least one element related to RFPS in the scene.
            if (GameObject.FindGameObjectWithTag("Usable"))
            {
                isRFPS = true;
            }
        }
        catch (Exception e)
        {
            //Debug.LogWarning(e + "RFPS components not found, ignoring them");
        }
    }

    /// <summary>
    /// Runs a list of win actions on the targeted objects, such as unlocking a door, etc
    /// </summary>
    public void Win()
    {
        // The lock is no longer locked
        isLocked = false;

        // Go through all the targeted objects and run the win functions on them
        winEvents.Invoke();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Deactivate the lock object and reactivate any relevant scene objects ( from RFPS, etc )
        DeactivateObject();
    }

    /// <summary>
    /// Runs a list of lose actions on the targeted objects, such as exploding a bomb, etc
    /// </summary>
    public void Lose()
    {
        // Go through all the targeted objects and run the lose functions on them
        loseEvents.Invoke();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Deactivate the lock object and reactivate any relevant scene objects ( from RFPS, etc )
        DeactivateObject();
    }

    /// <summary>
    /// Activates the lock object, and deactivates any relevant scene objects ( from RFPS, etc )
    /// </summary>
    public void ActivateObject()
    {
        // If the object is locked, activate it so we can interact with it and unlock it
        if (isLocked == true)
        {
            // Deactivate all objects related to the RFPS package which might interfere with the lock minigame
            DeactivateRFPSObjects();

            // Free the mouse cursor so we can click on buttons ( only for UFPS )
            //vp_Utility.LockCursor = false;

            // Show the mouse cursor and don't lock it to the game window so we can interact with the activated object
            Cursor.visible = true;
            
            Cursor.lockState = CursorLockMode.None;

            // Activate the lock object
            if (lockObject)
            {
                lockObject.SetActive(true);

                // Start the minigame specific to this lock object
                lockObject.SendMessage("Activate", this);

                // Animate the intro of the activated lock object
                if (lockObject.GetComponent<Animator>()) lockObject.GetComponent<Animator>().Play("Activate");
            }
        }
        else
        {
            // Go through all the targeted objects and run the win functions on them
            winEvents.Invoke();
        }
    }

    /// <summary>
    /// Deactivates the lock object, and activates any relevant scene objects ( from RFPS, etc )
    /// </summary>
    public void DeactivateObject()
    {
        // Lock the mouse cursor so we can aim again ( only for UFPS )
        //vp_Utility.LockCursor = true;

        // Activate all objects related to the RFPS package so we can continue playing the game
        ActivateRFPSObjects();

        // Deactivate the lock object
        if (lockObject && alwaysShowLock == false) lockObject.SetActive(false);
    }

    /// <summary>
    /// Deactivates all objects related to the RFPS package which might interfere with the lock minigame
    /// </summary>
    public void DeactivateRFPSObjects()
    {
        // If there is a 
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            // Get the player object in the scene
            playerObject = GameObject.FindGameObjectWithTag("Player");

            // Get all the components attached to the player and its children
            MonoBehaviour[] allComponents = playerObject.GetComponentsInChildren<MonoBehaviour>();

            // Disable all the components we found
            foreach (MonoBehaviour component in allComponents) component.enabled = false;

            // If the player has a Rigid Body, prevent it from reacting to world physics
            if (playerObject.GetComponent<Rigidbody>()) playerObject.GetComponent<Rigidbody>().isKinematic = true;

            // If we have a player position/rotation dummy, set the new player position and rotation
            if (playerDummy != null)
            {
                // Set player position to dummy position
                playerObject.transform.position = playerDummy.transform.position;

                // Set player rotation to dummy rotation
                playerObject.transform.eulerAngles = Vector3.up * playerDummy.transform.eulerAngles.y;
                Camera.main.transform.localEulerAngles = Vector3.right * playerDummy.transform.localEulerAngles.x;
            }

            //StartCoroutine("Reposition");
        }

        // Disable all the elements related to RFPS
        if (isRFPS == true)
        {
            // Get all mouse controllers in the scene
            cameraRFPS = (MonoBehaviour[])FindObjectsOfType(Type.GetType("SmoothMouseLook")) as MonoBehaviour[];

            // Deactivate all mouse controllers in the scene
            for (index = 0; index < cameraRFPS.Length; index++) cameraRFPS[index].enabled = false;

            // Get all weapon pivots in the scene
            weaponPivot = (MonoBehaviour[])FindObjectsOfType(Type.GetType("WeaponPivot")) as MonoBehaviour[];

            // Deactivate all weapon pivots in the scene
            for (index = 0; index < weaponPivot.Length; index++) weaponPivot[index].enabled = false;

            // Get all AI scripts in the scene ( enemies and NPCs )
            enemiesRFPS = (MonoBehaviour[])FindObjectsOfType(Type.GetType("AI")) as MonoBehaviour[];

            // Hide all AI scripts in the scene ( so that enemies and NPCs don't attack while we interact with the lock )
            for (index = 0; index < enemiesRFPS.Length; index++) enemiesRFPS[index].gameObject.SetActive(false);

            // Get all player control scripts in the scene
            playerRFPS = (MonoBehaviour[])FindObjectsOfType(Type.GetType("InputControl")) as MonoBehaviour[];

            // Hide all player control scripts in the scene ( so that the player can't move while interacting with a lock )
            for (index = 0; index < playerRFPS.Length; index++) playerRFPS[index].enabled = false;
        }
    }

    /// <summary>
    /// Activates all objects related to the RFPS package so we can continue playing the game
    /// </summary>
    public void ActivateRFPSObjects()
    {
        // Enable the components on the player object in the scene
        if (playerObject)
        {
            // Get all the components attached to the player and its children
            MonoBehaviour[] allComponents = playerObject.GetComponentsInChildren<MonoBehaviour>();

            // Enable all the components we found
            foreach (MonoBehaviour component in allComponents) component.enabled = true;

            // If the player has a Rigid Body, allow it to react to world physics
            if (playerObject.GetComponent<Rigidbody>()) playerObject.GetComponent<Rigidbody>().isKinematic = false;
        }

        // Enable all mouse controllers in the scene
        if (cameraRFPS != null) for (index = 0; index < cameraRFPS.Length; index++) cameraRFPS[index].enabled = true;

        // Enable all mouse controllers in the scene
        if (weaponPivot != null) for (index = 0; index < weaponPivot.Length; index++) weaponPivot[index].enabled = true;

        // Show all AI scripts in the scene ( so that enemies and NPCs don't attack while we interact with the lock )
        if (enemiesRFPS != null) for (index = 0; index < enemiesRFPS.Length; index++) enemiesRFPS[index].gameObject.SetActive(true);

        // Show all player control scripts in the scene ( so that the player can move again )
        if (playerRFPS != null) for (index = 0; index < playerRFPS.Length; index++) playerRFPS[index].enabled = true;
    }

    public void RemoveObject()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Animates the respositioning of the player to the position and rotation of the player dummy.
    /// THIS IS CURRENTLY UNUSED AS IT DOES NOT WORK CORRECTLY
    /// </summary>
    /// <returns></returns>
    IEnumerator Reposition()
    {
        if (playerDummy != null)
        {
            // Set a timeout for the repositioning transition
            float timeOut = 1;

            // Transition from the current position and rotation to the dummy position and rotation
            while (timeOut > 0)
            {
                // Transition to the dummy position
                playerObject.transform.position = Vector3.Slerp(playerObject.transform.position, playerDummy.position, 1);

                // Transition to the dummy rotation
                Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, playerDummy.rotation, 1);

                // Wait a frame between transition increments
                timeOut -= Time.deltaTime;

                // Animate the transition
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }
}
