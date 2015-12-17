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
					GameObject.FindGameObjectWithTag("GameController").SendMessage("buttonLevel",uint.Parse(this.name));
				}
			}
		}	
	}
	/*
	void OnMouseDown()
	{
		GameObject.FindGameObjectWithTag("GameController").SendMessage("buttonLevel",uint.Parse(this.name));
	}*/
}
