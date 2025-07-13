using UnityEngine;
using System.Collections;

namespace LHToolkit.Misc
{
	/// <summary>
	/// This simple script handles an overhead camera following an object at an offset
	/// </summary>
	public class LHCamera : MonoBehaviour
	{
		public Transform target; // The object to follow.
		public Vector3   offset; // How far from the object the camera hovers.
		public float     followSpeed  = 5; // How fast the camera follows the target.
		public bool      lookAtTarget = true; // Should the camera look at the target?
		
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>		
		public void Update ()
		{
			if ( target )
			{
				// Follow this object at an offset
				transform.position = Vector3.Slerp(transform.position, target.position + offset, followSpeed * Time.deltaTime);
		
				// Look at the followed object
				if (lookAtTarget)
					transform.LookAt(target.position);
			}
			else
			{
				Debug.LogWarning ("Warning! No target set for camera to follow. Pick a target such as the player trasnform.");
			}
		}		
		
	}
}
