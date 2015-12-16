using UnityEngine;

public class CameraBackgroundScript : MonoBehaviour {
	Texture2D background;
	
	void OnPreRender (){
		if( background != null )
		{
			Graphics.Blit( background, RenderTexture.active );
		}
	}	

	void Start ()
	{
		Debug.Log ("innit");
		background = Resources.Load("Textures/bg", typeof(Texture2D)) as Texture2D;
		Debug.Log (background);
	}

	void setBackground(Texture2D background)
	{
		this.background = background;
	}
}