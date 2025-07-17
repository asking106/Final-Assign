using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a sound from an audio source.
/// </summary>
public class LPPlaySound : MonoBehaviour
{
    [Tooltip("A preset sound that will be played")]
    public AudioClip sound;

    [Tooltip("The audio source of the sound we play")]
    public AudioSource soundSource;

    [Tooltip("Delay the sound play")]
    public float delay = 0;

    [Tooltip("Play the sound automatically")]
    public bool playOnStart = false;

    [Tooltip("Play the sound when colliding with physical objects")]
    public bool playOnCollision = false;

    public void Start()
    {
        if ( playOnStart == true )
        {
            StartCoroutine("PlaySound", sound);
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (playOnCollision == true)
        {
            StartCoroutine("PlaySound", sound);
        }
    }

    /// <summary>
    /// Plays the chosen sound after a delay
    /// </summary>
    IEnumerator PlaySound( AudioClip sound )
	{
        // Delay for some time
        yield return new WaitForSeconds(delay);

        // If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
        if ( soundSource && sound )    soundSource.PlayOneShot(sound);
	}

    /// <summary>
    /// Plays the current sound clip after a delay
    /// </summary>
    IEnumerator PlayCurrentSound()
    {
        // Delay for some time
        yield return new WaitForSeconds(delay);

        // If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
        if (soundSource && sound) soundSource.PlayOneShot(sound);
    }
}
