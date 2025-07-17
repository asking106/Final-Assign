using UnityEngine;
using System.Collections;
using LHToolkit.Misc; //Using this namespace from the script LHInventory.CS. We refer to this when accessing inventory of the player

public class LHPickup : MonoBehaviour {

	public string pickupName = "bobbypins"; // The name of the pickup object. It is compared to see if we have a slot by that name in the player's inventory. If we don't have such a name, the object will not be added

	public int count = 2; // The amount of this object that will be added to the inventory when picked up

	public string playerTag = "Player"; // The tag of the player object which will be able to collect inventory items

	private bool isPickedUp = false; // Has this object been picked up?

	public bool destroyOnPickup = false; // Completely destroy this object when picked up. It will not be available again in the game.

	public float respawnTime = 10; // How many seconds to wait before respawning this object, so it can be picked up again.
	private float respawnTimeCount = 0; // A counter for the respawn time

	public int respawnLimit = 0; // How many times is this object allowed to respawn and be picked up again. If set to 0, the object will respawn infinitely.

	// Use this for initialization
	void Start() 
	{
	
	}
	
	// Update is called once per frame
	void Update() 
	{
		if ( respawnTimeCount > 0 )
		{
			respawnTimeCount -= Time.deltaTime;
		}
		else if ( isPickedUp == true )
		{
			isPickedUp = false;

			respawnTimeCount = 0;
			
			GetComponent<Collider>().enabled = true;
			GetComponent<MeshRenderer>().enabled = true;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		//Check if the player touched the pickup object
		if ( other.tag == playerTag )
		{
			//Add the pickup object to the player's inventory. The name of the pickup object must be defined in the player's inventory for it to be added.
			other.GetComponent<LHInventory>().UpdateInventory( pickupName, count);

			//Remove, or otherwise set a respawn time for the pickup object
			if ( destroyOnPickup )    Destroy(gameObject);
			else if ( respawnTime > 0 )    
			{
				isPickedUp = true;

				//Reset respawn time
				respawnTimeCount = respawnTime;

				//Disable the pickup object, but don't destroy it because it will respawn later
				GetComponent<Collider>().enabled = false;
				GetComponent<MeshRenderer>().enabled = false;
			}
		}
	}
}
