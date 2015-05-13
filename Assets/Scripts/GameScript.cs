﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	public Cube[, ,] grid;

	public bool[, ,] levelArray;

	public Material[] numMat = new Material[10];

	List<GameObject> fragments = new List<GameObject>();

	public List<GameObject> buttonsMenu = new List<GameObject>();
	public List<GameObject> buttonsIngame = new List<GameObject>();
	public List<GameObject> starsIngame = new List<GameObject>();
	ushort score;

	GameObject justTouched;
	ushort touchTicks = 0;

	ushort ticks = 0;
	bool breaking = true;
	bool pause = true;
	bool inMenu = true;



	void Start () 
	{
		DontDestroyOnLoad(this.gameObject);

		foreach(GameObject go in starsIngame)
		{
			go.guiTexture.pixelInset = new Rect (-Screen.height / 20, -Screen.height / 20, Screen.height / 10, Screen.height / 10);
		}

		spawnMenuCubes ();

		Debug.Log (Screen.width);
		Debug.Log (Screen.height);
	}

	void Update () 
	{
		if(!pause)
		{
			ticks++;

			if (ticks % 60 == 0)
			{
				if(checkCompletion())
				{
					openMainMenu();
				}
			}
		}

		//---TouchControl------------------------------------------------------
		Camera cam = GameObject.FindGameObjectWithTag ("MainCamera").camera;

		if(!pause)
		{
			if (Input.touchCount > 0)
			{
				if(Input.GetTouch(0).phase == TouchPhase.Began)
				{
					Ray ray = cam.ScreenPointToRay (Input.GetTouch(0).position);
					RaycastHit hit = new RaycastHit();
					
					if (Physics.Raycast (ray, out hit)) 
					{
						justTouched = hit.collider.gameObject;
					}
				}
				
				if(Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					Ray ray = cam.ScreenPointToRay (Input.GetTouch(0).position);
					RaycastHit hit = new RaycastHit();
					
					if (Physics.Raycast (ray, out hit)) 
					{					
						if(justTouched != null)
						{
							if(hit.collider.gameObject.Equals(justTouched))
							{
								touchCube (hit.collider.gameObject);
							}
							justTouched = null;
						}
					}
				}
				
				cam.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
				
				if(!(((cam.transform.position.y > 9.8f) && (Input.GetTouch(0).deltaPosition.y < 0.0f)) || ((cam.transform.position.y < -9.8f) && (Input.GetTouch(0).deltaPosition.y > 0.0f))))
				{
					cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, -Input.GetTouch(0).deltaPosition.y * 800.0f / Screen.height);
					//cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, -Input.GetTouch(0).deltaPosition.y * 300.0f / Screen.height);
				}
				cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.up, Input.GetTouch(0).deltaPosition.x * 600.0f / Screen.height);
				//cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.up, Input.GetTouch(0).deltaPosition.x * 230.0f / Screen.height);
			}
		}

		//---MainMenuAnimation-------------------------------------------------

		if(inMenu)
		{
			cam.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
			cam.transform.RotateAround (new Vector3 (0, 0, 0), transform.up, 0.5f);
		}

		//---Cleaning cube fragments------------------------------------------
		fragments.ForEach(delegate(GameObject fragment)
		{
			fragment.transform.localScale = new Vector3(fragment.transform.localScale.x - 0.015f, fragment.transform.localScale.y - 0.015f, fragment.transform.localScale.z - 0.015f);
			
			if(fragment.transform.localScale.x < 0)
			{
				fragments.Remove(fragment);
				GameObject.Destroy(fragment);
			}
		});	
	}

	void SpawnCubes () 
	{
		//---Nastavenie kamery zo zakladnej pozicie------------------------------

		int tempMax = Mathf.Max (levelArray.GetLength (0), Mathf.Max (levelArray.GetLength (1), levelArray.GetLength (2)));

		GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera");
		camera.transform.position = new Vector3 (0, 0, 10);
		camera.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
		camera.camera.fieldOfView = tempMax * 10f;

		//---Vytvorenie kociek--------------------------------------------------
		if(grid != null)
		{
			foreach (Cube cube in grid) 
			{
				if(cube != null)
					Destroy(cube.cube);
			}
			grid = null;
		}
		grid = new Cube[levelArray.GetLength (0), levelArray.GetLength (1), levelArray.GetLength (2)];

		for (int x=0; x<levelArray.GetLength(0); x++)
		{
			for (int y=0; y<levelArray.GetLength(1); y++)
			{
				for (int z=0; z<levelArray.GetLength(2); z++)
				{
					
					grid[x,y,z] = new Cube();
					grid[x,y,z].cube.transform.position = new Vector3(x - levelArray.GetLength(0)/2.0f + 0.5f,y - levelArray.GetLength(1)/2.0f + 0.5f,z - levelArray.GetLength(2)/2.0f + 0.5f);
					
					sbyte tempCount = 0;			
					
					for(int i=0;i<levelArray.GetLength(2);i++)
					{
						if(levelArray[x,y,i] == true)
							tempCount++;
					}
					grid[x,y,z].setSide (0, tempCount, numMat[tempCount]);
					grid[x,y,z].setSide (2, tempCount, numMat[tempCount]);
					
					tempCount = 0;
					for(int i=0;i<levelArray.GetLength(0);i++)
					{
						if(levelArray[i,y,z] == true)
							tempCount++;
					}
					grid[x,y,z].setSide (1, tempCount, numMat[tempCount]);
					grid[x,y,z].setSide (3, tempCount, numMat[tempCount]);
					
					tempCount = 0;
					for(int i=0;i<levelArray.GetLength(1);i++)
					{
						if(levelArray[x,i,z] == true)
							tempCount++;
					}
					grid[x,y,z].setSide (4, tempCount, numMat[tempCount]);
					grid[x,y,z].setSide (5, tempCount, numMat[tempCount]);
					
					
				}
			}
		}
	}

	void touchCube (GameObject cube)
	{
		if (breaking)
		{
			if(cube.renderer.materials[0].color.r == 1)
			{
				for (int x=0; x<levelArray.GetLength(0); x++)
				{
					for (int y=0; y<levelArray.GetLength(1); y++)
					{
						for (int z=0; z<levelArray.GetLength(2); z++)
						{
							if(grid[x,y,z].cube.Equals(cube))
							{
								if(levelArray[x,y,z])
								{
									breakWrongCube(cube);
								}
								else
								{
									breakCube(cube);
								}
								return;
							}
						}
					}
				}
			}
				
		}
		else
		{
			brushCube(cube);
		}
	}

	void breakCube (GameObject cube)
	{
		GameObject fragment = GameObject.Instantiate(Resources.Load("CubeFragment1")) as GameObject;
		fragment.transform.position = cube.transform.position;
		fragment.renderer.materials = cube.renderer.materials;
		float rot = Random.Range (-50f, 50f);
		fragment.rigidbody.AddTorque (new Vector3(rot,rot,rot));
		fragment.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),Random.Range (0f, 150f),Random.Range (-100f, 100f)));
		fragments.Add (fragment);
		
		fragment = GameObject.Instantiate(Resources.Load("CubeFragment2")) as GameObject;
		fragment.transform.position = cube.transform.position;
		fragment.renderer.materials = cube.renderer.materials;
		rot = Random.Range (-50f, 50f);
		fragment.rigidbody.AddTorque (new Vector3(rot,rot,rot));
		fragment.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),Random.Range (0f, 150f),Random.Range (-100f, 100f)));
		fragments.Add (fragment);
		
		fragment = GameObject.Instantiate(Resources.Load("CubeFragment3")) as GameObject;
		fragment.transform.position = cube.transform.position;
		fragment.renderer.materials = cube.renderer.materials;
		rot = Random.Range (-50f, 50f);
		fragment.rigidbody.AddTorque (new Vector3(rot,rot,rot));
		fragment.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),Random.Range (0f, 150f),Random.Range (-100f, 100f)));
		fragments.Add (fragment);
		
		fragment = GameObject.Instantiate(Resources.Load("CubeFragment4")) as GameObject;
		fragment.transform.position = cube.transform.position;
		fragment.renderer.materials = cube.renderer.materials;
		rot = Random.Range (-50f, 50f);
		fragment.rigidbody.AddTorque (new Vector3(rot,rot,rot));
		fragment.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),Random.Range (0f, 150f),Random.Range (-100f, 100f)));
		fragments.Add (fragment);
		
		fragment = GameObject.Instantiate(Resources.Load("CubeFragment5")) as GameObject;
		fragment.transform.position = cube.transform.position;
		fragment.renderer.materials = cube.renderer.materials;
		rot = Random.Range (-50f, 50f);
		fragment.rigidbody.AddTorque (new Vector3(rot,rot,rot));
		fragment.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),Random.Range (0f, 150f),Random.Range (-100f, 100f)));
		fragments.Add (fragment);
		
		fragment = GameObject.Instantiate(Resources.Load("CubeFragment6")) as GameObject;
		fragment.transform.position = cube.transform.position;
		fragment.renderer.materials = cube.renderer.materials;
		rot = Random.Range (-50f, 50f);
		fragment.rigidbody.AddTorque (new Vector3(rot,rot,rot));
		fragment.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),Random.Range (0f, 150f),Random.Range (-100f, 100f)));
		fragments.Add (fragment);
		
		GameObject.Destroy(cube);
	}

	void breakWrongCube (GameObject cube)
	{
		cube.renderer.material = Resources.Load ("Materials/cubeCracks") as Material;

		score--;

		starsIngame[score].guiTexture.color = new Color (40.0f/255, 40.0f/255, 40.0f/255, 0.5f);

		if(score == 0)
		{
			openMainMenu();
		}


	}

	void brushCube (GameObject cube)
	{
		if(cube.renderer.materials[0].color.r == 1)
		{
			cube.renderer.materials[0].color = new Color(0.5f,1,1,1);
		}
		else
		{
			cube.renderer.materials[0].color = new Color(1,1,1,1);
		}
	}

	bool checkCompletion()
	{
		for (int x=0; x<levelArray.GetLength(0); x++)
		{
			for (int y=0; y<levelArray.GetLength(1); y++)
			{
				for (int z=0; z<levelArray.GetLength(2); z++)
				{
					//if(((grid[x,y,z].cube != null) && (!levelArray[x,y,z])) || ((grid[x,y,z].cube == null) && (levelArray[x,y,z])))
					//if(((!levelArray[x,y,z]) && (grid[x,y,z].cube != null)) || ((levelArray[x,y,z]) && (grid[x,y,z].cube == null)))
					if((!levelArray[x,y,z]) && (grid[x,y,z].cube != null))
					{
						return false;
					}
				}
			}
		}

		return true;
	}

	void openMainMenu()
	{
		foreach (Cube cube in grid) 
		{
			if(cube != null)
				Destroy(cube.cube);
		}
		grid = null;
		pause = true;
		inMenu = true;

		foreach(GameObject button in buttonsIngame)
		{
			button.SetActive(false);
		}

		foreach(GameObject star in starsIngame)
		{
			star.SetActive(false);
		}

		Camera.main.SendMessage("setBackground",Resources.Load("Textures/bg", typeof(Texture2D)) as Texture2D);

		spawnMenuCubes ();

		buttonsMenu [0].SetActive (true);
		buttonsMenu [1].SetActive (true);
		buttonsMenu [2].SetActive (true);
	}

	void spawnMenuCubes()
	{
		levelArray = new bool[2, 2, 2] { { { false, true }, { true, true } }, { { false, true }, { true, true } } };
		
		SpawnCubes ();
		
		foreach (Cube cube in grid) 
		{
			cube.cube.transform.position = new Vector3(cube.cube.transform.position.x, cube.cube.transform.position.y + 1.8f, cube.cube.transform.position.z);
		}
		
		Camera cam = GameObject.FindGameObjectWithTag ("MainCamera").camera;
		
		cam.fieldOfView = 40;
		cam.transform.position = new Vector3 (-5.22f, 5.29f, 6.69f);
		cam.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
	}

	void buttonStart()
	{
		buttonsMenu[1].SetActive (false);
		buttonsMenu[2].SetActive (false);
		buttonsMenu[3].SetActive (true);
		buttonsMenu[4].SetActive (true);
		buttonsMenu[5].SetActive (true);
		buttonsMenu[6].SetActive (true);
	}

	void buttonSettings()
	{

	}

	void buttonPicube()
	{
		foreach(GameObject button in buttonsIngame)
		{
			button.SetActive(false);
		}

		foreach(GameObject button in buttonsMenu)
		{
			button.SetActive(false);
		}
		
		buttonsMenu [0].SetActive (true);
		buttonsMenu [1].SetActive (true);
		buttonsMenu [2].SetActive (true);
	}

	void prepareToStart()
	{
		foreach(GameObject button in buttonsMenu)
		{
			button.SetActive(false);
		}
		
		foreach(GameObject go in buttonsIngame)
		{
			go.SetActive(true);
		}

		score = 5;
		foreach(GameObject go in starsIngame)
		{
			go.SetActive(true);
			go.guiTexture.color = new Color (200/255.0f, 175/255.0f, 50/255.0f, 0.5f);
		}
		
		pause = false;
		inMenu = false;
	}

	void buttonStage1()
	{
		prepareToStart ();

		levelArray = new bool[2, 2, 3] { { { false, true, false }, { true, false, true } }, { { false, true, false }, { true, false, true } } };

		SpawnCubes ();

		Camera.main.SendMessage("setBackground",Resources.Load("Textures/bg2", typeof(Texture2D)) as Texture2D);
	}

	void buttonStage2()
	{
		prepareToStart ();
		
		levelArray = new bool[3, 5, 5] { { { true, false, false, true, true }, { true, true, true, false, false }, { false, false, true, true, true }, { false, false, true, true, false },
				{ false, false, true, false, false }}, { { false, false, false, false, false }, { true, true, true, false, false }, { false, false, true, true, true }, 
				{ false, false, true, true, false },{ false, false, false, false, false }},{ { true, false, false, true, true }, { true, true, true, false, false }, 
				{ false, false, true, true, true }, { false, false, true, true, false },{ false, false, true, false, false }}};

		SpawnCubes ();

	}

	void buttonStage3()
	{
		prepareToStart ();

		levelArray = new bool[2, 2, 2] { { { false, true }, { true, true } }, { { false, true }, { true, true } } };

		SpawnCubes ();
	}

	void buttonStage4()
	{
		
	}

	void buttonHammer()
	{
		breaking = true;
		buttonsIngame [0].guiTexture.color = new Color (0.25f, 0.25f, 0.25f, 0.5f);
		buttonsIngame [1].guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
	}

	void buttonBrush()
	{
		breaking = false;
		buttonsIngame [1].guiTexture.color = new Color (0.25f, 0.25f, 0.25f, 0.5f);
		buttonsIngame [0].guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
	}

}






public class Cube
{
	public GameObject cube;
	public sbyte[] sides = new sbyte[6];

	public Cube()
	{
		cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;

		for (int i=0; i<6; i++) 
		{
			sides[i] = -1;
		}
	}

	public void setSide(byte side, sbyte num, Material mat)
	{
		if(num == sides[side])
		{
			return;
		}

		Vector2 sideOffset = new Vector2 ();
		// 0 - front	1 - right	2 - back	3 - left	4 - top		5 - bot
		if (side == 0)
			sideOffset = new Vector2 (0, 0);
		if (side == 1)
			sideOffset = new Vector2 (0.75f, 0);
		if (side == 2)
			sideOffset = new Vector2 (0.5f, 0);
		if (side == 3)
			sideOffset = new Vector2 (0.25f, 0);
		if (side == 4)
			sideOffset = new Vector2 (0, 0.75f);
		if (side == 5)
			sideOffset = new Vector2 (0, 0.25f);

		Material[] mats = new Material[cube.renderer.materials.Length];

		for (int i = 0; i < 7; i++) 
		{
			mats[i] = cube.renderer.materials[i];
		}

		if (num == -1)
		{
			mats[side + 1] = new Material(Shader.Find ("Diffuse"));
		}
		else
		{
			mats [side + 1] = mat;
		}

		sides[side] = num;
		cube.renderer.materials = mats;
		cube.renderer.materials[side + 1].mainTextureOffset = sideOffset;
	}
}
