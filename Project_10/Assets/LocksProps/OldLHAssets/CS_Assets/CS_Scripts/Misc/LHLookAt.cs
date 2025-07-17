using UnityEngine;

namespace LHToolkit
{
	/// <summary>
	/// Makes an object look at another. Used mainly to make billboard style objects that always align with the camera.
	/// </summary>
	public class LHLookAt : MonoBehaviour
	{
		public Transform lookAtObject;
	
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update ()
		{
			// Keep looking at the object
			transform.LookAt(lookAtObject);
		}
	}
}
