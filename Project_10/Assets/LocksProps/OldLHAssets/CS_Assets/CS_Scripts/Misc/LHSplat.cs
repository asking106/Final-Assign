using UnityEngine;

/// <summary>
/// Displays a splat, and fades it out as it slides down.
/// </summary>
public class LHSplat : MonoBehaviour
{
	// How quickly the splat fades out
	public float fadeSpeed = -1;

	// The speed at which the splat spreads
	public float splat = 1.2f;

	// The movement speed of the splat
	public Vector3 moveSpeed;

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	public void Update ()
	{
		// Scale the splat object up to give a splat effect
		if( Mathf.Abs(splat) > 1.01f )
		{
			// Scale the splat to give an effect of smacking against the camera
			transform.localScale *= splat;
			splat -= (splat - 1) * 0.5f;
		}
		else
		{
			// Fade out the splat object
			Color newColorFade = GetComponent<Renderer>().material.color;
			newColorFade.a = fadeSpeed * Time.deltaTime;

			GetComponent<Renderer>().material.color = newColorFade;

			transform.position += moveSpeed * Time.deltaTime;
		}
	}
}

