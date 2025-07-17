using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// This class defines a lockpicking system that behaves similar to games such as Skyrim, Fallout, and Dying Light.
/// Here you must rotate the lockpick until you find the sweetspot, and then press a button to rotate the cylinder and unlock it
/// </summary>
public class LPLockpicking : MonoBehaviour
{
    // The lock activator, which can trigger Win or Lose actions
    internal LPLockActivator activator;

    // Holds all pieces of the safe lock for quicker access
    public RectTransform cylinder;
    internal float cylinderSize;
    public RectTransform lockpick;

    [Tooltip("How accurately close we need to be to the sweetspot. If set to 1, we need to be exactly at the sweetspot position, but if set to a higher number we can be farther from the center position of the sweetspot.")]
    [Range(1, 90)]
    public float sweetspotRange = 20; 

    [Tooltip("The button that rotates the cylinder. The cylinder will only rotate if we are in the sweetspot.")]
    public string rotateButton = "a";

    [Tooltip("How fast the cylinder rotates")]
    public float rotateSpeed = 10;

    [Tooltip("How much we need to rotate the cylinder in order to win")]
    public float rotateToWin = 330;

    [Tooltip("How long to wait before deactivating the lock object when we exit the game, either after winning/losing")]
    public float deactivateDelay = 1;

    [Tooltip("The sound that plays when we rotate the cylinder")]
    public AudioClip rotateSound;

    [Tooltip("The sound that plays when we reach a correct spot in one direction")]
    public AudioClip clickSound;

    [Tooltip("The sound that plays when the sequence resets")]
    public AudioClip resetSound;

    [Tooltip("The sound that plays when we win the lock game")]
    public AudioClip winSound;

    internal int index;

    // Is the cylinder rotating
    internal bool isRotating = false;

    // If in the sweetspot, the cylinder can be rotated. If not, the cylinder will return to its original angle
    internal bool inSweetspot = false;

    // If the lock is unlocked, we win
    internal bool isUnlocked = false;

    //The sweetspot angle that we must reach with the lockpick
    internal float sweetspotAngle = 0;
    
    public void Start()
    {
        if (activator == null) Activate(null);
    }

    public void Update()
    {
        if (isUnlocked == true) return;
        
        // If we have a lockpick, move it to the position of the mouse within the limits of the lockpick area
        if (lockpick && cylinder)
        {
            // If we are using a mobile platform, use touch controls
            if (Application.isMobilePlatform)
            {
                //if ( Input.touchCount > 0 )    lockpick.position = Input.GetTouch(0).position;
                if (Input.touchCount > 0) lockpick.eulerAngles = Vector3.forward * 180 * Mathf.Clamp((Input.GetTouch(0).position.x / Screen.width), 0.01f, 0.99f);
            }
            else // Otherwise, use mouse controls
            {
                lockpick.eulerAngles = Vector3.forward * 180 * Mathf.Clamp((Input.mousePosition.x / Screen.width), 0.01f, 0.99f);
            }

            lockpick.eulerAngles = Vector3.forward * Mathf.Clamp(lockpick.eulerAngles.z, 0, 180);
            
            // If we are close enough to the sweetspot area, we are in the sweetspot!
            if (Mathf.Abs(sweetspotAngle - lockpick.eulerAngles.z) < sweetspotRange)
            {
                inSweetspot = true;
            }
            else // Otherwise, we are not
            {
                inSweetspot = false;
            }
        }

        // If we have a cylinder, we can rotate it
        if ( !Application.isMobilePlatform && cylinder )
        {
            // If we press the rotate button, rotate the cylinder
            if (Input.GetKey(rotateButton) )
            {
                isRotating = true;
            }
            else
            {
                isRotating = false;
            }
        }

        if ( isRotating == true )
        {
            // Rotate the cylinder object in the direction we chose
            cylinder.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime, Space.World);

            // If the lockpick is in the sweetspot, the cylinder can rotate
            if (inSweetspot == true)
            {
                // Play the cylinder sound
                if (GetComponent<AudioSource>().isPlaying == false) GetComponent<AudioSource>().PlayOneShot(rotateSound);

                // If the cylinder rotates beyond this angle, we win
                if (cylinder.eulerAngles.z > rotateToWin)
                {
                    isUnlocked = true;

                    // Play the deactivation animation
                    if (GetComponent<Animator>()) GetComponent<Animator>().Play("Win");

                    // Run the win event on the lock activator
                    if (activator) activator.Invoke("Win", deactivateDelay);

                    // Play the win sound, if it exists
                    if (winSound && GetComponent<AudioSource>())
                    {
                        GetComponent<AudioSource>().Stop();

                        GetComponent<AudioSource>().PlayOneShot(winSound);
                    }

                    // Unselect any buttons on the cylinder, so we don't press them when pressing 'SPACE' after closing the lock
                    if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
                }
            }
            else // Otherwise, return to the original rotation
            {
                // Return to original rotation
                cylinder.eulerAngles = Vector3.Slerp(cylinder.eulerAngles, Vector3.zero, Time.deltaTime * 10);

                // Wiggle the lockpick to show that we are not able to rotate the cylinder
                //lockpick.Find("LockpickCenter").localPosition = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), 0);
                lockpick.Find("Lockpick").localPosition = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);

                // Play the lockpick wiggle sound
                if (GetComponent<AudioSource>().isPlaying == false) GetComponent<AudioSource>().PlayOneShot(clickSound);
            }
        }
        else
        {
            // Return to original rotation
            cylinder.eulerAngles = Vector3.Slerp(cylinder.eulerAngles, Vector3.zero, Time.deltaTime * 10);

            // Reset the lockpick position
            lockpick.Find("Lockpick").localPosition = Vector3.zero;

            // Play the reset sound
            GetComponent<AudioSource>().Stop();
        }
    }

    public void StartRotate()
    {
        isRotating = true;
    }

    public void StopRotate()
    {
        isRotating = false;
    }


    /// <summary>
    /// Activates the lock and starts the lock game
    /// </summary>
    /// <param name="activatorSource"></param>
    public void Activate(LPLockActivator activatorSource)
    {
        // Keep track of the activator so we can deactivate it later
        if (activatorSource) activator = activatorSource;

        // If we are not using a mobile device, hide the cylinder rotation buttons 
        if (!Application.isMobilePlatform)
        {
            if (GameObject.Find("MobileColorIndicator")) GameObject.Find("MobileColorIndicator").SetActive(false);
            if (GameObject.Find("ButtonRotate")) GameObject.Find("ButtonRotate").SetActive(false);
        }
        
        // Set a random sweetspot center, 
        sweetspotAngle = Random.Range(0, 180);
    }

    /// <summary>
    /// Deactivates the lock and stops the lock game. This is when we press the abort button
    /// </summary>
    public void Deactivate()
    {
        // Play the deactivate animation
        if (GetComponent<Animator>()) GetComponent<Animator>().Play("Deactivate");

        // Deactivate the source lock activator
        if (activator) activator.Invoke("DeactivateObject", deactivateDelay);
    }
}


    
        
        
