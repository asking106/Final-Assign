using UnityEngine;
using System.Collections;

namespace LHToolkit.Misc
{
	public class LHMessage : MonoBehaviour 
	{
		public GUISkin guiSkin; // GUI for button graphic.
		
		private Rect labelRect = new Rect( 0, Screen.height - 40, Screen.width, 40); // Caching rect of GUI Label Rect, so we don't do it each OnGUI Call.
		
		/// <summary>
		/// OnGUI is called for rendering and handling GUI events.
		/// This means that your OnGUI implementation might be called several times per frame (one call per event).
		/// For more information on GUI events see the Event reference. If the MonoBehaviour's enabled property is 
		/// set to false, OnGUI() will not be called.
		/// </summary>
		public void OnGUI() 
		{
		    GUI.skin = guiSkin;
		    
		    // Some explanation of how to play
		    GUI.Label(labelRect, "Press A/D to move left/right. Press F to interact with objects");
		}
	}
}