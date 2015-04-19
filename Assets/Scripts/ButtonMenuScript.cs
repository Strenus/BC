using UnityEngine;
using System.Collections;

public class ButtonMenuScript : MonoBehaviour 
{
	public Texture idle;
	public Texture hover;

	// Use this for initialization
	void Start () {

		if((this.name == "buttonStart") || (this.name == "textStart"))
			this.guiTexture.pixelInset = new Rect (-Screen.width / 6, -Screen.height * 5 / 12, Screen.width / 3, Screen.height / 6);

	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);

			if(!guiTexture.HitTest(touch.position))
			{
				this.guiTexture.texture = idle;
			}
			else
			{
				if(touch.phase != TouchPhase.Ended)
				{
					this.guiTexture.texture = hover;
				}
				else
				{
					this.guiTexture.texture = idle;
					GameObject.FindGameObjectWithTag("GameController").SendMessage("buttonStart");
				}
			}
		}	
	}
}
