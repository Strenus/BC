using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	GameObject justTouched;

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
					justTouched = hit.collider.gameObject;
				}
			}

			if(Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch(0).position);
				RaycastHit hit = new RaycastHit();
				
				if (Physics.Raycast (ray, out hit)) 
				{					
					if(justTouched != null)
					{
						if(hit.collider.gameObject.Equals(justTouched))
						{
							GameObject.FindGameObjectWithTag("GameController").SendMessage ("touchCube", hit.collider.gameObject);
						}
						justTouched = null;
					}
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
}
