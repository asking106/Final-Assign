using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines an atomic bomb explosion and effect that trigger on command
/// </summary>
public class LPAtomicBomb : MonoBehaviour
{
    [Tooltip("The explosion effect created at the position of the detonated object")]
    public Transform explodeEffect;

    [Tooltip("The splat effect that appears and is attaced to the camera")]
    public Transform cameraEffect;

    [Tooltip("The power of shaking player camera effect fronm this explosion")]
    public float shockPower = 2;

    [Tooltip("The duration of shaking player camera effect fronm this explosion")]
    public float shockDuration = 5;

    [Tooltip("The delay before the shockwave effect starts")]
    public float shockwaveDelay = 8;

    public void ExplodeEffect()
    {
        // If there is an explosion effect, create it
        if (explodeEffect) Instantiate(explodeEffect, transform.position, transform.rotation);
    }

    public void CameraEffect()
    {
        // If there is a camera and a camera effect, create it and attach it to the camera
        if (cameraEffect && Camera.main)
        {
            // Create the camera effect ( ex: blood splat )
            Transform newCameraEffect = Instantiate(cameraEffect, Camera.main.transform.position, Camera.main.transform.rotation);

            // Attach the effect to the camera
            newCameraEffect.SetParent(Camera.main.transform);
        }
    }

    /// <summary>
    /// Starts a shockwave effect which shakes the player camera for a while and then subsides
    /// </summary>
    /// <returns></returns>
    IEnumerator ShockwaveProgress()
    {
        // Find the rigidbody of the player
        //Rigidbody playerObject = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        // Delay for 1 second between rounds

        // Wait some time before starting the shaking effect
        yield return new WaitForSeconds(shockwaveDelay);

        // Get the main camera in the game
        Transform playerCamera = Camera.main.transform;

        // Remember the initial position of the camera so we can return to it after the shaking is over
        Vector3 initCameraPosition = playerCamera.localPosition;

        // A timeout timer for the first phase of the shaking process, raising shake power
        float timeOut = 0;

        while (timeOut < shockDuration * 0.5f)
        {
            // Randomize the position of the camera across X Y Z to give a shaking effect
            playerCamera.localPosition = initCameraPosition + new Vector3(Random.Range(-timeOut, timeOut), Random.Range(-timeOut, timeOut), Random.Range(-timeOut, timeOut)) * shockPower;

            // Count up
            timeOut += Time.deltaTime;

            // Animate the process
            yield return new WaitForSeconds(Time.deltaTime);
        }

        timeOut = shockDuration * 0.5f;

        // A timeout timer for the second phase of the shaking process, lowering shake power
        while (timeOut > 0)
        {
            // Randomize the position of the camera across X Y Z to give a shaking effect
            playerCamera.localPosition = initCameraPosition + new Vector3(Random.Range(-timeOut, timeOut), Random.Range(-timeOut, timeOut), Random.Range(-timeOut, timeOut)) * shockPower;

            // Count down
            timeOut -= Time.deltaTime;

            // Animate the process
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }


    /// <summary>
    /// Detonates, playing an animation
    /// </summary>
    public void Detonate()
    {
        GetComponent<Animator>().Play("Bomb");
    }
}
