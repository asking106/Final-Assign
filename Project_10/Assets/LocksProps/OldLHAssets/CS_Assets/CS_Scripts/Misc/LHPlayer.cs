using UnityEngine;

namespace LHToolkit.Misc
{
	/// <summary>
	/// Simple representation of a player character, allowing you to move left & right.
	/// </summary>
	public class LHPlayer : MonoBehaviour
	{
		public float speed = 10; // The player's speed.

		private string controlsType = string.Empty; // Holds the type of controls we use, mobile or otherwise.

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start ()
		{
			// Detect if we are running on Android or iPhone
			#if UNITY_IPHONE
    	controlsType = "iphone";
    	print("iphone");
			#endif

  		#if UNITY_ANDROID
    	controlsType = "android";
    	print("android");
  		#endif
		}

		/// <summary>
		/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
		/// FixedUpdate should be used instead of Update when dealing with Rigidbody.
		/// For example when adding a force to a rigidbody, you have to apply the force every fixed frame
		/// inside FixedUpdate instead of every frame inside Update.
		/// </summary>
		public void FixedUpdate ()
		{
			// If we are using Android/iPhone, controls are touch based
			if( controlsType == "android" || controlsType == "iphone" )
			{
				// If we are touching the screen
				if( Input.touchCount > 0 )
				{
					// If we are swiping the screen horizontally
					if( Mathf.Abs(Input.GetTouch(0).deltaPosition.x) > 0.5f )
					{
						// Moving horizontally and vertically with touch screen swipes
						transform.Translate(Vector3.right * speed * 0.2f * Input.GetTouch(0).deltaPosition.x * Time.deltaTime, Space.World);
					}
				}
			}
			else
			{
				// Moving horizontally and vertically with WASD
				transform.Translate(-Vector3.right * speed * Input.GetAxis("Horizontal") * Time.deltaTime, Space.World);
			}
		}
	}
}