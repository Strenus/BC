using UnityEngine;
using System.Collections;

public class textMenuScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

		if((this.name == "textStart") || (this.name == "text3"))
		{
			this.guiText.pixelOffset = new Vector2(0, -Screen.height * 46 / 100 + Screen.height / 12);
			this.guiText.fontSize = Screen.height / 12;
		}

		if(this.name == "text2")
		{
			this.guiText.pixelOffset = new Vector2(0, -Screen.height * 28 / 100 + Screen.height / 12);
			this.guiText.fontSize = Screen.height / 12;
		}

		if(this.name == "text1")
		{
			this.guiText.pixelOffset = new Vector2(0, -Screen.height * 10 / 100 + Screen.height / 12);
			this.guiText.fontSize = Screen.height / 12;
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
