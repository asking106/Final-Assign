using UnityEngine;

namespace LHToolkit
{
	public class LHEnlarge : MonoBehaviour
	{
		/// <summary>
		/// Enlarges object instance attached to.
		/// </summary>
		public void Enlarge ()
		{
			transform.localScale = new Vector3(transform.localScale.x + 0.1f, transform.localScale.y + 0.1f, transform.localScale.z + 0.1f);
		}
	}
}