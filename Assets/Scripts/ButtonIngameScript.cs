using UnityEngine;
using System.Collections;

public class ButtonIngameScript : MonoBehaviour 
{	
	// Use this for initialization
	void Start () 
	{
		if(this.name == "buttonPause")
		{
			this.guiTexture.pixelInset = new Rect ( -Screen.width / 32, -Screen.width / 32 , Screen.width / 16, Screen.width / 16);
			return;
		}
		if((this.name == "buttonCamPlus") || (this.name == "buttonCamMinus"))
		{
			this.guiTexture.pixelInset = new Rect ( -Screen.height / 20, -Screen.height / 20 , Screen.height / 10, Screen.height / 10);
			return;
		}

		this.guiTexture.pixelInset = new Rect ( -Screen.width / 16, -Screen.width / 16 , Screen.width / 8, Screen.width / 8);
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
