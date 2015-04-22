using UnityEngine;
using System.Collections;

public class ButtonMenuScript : MonoBehaviour 
{
	// Use this for initialization
	void Start () {

		if((this.name == "buttonStart") || (this.name == "button3"))
			this.guiTexture.pixelInset = new Rect (-Screen.width / 6, -Screen.height * 46 / 100, Screen.width / 3, Screen.height / 6);

		if(this.name == "button2")
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 6, -Screen.height * 28 / 100, Screen.width / 3, Screen.height / 6);
		}

		if(this.name == "button1")
		{
			this.guiTexture.pixelInset = new Rect (-Screen.width / 6, -Screen.height * 10 / 100, Screen.width / 3, Screen.height / 6);
		}

	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);

			if(!guiTexture.HitTest(touch.position))
			{
				this.guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
			}
			else
			{
				if(touch.phase != TouchPhase.Ended)
				{
					this.guiTexture.color = new Color (0.25f, 0.5f, 0.5f, 0.5f);
				}
				else
				{
					this.guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
					GameObject.FindGameObjectWithTag("GameController").SendMessage(this.name);
				}
			}
		}	
	}
}
