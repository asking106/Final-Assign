using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines a door that can be opened and closed
/// </summary>
public class LPDoor : MonoBehaviour
{
    // The animator of the door that has two animations, "DoorOpen" and "DoorClose"
    internal Animator doorAnimator;

    // Use this for initialization
    void Start()
    {
        // If there is an animator, assign it
        if (GetComponentInChildren<Animator>()) doorAnimator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Opens the door if it is closed, and closes it if it is open
    /// </summary>
    public void ToggleDoor()
    {
        // If there is a door animator, toggle the open/close states
        if (doorAnimator)
        {
            // Play the relevant animation for the door
            doorAnimator.SetTrigger("ToggleDoor");
        }
    }

    /// <summary>
    /// Plays a chosen sound from the current audio clip
    /// </summary>
    /// <param name="sound"></param>
    public void PlaySound(AudioClip sound)
    {
        if (GetComponent<AudioSource>()) GetComponent<AudioSource>().PlayOneShot(sound);
    }
}
