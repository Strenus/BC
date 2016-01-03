using UnityEngine;
using System.Collections;

public class ButtonLevelScript : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{		
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
						GameObject.FindGameObjectWithTag("GameController").SendMessage("buttonLevel",uint.Parse(this.name));
					}
				}
			}
			else
			{
				this.guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
			}
		}	
	}

	void OnMouseDown()
	{
		GameObject.FindGameObjectWithTag("GameController").SendMessage("buttonLevel",uint.Parse(this.name));
	}
}
