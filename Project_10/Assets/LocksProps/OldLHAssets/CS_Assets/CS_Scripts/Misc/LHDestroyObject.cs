using UnityEngine;

namespace LHToolkit
{
	/// <summary>
	/// Destroy function on a target after a delay. If no target it set, the function runs on this gameobject.
	/// </summary>
	public class LHDestroyObject : MonoBehaviour
	{
		public Transform destroyTarget;
		public float delay = 0;

		/// <summary>
		/// Destroys the object/target.
		/// </summary>
		public void DestroyObject ()
		{
			// Destroy the target, if it exists. Otherwise, destroy this game object
			if( destroyTarget )
				Destroy(destroyTarget.gameObject, delay);
			else
				Destroy(gameObject, delay);
		}
	}
}
