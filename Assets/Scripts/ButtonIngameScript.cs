using UnityEngine;
using System.Collections;

public class ButtonIngameScript : MonoBehaviour 
{
	public Texture idle;
	public Texture hover;
	
	// Use this for initialization
	void Start () {		
		if(this.name == "buttonHammer")
		{
			this.guiTexture.pixelInset = new Rect (- Screen.width / 2 + Screen.width / 100, - Screen.height / 2 + Screen.height/50 , Screen.width / 10, Screen.width / 10);
		}
		
		if(this.name == "buttonBrush")
		{
			this.guiTexture.pixelInset = new Rect (- Screen.width / 2 + Screen.width / 100, - Screen.height / 4 , Screen.width / 10, Screen.width / 10);
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
