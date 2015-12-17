using UnityEngine;
using System.Collections;

public class ButtonMenuScript : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		if(this.name == "buttonPicube")
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 4, -Screen.height / 9, Screen.width / 2, Screen.height * 2 / 9);
			return;
		}

		if((this.name == "buttonStart") || (this.name == "buttonSettings") || (this.name == "pauseContinue") || (this.name == "pauseExit") || (this.name == "buttonBack"))
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 6, -Screen.height / 12, Screen.width / 3, Screen.height / 6);
			return;
		}
		
		if((this.name == "buttonStage1") || (this.name == "buttonStage2") || (this.name == "buttonStage3") || (this.name == "buttonStage4") || (this.name == "buttonLevelBack")
		   || (this.name == "buttonLevelCreate") || (this.name == "buttonStageBack") || (this.name == "buttonLevelDelete")  || (this.name == "buttonSetYes")
		   || (this.name == "buttonSetNo"))
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 8, -Screen.height / 12, Screen.width / 4, Screen.height / 6);
			return;
		}

		if(this.name == "buttonReset")
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 4, -Screen.height / 12, Screen.width / 2, Screen.height / 6);
			return;
		}

		if(this.name == "buttonCamera")
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 3, -Screen.height / 12, Screen.width * 2 / 3, Screen.height / 6);
			return;
		}

		this.guiTexture.pixelInset = new Rect (-Screen.height / 12, -Screen.height / 12, Screen.height / 6, Screen.height / 6);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			
			
			
			if(guiTexture.HitTest(touch.position))
			{
				if(touch.phase == TouchPhase.Began)
				{
					this.guiTexture.color = new Color (0.25f, 0.5f, 0.5f, 0.5f);
				}
				if(touch.phase == TouchPhase.Ended)
				{
					if(this.guiTexture.color.r == 0.25f)
					{
						this.guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
						GameObject.FindGameObjectWithTag("GameController").SendMessage(this.name);
					}
				}
			}
			else
			{
				this.guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
			}
		}
	}
	/*
	void OnMouseDown()
	{
		GameObject.FindGameObjectWithTag("GameController").SendMessage(this.name);
	}*/
}
