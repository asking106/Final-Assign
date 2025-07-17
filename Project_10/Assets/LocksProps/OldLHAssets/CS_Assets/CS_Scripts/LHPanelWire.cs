using UnityEngine;
using System.Collections;

namespace LHToolkit
{
	/// <summary>
	/// This script defines a panel wire, which is used in the panel lock type. The panel wire check for collision
	/// with any other panel wires and changes the color of the wire accordingly. In the panel lock, part of the
	/// game goal is to make sure no wires are intersecting.
	/// </summary>
	public class LHPanelWire : MonoBehaviour
	{
		// The tag of the panel wire
		public string panelWireTag = "LHpanelWire";
		
		// The color of the wire when intersecting with another wire
		public Color wrongWireColor;
		private Color defaultWireColor;
		
		// Is the wire intersecting?
		internal bool isIntersecting = false;
		
		public void Start() 
		{
			// Set the default color of the wire
			defaultWireColor = GetComponent<Renderer>().material.GetColor("_Emission");
		}
		
		// Check if the wire starts colliding with other wires
		public void OnTriggerEnter(Collider other)
		{
			if ( other.tag == panelWireTag )
			{
				// Change the color fo the wire to the intersecting color
				GetComponent<Renderer>().material.SetColor("_Emission", wrongWireColor);
				
				// This wire is intersecting with another wire
				isIntersecting = true;
			}
		}
		
		// Check if the wire stops colliding with other wires
		public void OnTriggerExit(Collider other)
		{
			if ( other.tag == panelWireTag )
			{
				// Change the color fo the wire to the default color
				GetComponent<Renderer>().material.SetColor("_Emission", defaultWireColor);
				
				// This wire is not intersecting with another wire
				isIntersecting = false;
			}
		}
	}
}
