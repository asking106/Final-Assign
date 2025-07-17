using System;
using UnityEngine;

namespace LHToolkit.Misc
{
	/// <summary>
	/// This abstract class simply holds the number of lockpicks the player has. As long as you have it placed somewhere in your 
	/// project ( No need to attach it to any object ) it will keep track of the player's lockpicks
	/// </summary>
	public class LHInventory : MonoBehaviour
	{
		public Inventory[] inventory;

		[Serializable]
		public class Inventory
		{
			public string tool = "lockpicks";
			public int    count = 1;
		}

		public void UpdateInventory ( string toolName, int change )
		{
			//Go through all tools in the inventory
			for ( var index = 0 ; index < inventory.Length ; index++ )
			{
				// If we have the correct tool name, and we have at least 1 of it, return true
				if ( inventory[index].tool == toolName )
				{
					inventory[index].count += change;

					return;
				}
			}

			Debug.LogWarning("Pickupable item '" + toolName + "' does not exist in the player's inventory. Item will not be added until you define it in the player's inventory."); 

		}
	}
}


