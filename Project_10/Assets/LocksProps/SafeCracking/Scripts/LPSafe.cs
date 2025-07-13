using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// This class defines a Dial Pad, which is a type of lock similar to the game "Simon"; in it you must repeat a sequence
/// of tones succesfully in order to win. You can set a random seuqence or a predetermined sequence in the lock.
/// </summary>
public class LPSafe : MonoBehaviour
{
    // The lock activator, which can trigger Win or Lose actions
    internal LPLockActivator activator;

    // Holds all pieces of the safe lock for quicker access
    internal RectTransform stethoscopeArea;
    internal RectTransform stethoscope;
    internal RectTransform hotspot;
    internal RectTransform dial;

    // Holds the size of the hotspot, which is automatically taken from the width or height of the 'hotspot' object ( the shorter )
    internal float hotspotSize = 100;

    [Tooltip("The button that rotate the dial right")]
    public string dialRightButton = "d";

    [Tooltip("The button that rotate the dial right")]
    public string dialLeftButton = "a";

    // Holds the current rotation direction of the dial. This is used by mobile controls to keep track of the dial rotation
    internal int rotateDirection = 0;

    [Tooltip("How fast the dial rotates left or right")]
    public float dialRotateSpeed = 10;

    [Tooltip("How many correct dial rotation ( until we hear the click ) are needed in order to unlock this lock")]
    public int dialTurns = 4;

    [Tooltip("The rotation range needed to reach the correct point. For example is we may need to rotate to the right 100 degrees, or to the left 30 degrees until we hear the click")]
    public Vector2 minMaxRotation = new Vector2(30, 180);
    
    // The direction we should be rotating in to get to the right spot. If we move too much in the opposite direction the whole sequence resets
    internal int dialDirection = 1;

    [Tooltip("How far in the wrong direction must a dial rotate before the whole sequence is reset")]
    public float dialReset = 20;
    internal float dialResetCount = 0;
    
    [Tooltip("The sequence of dial rotation we must follow in order to win. These must be entered manually.")]
    public int[] dialSequence;
    internal int sequenceIndex = 0;
    internal float currentSequence = 0;

    [Tooltip("How long to wait before deactivating the lock object when we exit the game, either after winning/losing")]
    public float deactivateDelay = 1;

    [Tooltip("The sound that plays when we rotate the dial")]
    public AudioClip rotateSound;

    [Tooltip("The sound that plays when we reach a correct spot in one direction")]
    public AudioClip clickSound;

    [Tooltip("The sound that plays when the sequence resets")]
    public AudioClip resetSound;

    [Tooltip("The sound that plays when we win the lock game")]
    public AudioClip winSound;

    internal int index;

    public void Start()
    {
        if (activator == null) Activate(null);
    }

    public void Update()
    {
        // If we have a stethoscope, move it to the position of the mouse within the limits of the stethoscope area
        if (stethoscope && stethoscopeArea)
        {
            // If we are using a mobile platform, use touch controls
            if (Application.isMobilePlatform)
            {
                // Go through all the touch points
                for ( index = 0; index < Input.touchCount; index++ )
                {
                    // Check if we are touching a point to the right of the stethoscopeArea, and move the stethoscope to it
                    if ( Input.GetTouch(index).position.x > stethoscopeArea.position.x ) stethoscope.position = Input.GetTouch(index).position;
                }
            }
            else // Otherwise, use mouse controls
            {
                stethoscope.position = Input.mousePosition;
            }

            // Set the horizontal limit of the stethoscope within the Stethoscope Area width
            float clampX = stethoscope.position.x;
            clampX = Mathf.Clamp(clampX, stethoscopeArea.position.x, stethoscopeArea.position.x + stethoscopeArea.sizeDelta.x * GetComponent<RectTransform>().localScale.x);

            // Set the vertical limit of the stethoscope within the Stethoscope Area height
            float clampY = stethoscope.position.y;
            clampY = Mathf.Clamp(clampY, stethoscopeArea.position.y - stethoscopeArea.sizeDelta.y * GetComponent<RectTransform>().localScale.y, stethoscopeArea.position.y);

            // Set the final stethoscope position
            stethoscope.position = new Vector2(clampX, clampY);

            // Set the volume of the safe based on how close we are to the center of the hotspot
            GetComponent<AudioSource>().volume = 1 - Vector2.Distance(stethoscope.position, hotspot.position)/ hotspotSize;
        }

        // If we have a dial, rotate it either left or right
        if ( dial )
        {
            // Rotate either right, left, or if on a mobile platform rotate based on the buttons we touch. Otherwise stop the dial rotating sound.
            if (Input.GetKey(dialRightButton)) RotateDial(1);
            else if (Input.GetKey(dialLeftButton)) RotateDial(-1);
            else if (Application.isMobilePlatform && rotateDirection != 0) RotateDial(rotateDirection);
            else if (GetComponent<AudioSource>().isPlaying == true) GetComponent<AudioSource>().Stop();
        }
    }
    
    /// <summary>
    /// Rotates the dial either to the left or right. If we are moving in the correct direction it clicks and moves to the next rotation in the sequence until we win.
    /// If we are moving in the wrong direction it resets the sequence and we must start over.
    /// </summary>
    /// <param name="direction"></param>
    public void RotateDial( int direction )
    {
        // Rotate to the next angle in the sequence
        if (sequenceIndex < dialSequence.Length )
        {
            // Check the direction we are moving in, for mobile use
            rotateDirection = direction;

            // Rotate the dial object in the direction we chose
            dial.Rotate(Vector3.forward * dialRotateSpeed * direction * 3.6f * Time.deltaTime, Space.World);

            // count towards the current sequence completion ( moving in the correct direction )
            currentSequence += dialRotateSpeed * direction * 3.6f * Time.deltaTime;

            // Play the dial sound
            if (GetComponent<AudioSource>().isPlaying == false) GetComponent<AudioSource>().PlayOneShot(rotateSound);

            // If we reached the correct spot in the dial rotation, switch direction and move to the next dial angle in the sequence
            if ((dialDirection == 1 && currentSequence >= dialSequence[sequenceIndex]) || (dialDirection == -1 && currentSequence <= dialSequence[sequenceIndex]))
            {
                // Go to next angle in sequence
                sequenceIndex++;

                // Reset the current sequence rotation and switch direction
                if (sequenceIndex < dialSequence.Length)
                {
                    currentSequence = 0;

                    dialDirection *= -1;
                }
                else // Otherwise, if we reached the last dial in the sequence, win the game
                {
                    // Play the deactivation animation
                    if (GetComponent<Animator>()) GetComponent<Animator>().Play("Win");

                    // Run the win event on the lock activator
                    if (activator) activator.Invoke("Win", deactivateDelay);

                    // Play the win sound, if it exists
                    if (winSound && GetComponent<AudioSource>()) GetComponent<AudioSource>().PlayOneShot(winSound);

                    // Unselect any buttons on the dial pad, so we don't press them when pressing 'SPACE' after closing the lock
                    if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
                }

                // Play the click sound
                GetComponent<AudioSource>().PlayOneShot(clickSound);
            }

            // If we moved too much in the wrong direction, reset the entire sequence and start over
            if ((dialDirection == 1 && currentSequence <= -dialReset) || (dialDirection == -1 && currentSequence >= dialReset))
            {
                // Reset the wrong direction checker
                dialResetCount = 0;

                // Reset the correct direction checker
                currentSequence = 0;

                // Reset the dial index ( angle we need to reach )
                sequenceIndex = 0;

                // Reset the initial rotation direction to the right
                dialDirection = 1;

                // Play the reset sound
                GetComponent<AudioSource>().PlayOneShot(resetSound);
            }
        }
    }

    /// <summary>
    /// Activates the lock and starts the lock game
    /// </summary>
    /// <param name="activatorSource"></param>
    public void Activate(LPLockActivator activatorSource)
    {
        // Keep track of the activator so we can deactivate it later
        if (activatorSource) activator = activatorSource;

        // If we are not using a mobile device, hide the dial rotation buttons 
        if (!Application.isMobilePlatform)
        {
            if (GameObject.Find("ButtonRotateRight")) GameObject.Find("ButtonRotateRight").SetActive(false);
            if (GameObject.Find("ButtonRotateLeft")) GameObject.Find("ButtonRotateLeft").SetActive(false);
        }

        // Hold the stethoscope, stethoscope area, and dial objects for quicker access
        if (GameObject.Find("StethoscopeArea")) stethoscopeArea = GameObject.Find("StethoscopeArea").GetComponent<RectTransform>();
        if (GameObject.Find("Stethoscope")) stethoscope = GameObject.Find("Stethoscope").GetComponent<RectTransform>();
        if (GameObject.Find("Dial")) dial = GameObject.Find("Dial").GetComponent<RectTransform>();

        if (GameObject.Find("Hotspot"))
        {
            // Hold the hotspot object for quicker access
            hotspot = GameObject.Find("Hotspot").GetComponent<RectTransform>();

            // Set the size of the hotspot based on the smaller side ( width or height ) we set in the RectTransform
            if (hotspot.sizeDelta.x < hotspot.sizeDelta.y) hotspotSize = hotspot.sizeDelta.x;
            else hotspotSize = hotspot.sizeDelta.y;

            // Hide the image component from the hotspot ( the red do we use to set the size in the editor )
            hotspot.GetComponent<Image>().enabled = false;
        }

        // Set a random hotspot position within the stethoscope area
        hotspot.position = new Vector2(Random.Range(stethoscopeArea.position.x, stethoscopeArea.position.x + stethoscopeArea.sizeDelta.x * GetComponent<RectTransform>().localScale.x), Random.Range(stethoscopeArea.position.y, stethoscopeArea.position.y - stethoscopeArea.sizeDelta.y * GetComponent<RectTransform>().localScale.y));

        // Set a number of dial rotation we need to make to win the game
        dialSequence = new int[dialTurns];

        // Go through and set a random value for each rotation, while alternating between rotating clockwise and counterclockwise
        for (index = 0; index < dialSequence.Length; index++)
        {
            dialSequence[index] = Mathf.RoundToInt(Random.Range(minMaxRotation.x, minMaxRotation.y)) * dialDirection;

            // Alternate direction between left and right
            dialDirection *= -1;
        }

        // Set the initial dial direction to the right
        dialDirection = 1;

        // Reset the current rotation counter
        currentSequence = 0;
    }

    /// <summary>
    /// Deactivates the lock and stops the lock game. This is when we press the abort button
    /// </summary>
    public void Deactivate()
    {
        // Reset the wrong direction checker
        dialResetCount = 0;

        // Reset the correct direction checker
        currentSequence = 0;

        // Reset the dial index ( angle we need to reach )
        sequenceIndex = 0;

        // Reset the initial rotation direction to the right
        dialDirection = 1;

        // Play the deactivate animation
        if (GetComponent<Animator>()) GetComponent<Animator>().Play("Deactivate");

        // Deactivate the source lock activator
        if (activator) activator.Invoke("DeactivateObject", deactivateDelay);
    }
}
