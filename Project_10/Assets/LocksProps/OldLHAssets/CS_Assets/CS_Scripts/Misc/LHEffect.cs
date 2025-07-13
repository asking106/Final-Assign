using System;
using UnityEngine;

namespace LHToolkit.Misc
{
	/// <summary>
	/// creates effects at a position, and removes them after a while.
	/// </summary>
	public class LHEffect : MonoBehaviour
	{
		public float removeAfter = 2;
		public LHEffects[] effects; // List of effects to create.
		
		/// <summary>
		/// Creates the effect(s).
		/// </summary>
		public void CreateEffect ()
		{
			// Go through all effects
			foreach( LHEffects effect in effects )
			{
				Transform newEffect = null;

				// Create a new effect at the correct position
				if( effect.createAt )
				{
					newEffect = (Transform)Instantiate(effect.effect, effect.createAt.position, effect.createAt.rotation);
				}
				else
				{
					newEffect = (Transform)Instantiate(effect.effect, Camera.main.transform.position, Camera.main.transform.rotation);
				}

				// Set the remove time.
				if( effect.removeAfter > 0 )
				{
					Destroy(newEffect.gameObject, effect.removeAfter);
				}
			}
			
			// Set the remove time for this object (the object that creates the effects)
			if( removeAfter > 0 )
			{
				Destroy(gameObject, removeAfter);
			}
		}

		/// <summary>
		/// Represents an effect and various values.
		/// </summary>
		[Serializable]
		public class LHEffects
		{
			// The effect object, where to create it at, and after how many seconds to remove it.
			public Transform effect;
			public Transform createAt;
			public float removeAfter = 2;
		}		
	}
}
