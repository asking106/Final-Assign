using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// This class defines a Dial Pad, which is a type of lock similar to the game "Simon"; in it you must repeat a sequence
/// of tones succesfully in order to win. You can set a random seuqence or a predetermined sequence in the lock.
/// </summary>
public class LPDialPad : MonoBehaviour
{
    // The lock activator, which can trigger Win or Lose actions
    internal LPLockActivator activator;

    [Tooltip("A list of buttons on the dial pad")]
    public Button[] dialButtons;

    [Tooltip("The sequence of buttons that need to be pressed in order to win. If you enter these manually, the game will always start with them. If you leave them empty, the game will be entirely random")]
    public int[] sequence;
    internal int[] sequenceTemp;
    internal int sequenceIndex = 0;

    [Tooltip("The delay between each two tones in seconds. Lower number means faster dialing and harder game")]
    public float sequenceSpeed = 0.5f;

    [Tooltip("The number of sequence rounds we need to pass in order to win")]
    public int rounds = 5;
    internal int roundsCount = 0;
    internal int mistakesCount = 0;

    [Tooltip("The text object on the dial pad that displays current sequence, win/lose status")]
    public Text screenText;

    [Tooltip("Show the user input when pressing buttons. For example we will see 123 on the screen when the player presses the 123 buttons")]
    public bool showUserInput = false;

    [Tooltip("Allow the user to enter the full sequence before checking if it is correct or not")]
    public bool fullInputBeforeCheck = false;

    [Tooltip("The message for winning/losing, and the current sequence round")]
    public string winMessage = "SUCCESS";
    public string loseMessage = "FAIL";
    public string roundMessage = "SEQUENCE ";

    [Tooltip("How long to wait before deactivating the lock object when we exit the game, either after winning/losing")]
    public float deactivateDelay = 1;

    [Tooltip("Change the dial sound pitch based on the index of the button we press. First button pitch is lowest, last button pitch is highest")]
    public bool dynamicDialSound = true;

    [Tooltip("The sound that plays when we press a dial pad button. This is pitched based on the button to give a different tone like in a real phone")]
    public AudioClip dialSound;

    [Tooltip("The sound that plays when we win the lock game")]
    public AudioClip winSound;

    [Tooltip("The sound that plays when we fail the lock game")]
    public AudioClip loseSound;

    internal int index;

    public void Start()
    {
        if (activator == null) Activate(null);
    }

    /// <summary>
    /// Adds a random note to the sequence
    /// </summary>
    public void AddNote()
    {
        // Keep the current sequence in a temporary array
        sequenceTemp = sequence;

        // Create a new sequence with one extra note slot
        sequence = new int[sequence.Length + 1];

        // Add the current sequence to the new array
        for (index = 0; index < sequenceTemp.Length; index++) sequence[index] = sequenceTemp[index];

        // Choose a random dial button ( a random note )
        int randomNote = Mathf.FloorToInt(Random.Range(0, dialButtons.Length));

        // Set the random note as the last note in the sequence
        sequence[sequence.Length - 1] = randomNote;

        // Play the sequence of notes
        StartCoroutine("PlaySequence");
    }

    /// <summary>
    /// Play the sequence of notes, highlight each played button, and play each note with the relevant pitch
    /// </summary>
    /// <returns></returns>
    IEnumerator PlaySequence()
    {
        // Clear the message text
        //if (screenText) screenText.text = "";

        // Unselect any buttons on the dial pad
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);

        // Deactivate all the dial buttons so the player can't interact with them while the sequence is playing
        for (index = 0; index < dialButtons.Length; index++) dialButtons[index].interactable = false;

        // Delay for 1 second between rounds
        yield return new WaitForSeconds(1);

        // Show the current round on the screen text
        if (screenText) screenText.text = roundMessage + sequence.Length;

        // Go through the sequence, play each note at the correct pitch, and highlight the button
        foreach (int note in sequence)
        {
            // Set the pitch based on the button index ( first button pitch is lowest, last button pitch is highest)
            GetComponent<AudioSource>().pitch = 0.8f + note * 0.05f;

            // Play the dial sound
            GetComponent<AudioSource>().PlayOneShot(dialSound);

            // Play the button highlight animation
            dialButtons[note].GetComponent<Animator>().Play("ButtonPress");

            // Delay for a while before playing the next nore in the sequence
            yield return new WaitForSeconds(sequenceSpeed);
        }

        // Activate all the dial buttons so the player can interact with them again
        for (index = 0; index < dialButtons.Length; index++) dialButtons[index].interactable = true;

        //if (EventSystem.current) EventSystem.current.SetSelectedGameObject(dialButtons[0].gameObject);
    }


    /// <summary>
    /// Presses a button on the dial pad, playing the relevant note and checking if we are following the sequence or not
    /// </summary>
    /// <param name="sourceButton"></param>
    public void PressButton(Button sourceButton)
    {
        // Find the index of the button we pressed so we can compare it in the sequence and play the correct pitch
        int buttonIndex = sourceButton.transform.GetSiblingIndex();

        // Set the pitch based on the button index ( first button pitch is lowest, last button pitch is highest)
        GetComponent<AudioSource>().pitch = 0.8f + buttonIndex * 0.05f;

        // Play the dial sound
        GetComponent<AudioSource>().PlayOneShot(dialSound);

        // Play the button highlight animation
        dialButtons[buttonIndex].GetComponent<Animator>().Play("ButtonPress");

        // Show user input on the screen as it happens
        if (showUserInput == true && screenText && sourceButton.GetComponentInChildren<Text>())
        {
            // If this is the first button we press in the sequence, clear the displayed text
            if (sequenceIndex == 0) screenText.text = "";

            // Add the input to the displayed sequence text
            screenText.text += sourceButton.GetComponentInChildren<Text>().text;
        }

        // If we pressed the wrong note in the sequence, add to the mistakes count
        if (buttonIndex != sequence[sequenceIndex])
        {
            mistakesCount++;

            // If we are not supposed to enter full input before checking the sequence, immediately fail
            if (fullInputBeforeCheck == false) FailSequence();
        }

        // Go to the next note in the sequence
        sequenceIndex++;

        // If we reached the end of the sequence, check if we passed, and go to the next round
        if (sequenceIndex >= sequence.Length)
        {
            if (mistakesCount > 0) // Otherwise, if we pressed the wrong note we lose the game
            {
                FailSequence();
            }
            else if (roundsCount < rounds) // Go to the next round and add a note
            {
                // Increase round count
                roundsCount++;

                // Reset the sequence note index
                sequenceIndex = 0;

                // Add a note to the sequence
                AddNote();
            }
            else // Otherwise, if we reached the last round, win the game
            {
                // Display the win message
                if (screenText) screenText.text = winMessage;

                // Play the deactivation animation
                if (GetComponent<Animator>()) GetComponent<Animator>().Play("Win");

                // Run the win event on the lock activator
                if (activator) activator.Invoke("Win", deactivateDelay);

                // Play the win sound, if it exists
                if (winSound && GetComponent<AudioSource>()) GetComponent<AudioSource>().PlayOneShot(winSound);

                // Unselect any buttons on the dial pad, so we don't press them when pressing 'SPACE' after closing the lock
                if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void FailSequence()
    {
        // Deactivate all the dial buttons so the player can't interact with them after we lose
        for (index = 0; index < dialButtons.Length; index++) dialButtons[index].interactable = false;

        // Reset the sequence index
        sequenceIndex = 0;

        // Display the lose message
        if (screenText) screenText.text = loseMessage;

        // Play the deactivation animation
        if (GetComponent<Animator>()) GetComponent<Animator>().Play("Fail");

        // Run the lose event on the lock activator
        if (activator) activator.Invoke("Lose", deactivateDelay);

        // Play the lose sound, if it exists
        if (loseSound && GetComponent<AudioSource>()) GetComponent<AudioSource>().PlayOneShot(loseSound);

        // Unselect any buttons on the dial pad, so we don't press them when pressing 'SPACE' after closing the lock
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);

    }

    /// <summary>
    /// Activates the lock and starts the lock game
    /// </summary>
    /// <param name="activatorSource"></param>
    public void Activate(LPLockActivator activatorSource)
    {
        // Keep track of the activator so we can deactivate it later
        if ( activatorSource )    activator = activatorSource;

        // Reset the round count. This is for example when you start playing and then abort the game, when you play again the round starts from 1
        roundsCount = 1;

        // Reset the mistakes count
        mistakesCount = 0;

        // If the sequence is not preset, disable the buttons and add a note to it
        if (sequence.Length <= 0)
        {
            // Deactivate all the dial buttons so the player can't interact with them while the sequence is playing
            for (index = 0; index < dialButtons.Length; index++) dialButtons[index].interactable = false;

            // Add a note
            AddNote();
        }
        else
        {
            // Activate all the dial buttons so the player can interact with them again
            for (index = 0; index < dialButtons.Length; index++) dialButtons[index].interactable = true;
        }

        // Show the first round on the screen text
        if (screenText)
        {
            // If the game has just one round, don't display a number next to the round message
            if (rounds <= 1) screenText.text = roundMessage;
            else screenText.text = roundMessage + sequence.Length;
        }
    }

    /// <summary>
    /// Deactivates the lock and stops the lock game. This is when we press the abort button
    /// </summary>
    public void Deactivate()
    {
        // Reset the sequence when we quit the lock game
        if (sequence.Length > 0 && rounds > 1) sequence = new int[0];

        // Reset the sequence index
        sequenceIndex = 0;

        // Play the deactivate animation
        if (GetComponent<Animator>()) GetComponent<Animator>().Play("Deactivate");

        // Deactivate the source lock activator
        if (activator) activator.Invoke("DeactivateObject", deactivateDelay);
    }
}
