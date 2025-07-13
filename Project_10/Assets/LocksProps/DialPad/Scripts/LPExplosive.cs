using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines an explosion and splat effect that trigger on command
/// </summary>
public class LPExplosive : MonoBehaviour
{
    [Tooltip("The explosion effect created at the position of the detonated object")]
    public Transform explodeEffect;

    [Tooltip("The splat effect that appears and is attaced to the camera")]
    public Transform cameraEffect;

    [Tooltip("The physical power pushing the player away from the center of the explosion")]
    public float explodePower = 10;

    [Tooltip("The sound that will be played")]
    public AudioClip explodeSound;

    /// <summary>
    /// Detonates, playing particle effects and a sound
    /// </summary>
    public void Detonate()
    {
        // If there is an explosion effect, create it
        if (explodeEffect) Instantiate(explodeEffect, transform.position, transform.rotation);

        // If there is a camera and a camera effect, create it and attach it to the camera
        if (cameraEffect && Camera.main)
        {
            // Create the camera effect ( ex: blood splat )
            Transform newCameraEffect = Instantiate(cameraEffect, Camera.main.transform.position, Camera.main.transform.rotation);

            // Attach the effect to the camera
            newCameraEffect.SetParent(Camera.main.transform);
        }

        // If there is an explosion power, apply it to the player's regidbody
        if (explodePower > 0)
        {
            // Find the rigidbody of the player
            Rigidbody playerObject = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

            // Apply the explosion to the player
            playerObject.AddExplosionForce(explodePower, transform.position, 5, 2);
        }

        // Play the sound effect
        if (GetComponent<AudioSource>() && explodeSound) GetComponent<AudioSource>().PlayOneShot(explodeSound);

        Destroy(gameObject);
    }
}
