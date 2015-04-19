using UnityEngine;
using System.Collections;

public class ButtonTextScript: MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.guiTexture.pixelInset = new Rect (-Screen.width / 6, -Screen.height * 5 / 12, Screen.width / 3, Screen.height / 6);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
