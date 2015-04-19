using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {


		if (Input.touchCount > 0) 
		{
			if(Input.GetTouch(0).phase == TouchPhase.Began)
			{
				Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch(0).position);
				RaycastHit hit = new RaycastHit();

				if (Physics.Raycast (ray, out hit)) 
				{
					//Debug.Log(" You just hit " + hit.collider.gameObject.name);
					//Destroy(hit.collider.gameObject);

					GameObject gc = GameObject.FindGameObjectWithTag("GameController");

					gc.SendMessage ("breakCube", hit.collider.gameObject);
					gc.SendMessage("checkCompletion");
				}

			}


			transform.LookAt (new Vector3(0, 0, 0), Vector3.up);

			if(!(((transform.position.y > 9.8f) && (Input.touches[0].deltaPosition.y < 0.0f)) || ((transform.position.y < -9.8f) && (Input.touches[0].deltaPosition.y > 0.0f))))
			{
				transform.RotateAround (new Vector3 (0, 0, 0), transform.right, -Input.touches[0].deltaPosition.y / 1.5f);
			}
			transform.RotateAround (new Vector3 (0, 0, 0), transform.up, Input.touches[0].deltaPosition.x / 2.0f);
		}
	}

	/*var platform : RuntimePlatform = Application.platform;
	
	function Update(){
		if(platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer){
			if(Input.touchCount > 0) {
				if(Input.GetTouch(0).phase == TouchPhase.Began){
					checkTouch(Input.GetTouch(0).position);
				}
			}
		}else if(platform == RuntimePlatform.WindowsEditor){
			if(Input.GetMouseButtonDown(0)) {
				checkTouch(Input.mousePosition);
			}
		}
	}
	
	function checkTouch(pos){
		var wp : Vector3 = Camera.main.ScreenToWorldPoint(pos);
		var touchPos : Vector2 = new Vector2(wp.x, wp.y);
		var hit = Physics2D.OverlapPoint(touchPos);
		
		if(hit){
			Debug.Log(hit.transform.gameObject.name);
			hit.transform.gameObject.SendMessage('Clicked',0,SendMessageOptions.DontRequireReceiver);
		}
	}*/
}
