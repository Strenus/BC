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
		background = Resources.Load("Textures/bg", typeof(Texture2D)) as Texture2D;
	}

	void setBackground(Texture2D background)
	{
		this.background = background;
	}
}