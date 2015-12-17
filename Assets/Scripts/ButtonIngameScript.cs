using UnityEngine;
using System.Collections;

public class ButtonIngameScript : MonoBehaviour 
{	
	bool touched = false;

	// Use this for initialization
	void Start () 
	{
		if((this.name == "buttonPause") || (this.name == "buttonEditPause") || (this.name == "buttonEditPlus") || (this.name == "buttonEditMinus") 
		   || (this.name == "buttonEditOK") || (this.name == "buttonCamPlus") || (this.name == "buttonCamMinus"))
		{
			this.guiTexture.pixelInset = new Rect ( -Screen.height / 16, -Screen.height / 16 , Screen.height / 8, Screen.height / 8);
			return;
		}

		if(this.name == "buttonEditColor")
		{
			this.guiTexture.pixelInset = new Rect ( -Screen.height / 12, -Screen.height / 12 , Screen.height / 6, Screen.height / 6);
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

			if((touch.phase == TouchPhase.Began) && (this.guiTexture.HitTest(touch.position)))
			{
				touched = true;
			}
			if((touch.phase == TouchPhase.Ended) && (this.guiTexture.HitTest(touch.position)))
			{
				if (touched == true)
				{
					touched = false;
					GameObject.FindGameObjectWithTag("GameController").SendMessage(this.name);
				}
			}
			if(!this.guiTexture.HitTest(touch.position))
			{
				touched = false;
			}
		}

	}
	/*
	void OnMouseDown()
	{
		GameObject.FindGameObjectWithTag("GameController").SendMessage(this.name);
	}*/
}
