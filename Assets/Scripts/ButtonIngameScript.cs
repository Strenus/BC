using UnityEngine;
using System.Collections;

public class ButtonIngameScript : MonoBehaviour 
{	
	// Use this for initialization
	void Start () 
	{
		if(this.name == "buttonPause")
		{
			this.guiTexture.pixelInset = new Rect ( -Screen.width / 23, -Screen.width / 23 , Screen.width * 2 / 23, Screen.width * 2 / 23);
		}
		else
		{
			this.guiTexture.pixelInset = new Rect ( -Screen.width / 16, -Screen.width / 16 , Screen.width / 8, Screen.width / 8);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);

			if((touch.phase == TouchPhase.Began) && (guiTexture.HitTest(touch.position)))
			{
				GameObject.FindGameObjectWithTag("GameController").SendMessage(this.name);
			}
		}	

	}
}
