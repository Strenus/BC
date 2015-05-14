using UnityEngine;
using System.Collections;

public class SpriteScript : MonoBehaviour 
{	
	// Use this for initialization
	void Start () 
	{
		if(this.name == "pauseBox")
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 2.0f, -Screen.height / 2.0f, Screen.width, Screen.height);
		}

		if(this.name == "buttonCamera")
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 3, -Screen.height / 12, Screen.width * 2 / 3, Screen.height / 6);
			return;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{	
		
	}
}
