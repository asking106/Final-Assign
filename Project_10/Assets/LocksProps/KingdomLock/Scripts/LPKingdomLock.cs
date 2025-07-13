using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// This class defines a cylinder , which is a type of lock similar to the game "Simon"; in it you must repeat a sequence
/// of tones succesfully in order to win. You can set a random seuqence or a predetermined sequence in the lock.
/// </summary>
public class LPKingdomLock : MonoBehaviour
{
    // The lock activator, which can trigger Win or Lose actions
    internal LPLockActivator activator;

    // Holds all pieces of the safe lock for quicker access
    public RectTransform cylinder;
    internal float cylinderSize;
    public RectTransform lockpick;
    internal RectTransform sweetspot;

    [Tooltip("How accurately close we need to be to the sweetspot. If set to 1, we need to be exactly at the sweetspot position, but if set to a lower number we can be farther from the center position of the sweetspot.")]
    [Range(0, 1)]
    public float sweetspotAccuracy = 0.9f;

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
                if ( Input.touchCount > 0 )    lockpick.position = Input.GetTouch(0).position;
            }
            else // Otherwise, use mouse controls
            {
                lockpick.position = Input.mousePosition;
            }

            // Calculate the cylinder size based on either the width of height of it, whichever is smaller
            cylinderSize = Mathf.Min(cylinder.sizeDelta.x * GetComponent<RectTransform>().localScale.x, cylinder.sizeDelta.y * GetComponent<RectTransform>().localScale.y);

            // Keep the lockpick within the cylinder size in a circular area
            if ( Vector2.Distance(lockpick.position, cylinder.position) > cylinderSize * 0.5f )
            {
                lockpick.position = cylinder.position + (lockpick.position - cylinder.position).normalized * cylinderSize * 0.5f;
            }

            // Set the scale of the lockpick based on its distance from the sweetspot
            lockpick.Find("LockpickCenter").localScale = Vector3.one * (1-(Vector2.Distance(lockpick.position, sweetspot.position) / cylinderSize));

            // If we are close enough to the sweetspot area, we are in the sweetspot!
            if (lockpick.Find("LockpickCenter").localScale.x >= sweetspotAccuracy)
            {
                inSweetspot = true;

                lockpick.Find("LockpickCenter").GetComponent<Image>().color = Color.white;

                if (GameObject.Find("MobileColorIndicator")) GameObject.Find("MobileColorIndicator").GetComponent<Image>().color = Color.white;
            }
            else // Otherwise, we are not
            {
                inSweetspot = false;

                lockpick.Find("LockpickCenter").GetComponent<Image>().color = Color.red;

                if (GameObject.Find("MobileColorIndicator")) GameObject.Find("MobileColorIndicator").GetComponent<Image>().color = Color.red;
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
                lockpick.Find("LockpickCenter").localPosition = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), 0);

                // Play the lockpick wiggle sound
                if (GetComponent<AudioSource>().isPlaying == false) GetComponent<AudioSource>().PlayOneShot(clickSound);
            }
        }
        else
        {
            // Return to original rotation
            cylinder.eulerAngles = Vector3.Slerp(cylinder.eulerAngles, Vector3.zero, Time.deltaTime * 10);

            // Reset the lockpick position
            lockpick.Find("LockpickCenter").localPosition = Vector3.zero;

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
        
        if (GameObject.Find("Sweetspot"))
        {
            // Hold the sweetspot object for quicker access
            sweetspot = GameObject.Find("Sweetspot").GetComponent<RectTransform>();
            
            // Hide the image component from the sweetspot ( the red do we use to set the size in the editor )
            sweetspot.GetComponent<Image>().enabled = false;
        }
        
        // Calculate the minimum sweetspot range, so that it's never exactly in the center
        float minimumRange = Mathf.Min(lockpick.sizeDelta.x * GetComponent<RectTransform>().localScale.x * 0.5f, lockpick.sizeDelta.y * GetComponent<RectTransform>().localScale.y * 0.5f);

        // Calculate the maximum sweetspot range based on the cylinder size
        cylinderSize = Mathf.Min(cylinder.sizeDelta.x * GetComponent<RectTransform>().localScale.x, cylinder.sizeDelta.y * GetComponent<RectTransform>().localScale.y);
        
        // Choose a random spot within the cylinder area
        float positionX = Random.Range(cylinder.position.x - cylinder.sizeDelta.x * GetComponent<RectTransform>().localScale.x * 0.5f, cylinder.position.x + cylinder.sizeDelta.x * GetComponent<RectTransform>().localScale.x * 0.5f);
        float positionY = Random.Range(cylinder.position.y - cylinder.sizeDelta.y * GetComponent<RectTransform>().localScale.y * 0.5f, cylinder.position.y + cylinder.sizeDelta.y * GetComponent<RectTransform>().localScale.y * 0.5f);

        // Set the sweetspot position
        sweetspot.position = new Vector2(positionX, positionY);

        // Limit the sweetspot position to a circular area within the cylinder
        sweetspot.position = cylinder.position + (sweetspot.position - cylinder.position).normalized * Random.Range(minimumRange, cylinderSize * 0.5f);
        
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
