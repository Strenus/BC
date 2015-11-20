﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameScript : MonoBehaviour {

	public Cube[, ,] grid;

	public bool[, ,] levelArray;

	public Material[] numMat = new Material[10];

	List<GameObject> fragments = new List<GameObject>();
	List<Cube> creativeCubes = new List<Cube>();

	public List<GameObject> buttonsMenu = new List<GameObject>();
	public List<GameObject> buttonsIngame = new List<GameObject>();
	public List<GameObject> starsIngame = new List<GameObject>();
	public List<GameObject> buttonsPause = new List<GameObject>();
	public List<GameObject> buttonsLevels = new List<GameObject>();
	public List<GameObject> buttonsEdit = new List<GameObject>();

	ushort score;
	bool solved = false;

	GameObject justTouched;
	Cube ghostCube;
	float lastH = 2;
	float editorX = 1;
	float editorY = 1;
	float editorZ = 1;

	ushort ticks = 0;
	bool breaking = true;
	bool pause = true;
	bool inMenu = true;
	bool creative = false;
	public float camSen = 1.0f;
	uint stage;
	uint customStageCount = 0;



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

		savingTest ();
	}

	void Update () 
	{

		//---AndroidBackButton-------------------------------------------------
		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			if(inMenu)
			{
				if(buttonsMenu[1].activeSelf)
				{
					Debug.Log ("exit");
					Application.Quit();
				}

				if(buttonsMenu[3].activeSelf || buttonsMenu[8].activeSelf)
				{
					buttonPicube();
				}

				if(buttonsLevels[0].activeSelf)
				{
					buttonLevelBack();
				}
			}
			else
			{
				if(!creative)
				{
					if(pause)
					{
						if(solved)
						{
							pauseExit();
						}
						else
						{
							pauseContinue();
						}
					}
					else
					{
						buttonPause();
					}
				}
				else
				{
					if(pause)
					{
						pauseContinue();
					}
					else
					{
						buttonPause();
					}
				}
			}
		}

		//---TickCount---------------------------------------------------------

		if(!pause && !creative)
		{
			ticks++;

			if (ticks % 60 == 0)
			{
				if(checkCompletion())
				{
					solved = true;
					openPauseMenu();
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
								if(!creative)
								{
									touchCube (hit.collider.gameObject);
								}
								else
								{
									if(breaking)
									{
										foreach(Cube cube in creativeCubes)
										{
											if(cube.cube.Equals(hit.collider.gameObject))
											{
												Destroy(cube.cube);
												creativeCubes.Remove(cube);
												break;
											}
										}

										float[] x = new float[creativeCubes.Count + 1];
										float[] y = new float[creativeCubes.Count + 1];
										float[] z = new float[creativeCubes.Count + 1];
										
										for(int i=0; i < creativeCubes.Count;i++)
										{
											x[i] = creativeCubes[i].cube.transform.position.x;
											y[i] = creativeCubes[i].cube.transform.position.y;
											z[i] = creativeCubes[i].cube.transform.position.z;
										}
										
										float maxX = Mathf.Max(x) + 0.5f;
										float minX = Mathf.Min(x) - 0.5f;
										float maxY = Mathf.Max(y) + 0.5f;
										float minY = Mathf.Min(y) - 0.5f;
										float maxZ = Mathf.Max(z) + 0.5f;
										float minZ = Mathf.Min(z) - 0.5f;
										
										float h = Mathf.Max(maxX - minX, Mathf.Max(maxY - minY,maxZ - minZ));
										
										if(h <= 9)
										{
											
											//editorCenter = new Vector3((maxX + minX)/2.0f,(maxY + minY)/2.0f,(maxZ + minZ)/2.0f);
											if((maxX - minX) < editorX)
											{
												foreach(Cube cube in creativeCubes)
												{
													if(hit.collider.gameObject.transform.position.x > 0)
													{
														cube.cube.transform.Translate(0.5f,0,0);
													}
													else
													{
														cube.cube.transform.Translate(-0.5f,0,0);
													}
												}
											}
											if((maxY - minY) < editorY)
											{
												foreach(Cube cube in creativeCubes)
												{
													if(hit.collider.gameObject.transform.position.y > 0)
													{
														cube.cube.transform.Translate(0,0.5f,0);
													}
													else
													{
														cube.cube.transform.Translate(0,-0.5f,0);
													}
												}
											}
											if((maxZ - minZ) < editorZ)
											{
												foreach(Cube cube in creativeCubes)
												{
													if(hit.collider.gameObject.transform.position.z > 0)
													{
														cube.cube.transform.Translate(0,0,0.5f);
													}
													else
													{
														cube.cube.transform.Translate(0,0,-0.5f);
													}
												}
											}
											
											/*if(h > 2)
											{
												Camera.main.transform.position = new Vector3(Camera.main.transform.position.x * lastH/h, Camera.main.transform.position.y * lastH/h, Camera.main.transform.position.z * lastH/h);
												lastH = h;
											}*/
											if(h > 2)
											{
												Camera.main.transform.position = new Vector3(Camera.main.transform.position.x * h/lastH, 
													Camera.main.transform.position.y * h/lastH, Camera.main.transform.position.z * h/lastH);
												lastH = h;
											}
											
											editorX = maxX - minX;
											editorY = maxY - minY;
											editorZ = maxZ - minZ;
										}

									}
								}
							}
						}
					}
					justTouched = null;
				}

				if(creative)
				{
					Vector3 direction;
					RaycastHit hit = new RaycastHit();

					if((!breaking) && ((Input.GetTouch(0).phase == TouchPhase.Began) || (Input.GetTouch(0).phase == TouchPhase.Moved)))
					{
						Ray ray = cam.ScreenPointToRay (Input.GetTouch(0).position);
						
						if ((Physics.Raycast (ray, out hit)) && justTouched!= null) 
						{
							if(ghostCube != null)
								Destroy(ghostCube.cube);

							ghostCube = new Cube();
							Vector3 tempVect = hit.collider.transform.position;
							Destroy(ghostCube.cube.collider);
							Material[] mat= new Material[1];
							mat[0] = ghostCube.cube.renderer.materials[0];
							mat[0].color = new Color(mat[0].color.r, mat[0].color.g, mat[0].color.b, 0.2f);
							ghostCube.cube.renderer.materials = mat;

							if(Mathf.Abs(hit.point.x - hit.collider.gameObject.transform.position.x) > 0.499f)
							{
								int temp = Mathf.RoundToInt((hit.point.x - hit.collider.gameObject.transform.position.x) * 2);
								direction = new Vector3(temp,0,0);
								ghostCube.cube.transform.position = tempVect + direction;
							}
							if(Mathf.Abs(hit.point.y - hit.collider.gameObject.transform.position.y) > 0.499f)
							{
								int temp = Mathf.RoundToInt((hit.point.y - hit.collider.gameObject.transform.position.y) * 2);
								direction = new Vector3(0,temp,0);
								ghostCube.cube.transform.position = tempVect + direction;
							}
							if(Mathf.Abs(hit.point.z - hit.collider.gameObject.transform.position.z) > 0.499f)
							{
								int temp = Mathf.RoundToInt((hit.point.z - hit.collider.gameObject.transform.position.z) * 2);
								direction = new Vector3(0,0,temp);
								ghostCube.cube.transform.position = tempVect + direction;
							}
						}
						else
						{
							if(ghostCube != null)
								Destroy(ghostCube.cube);
							ghostCube = null;

						}
					}
					if((Input.GetTouch(0).phase == TouchPhase.Ended) && (ghostCube != null) && (!breaking))
					{
						ghostCube.cube.AddComponent("BoxCollider");
						ghostCube.cube.renderer.material.color = new Color(1,1,1,1);
						creativeCubes.Add(ghostCube);

						float[] x = new float[creativeCubes.Count + 1];
						float[] y = new float[creativeCubes.Count + 1];
						float[] z = new float[creativeCubes.Count + 1];

						for(int i=0; i < creativeCubes.Count;i++)
						{
							x[i] = creativeCubes[i].cube.transform.position.x;
							y[i] = creativeCubes[i].cube.transform.position.y;
							z[i] = creativeCubes[i].cube.transform.position.z;
						}

						float maxX = Mathf.Max(x) + 0.5f;
						float minX = Mathf.Min(x) - 0.5f;
						float maxY = Mathf.Max(y) + 0.5f;
						float minY = Mathf.Min(y) - 0.5f;
						float maxZ = Mathf.Max(z) + 0.5f;
						float minZ = Mathf.Min(z) - 0.5f;

						float h = Mathf.Max(maxX - minX, Mathf.Max(maxY - minY,maxZ - minZ));

						if(h <= 9)
							{

							//Debug.Log("maxX: " + maxX + "  maxY: " + maxY + "  maxZ: " + maxZ);
							//Debug.Log("minX: " + minX + "  minY: " + minY + "  minZ: " + minZ);
							//Debug.Log("ediX: " + editorX + "  ediY: " + editorY + "  ediZ: " + editorZ);

							//editorCenter = new Vector3((maxX + minX)/2.0f,(maxY + minY)/2.0f,(maxZ + minZ)/2.0f);
							if((maxX - minX) > editorX)
							{
								foreach(Cube cube in creativeCubes)
								{
									if(ghostCube.cube.transform.position.x < 0)
									{
										cube.cube.transform.Translate(0.5f,0,0);
									}
									else
									{
										cube.cube.transform.Translate(-0.5f,0,0);
									}
								}
							}
							if((maxY - minY) > editorY)
							{
								foreach(Cube cube in creativeCubes)
								{
									if(ghostCube.cube.transform.position.y < 0)
									{
										cube.cube.transform.Translate(0,0.5f,0);
									}
									else
									{
										cube.cube.transform.Translate(0,-0.5f,0);
									}
								}
							}
							if((maxZ - minZ) > editorZ)
							{
								foreach(Cube cube in creativeCubes)
								{
									if(ghostCube.cube.transform.position.z < 0)
									{
										cube.cube.transform.Translate(0,0,0.5f);
									}
									else
									{
										cube.cube.transform.Translate(0,0,-0.5f);
									}
								}
							}

							if(h > 2)
							{
								Camera.main.transform.position = new Vector3(Camera.main.transform.position.x * h/lastH, 
									Camera.main.transform.position.y * h/lastH, Camera.main.transform.position.z * h/lastH);
								lastH = h;
							}

							editorX = maxX - minX;
							editorY = maxY - minY;
							editorZ = maxZ - minZ;
						}
						else
						{
							creativeCubes.Remove(ghostCube);
							Destroy(ghostCube.cube);
						}

						ghostCube = null;

					}
				}


				cam.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
				if(justTouched == null)
				{
					if(!(((cam.transform.position.y > 4.6f * lastH) && (Input.GetTouch(0).deltaPosition.y < 0.0f))
					     || ((cam.transform.position.y < -4.6f * lastH) && (Input.GetTouch(0).deltaPosition.y > 0.0f))))
					{
						cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, -Input.GetTouch(0).deltaPosition.y * 800.0f * camSen / Screen.height);
						//cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, -Input.GetTouch(0).deltaPosition.y * 300.0f / Screen.height);
					}
					cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.up, Input.GetTouch(0).deltaPosition.x * 600.0f * camSen / Screen.height);
					//cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.up, Input.GetTouch(0).deltaPosition.x * 230.0f / Screen.height);
				}
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
		//---Nastavenie kamery do zakladnej pozicie------------------------------

		int tempMax = Mathf.Max (levelArray.GetLength (0), Mathf.Max (levelArray.GetLength (1), levelArray.GetLength (2)));

		GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera");
		camera.transform.position = new Vector3 (0, 0, tempMax * 5);
		camera.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
		camera.camera.fieldOfView = 20.0f;

		//---Vymazanie predoslych kociek--------------------------------------------------
		if(grid != null)
		{
			foreach (Cube cube in grid) 
			{
				if(cube != null)
					Destroy(cube.cube);
			}
			grid = null;
		}
		//---Vytvorenie kociek--------------------------------------------------
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

	void spawnEditCubes()
	{
		//---Nastavenie kamery do zakladnej pozicie------------------------------
		
		GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera");
		camera.transform.position = new Vector3 (0, 0, 10);
		camera.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
		camera.camera.fieldOfView = 20.0f;

		if(grid != null)
		{
			foreach (Cube cube in grid) 
			{
				if(cube != null)
					Destroy(cube.cube);
			}
			grid = null;
		}

		/*grid = new Cube[9, 9, 9];
		grid [4, 4, 4] = new Cube ();

		Material[] mat= new Material[1];
		mat[0] = grid [4, 4, 4].cube.renderer.materials[0];
		grid [4, 4, 4].cube.renderer.materials = mat;*/

		creativeCubes.Clear ();
		creativeCubes.Add (new Cube ());

		Material[] mat= new Material[1];
		mat[0] = creativeCubes[0].cube.renderer.materials[0];
		creativeCubes[0].cube.renderer.materials = mat;

	}

	void touchCube (GameObject cube)
	{
		if(creative)
		{



			return;
		}

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
		if(grid != null)
		{
			foreach (Cube cube in grid) 
			{
				if(cube != null)
					Destroy(cube.cube);
			}
		}

		foreach (Cube cube in creativeCubes) 
		{
			if(cube != null)
				Destroy(cube.cube);
		}

		grid = null;
		creativeCubes.Clear ();
		creative = false;
		pause = true;
		inMenu = true;
		solved = false;

		foreach(GameObject button in buttonsIngame)
		{
			button.SetActive(false);
		}

		foreach(GameObject button in buttonsPause)
		{
			button.SetActive(false);
		}

		foreach(GameObject go in buttonsEdit)
		{
			go.SetActive(false);
		}

		for(int i=0;i < starsIngame.Count;i++)
		{
			starsIngame[i].transform.position = new Vector3(0.68f + i * 0.07f,0.94f,0);
			starsIngame[i].guiTexture.pixelInset = new Rect (-Screen.height / 20, -Screen.height / 20, Screen.height / 10, Screen.height / 10);
			starsIngame[i].SetActive(false);
		}

		Camera.main.SendMessage("setBackground",Resources.Load("Textures/bg", typeof(Texture2D)) as Texture2D);

		spawnMenuCubes ();

		buttonsMenu [0].SetActive (true);
		buttonsMenu [1].SetActive (true);
		buttonsMenu [2].SetActive (true);
	}

	void openPauseMenu()
	{
		foreach(GameObject go in buttonsPause)
		{
			go.SetActive(true);
		}

		if(solved)
		{
			for(int i=0;i < starsIngame.Count;i++)
			{
				starsIngame[i].transform.position = new Vector3(0.3f + i * 0.1f,0.6f,0.2f);
				starsIngame[i].guiTexture.pixelInset = new Rect (-Screen.height * 3 / 40, -Screen.height * 3 / 40, Screen.height * 3 / 20, Screen.height * 3 / 20);
			}
		}
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
		buttonsMenu[7].SetActive (true);
	}

	void buttonSettings()
	{
		buttonsMenu[1].SetActive (false);
		buttonsMenu[2].SetActive (false);
		buttonsMenu[8].SetActive (true);
		buttonsMenu[9].SetActive (true);
		buttonsMenu[10].SetActive (true);
		buttonsMenu[11].SetActive (true);
		buttonsMenu[12].SetActive (true);
	}

	void buttonPicube()
	{
		foreach(GameObject button in buttonsIngame)
		{
			button.SetActive(false);
		}

		foreach(GameObject go in buttonsLevels)
		{
			go.SetActive(false);
		}

		foreach(GameObject button in buttonsMenu)
		{
			button.SetActive(false);
		}
		
		buttonsMenu [0].SetActive (true);
		buttonsMenu [1].SetActive (true);
		buttonsMenu [2].SetActive (true);
	}

	void buttonReset()
	{

	}

	void buttonBack()
	{
		buttonPicube ();
	}

	void buttonCamPlus()
	{
		if(camSen < 2.95f)
			camSen += 0.1f;

		int sens = 10 * Mathf.RoundToInt (camSen * 10.0f);

		buttonsMenu [8].GetComponentInChildren<GUIText> ().text = "Camera Sensitivity: " + sens + "%";
	}

	void buttonCamMinus()
	{
		if(camSen > 0.15f)
			camSen -= 0.1f;

		int sens = 10 * Mathf.RoundToInt (camSen * 10.0f);
		
		buttonsMenu [8].GetComponentInChildren<GUIText> ().text = "Camera Sensitivity: " + sens + "%";
	}

	void prepareToStart()
	{
		foreach(GameObject button in buttonsMenu)
		{
			button.SetActive(false);
		}

		foreach(GameObject go in buttonsLevels)
		{
			go.SetActive(false);
		}
		
		foreach(GameObject go in buttonsIngame)
		{
			go.SetActive(true);
		}

		score = 5;
		solved = false;

		foreach(GameObject go in starsIngame)
		{
			go.SetActive(true);
			go.guiTexture.color = new Color (200/255.0f, 175/255.0f, 50/255.0f, 0.5f);
		}

		breaking = true;
		buttonsIngame [0].guiTexture.color = new Color (0.25f, 0.25f, 0.25f, 0.5f);
		buttonsIngame [1].guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);

		pause = false;
		inMenu = false;
	}

	void buttonStage(uint stage)
	{
		this.stage = stage;
		
		for(int i=1; i < buttonsMenu.Count; i++)
		{
			buttonsMenu[i].SetActive(false);
		}
		buttonsMenu [0].SetActive (true);
		
		foreach(GameObject go in buttonsLevels)
		{
			go.SetActive(true);
		}
		buttonsLevels [17].SetActive (false);
		buttonsLevels [18].SetActive (false);
		buttonsLevels [19].SetActive (false);

		
		buttonsLevels[0].guiText.text = "Stage " + stage;
	}

	void buttonStage1()
	{
		buttonStage (1);
	}

	void buttonStage2()
	{
		buttonStage (2);
	}

	void buttonStage3()
	{
		buttonStage (3);
	}

	void buttonStage4()
	{
		stage = 4;

		for(int i=1; i < buttonsMenu.Count; i++)
		{
			buttonsMenu[i].SetActive(false);
		}
		buttonsMenu [0].SetActive (true);

		loadLevelCount ();

		for(int i = 0; i <= customStageCount; i++)
		{
			buttonsLevels[i].SetActive(true);
		}

		buttonsLevels [16].SetActive (true);
		buttonsLevels [17].SetActive (true);
		buttonsLevels [18].SetActive(true);

		if(customStageCount == 0)
		{
			buttonsLevels [18].SetActive(false);
			buttonsLevels [19].SetActive (true);
		}

		buttonsLevels[0].guiText.text = "Custom Stages";
	}

	void buttonStageBack()
	{
		buttonPicube ();
	}

	void buttonHammer()
	{
		if(pause)
			return;

		breaking = true;
		buttonsIngame [0].guiTexture.color = new Color (0.25f, 0.25f, 0.25f, 0.5f);
		buttonsIngame [1].guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
	}

	void buttonBrush()
	{
		if(pause)
			return;

		breaking = false;
		buttonsIngame [1].guiTexture.color = new Color (0.25f, 0.25f, 0.25f, 0.5f);
		buttonsIngame [0].guiTexture.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
	}

	void buttonPause()
	{
		pause = true;
		openPauseMenu ();
	}

	void pauseContinue()
	{
		if(solved)
		{
			openMainMenu();
		}
		else
		{
			pause = false;

			foreach(GameObject go in buttonsPause)
			{
				go.SetActive(false);
			}

			/*
			for(int i=0;i < starsIngame.Count;i++)
			{
				starsIngame[i].transform.position = new Vector3(0.68f + i * 0.07f,0.94f,0);
				starsIngame[i].guiTexture.pixelInset = new Rect (-Screen.height / 20, -Screen.height / 20, Screen.height / 10, Screen.height / 10);
			}
			*/
		}

	}

	void pauseExit()
	{
		openMainMenu ();
	}

	void buttonLevelBack()
	{
		if(stage == 5)
		{
			buttonStage4();
			return;
		}

		buttonsMenu[1].SetActive (false);
		buttonsMenu[2].SetActive (false);
		buttonsMenu[3].SetActive (true);
		buttonsMenu[4].SetActive (true);
		buttonsMenu[5].SetActive (true);
		buttonsMenu[6].SetActive (true);
		buttonsMenu[7].SetActive (true);

		foreach(GameObject go in buttonsLevels)
		{
			go.SetActive(false);
		}
	}

	void buttonLevelCreate()
	{
		prepareToCreateLevel ();

		spawnEditCubes ();
	}

	void buttonLevelDelete()
	{
		stage = 5;
		
		if(customStageCount == 0)
		{
			buttonsLevels [19].SetActive (false);
		}
		
		for(int i = 0; i <= customStageCount; i++)
		{
			buttonsLevels[i].SetActive(true);
		}
		
		buttonsLevels [16].SetActive (true);
		buttonsLevels [17].SetActive (false);
		buttonsLevels [18].SetActive (false);
		buttonsLevels[0].guiText.text = "Delete Custom Stage";
	}

	void buttonLevel(uint level)
	{
		if(stage == 1)
		{
			prepareToStart ();

			if(level == 1)
			{
				levelArray = new bool[2, 2, 2] { { { false, true }, { true, true } }, { { false, true }, { true, true } } };
			}
			if(level == 2)
			{
				Camera.main.SendMessage("setBackground",Resources.Load("Textures/bg2", typeof(Texture2D)) as Texture2D);	
				levelArray = new bool[2, 2, 3] { { { false, true, false }, { true, false, true } }, { { false, true, false }, { true, false, true } } };
			}
			if(level == 3)
			{
				levelArray = new bool[3, 5, 5] { { { true, false, false, true, true }, { true, true, true, false, false }, { false, false, true, true, true }, { false, false, true, true, false },
						{ false, false, true, false, false }}, { { false, false, false, false, false }, { true, true, true, false, false }, { false, false, true, true, true }, 
						{ false, false, true, true, false },{ false, false, false, false, false }},{ { true, false, false, true, true }, { true, true, true, false, false }, 
						{ false, false, true, true, true }, { false, false, true, true, false },{ false, false, true, false, false }}};
			}
			if(level == 4)
			{
				//levelArray = new bool[3,5,5] { { { true, false, true, true, true}, { true, true, true, false, false}, { false, false, true, true, true}, { false, false, true, true, false}, { false, false, true, false, false}}, { { false, false, false, false, false}, { true, true, true, false, false}, { true, false, true, true, true}, { false, false, true, true, false}, { false, false, false, false, false}}, { { true, false, true, true, true}, { true, true, true, false, false}, { false, false, true, true, true}, { false, false, true, true, false}, { false, false, true, false, false}}};
				//levelArray = new bool[5,5,3] { { { true, false, true}, { false, false, false}, { true, true, true}, { false, false, false}, { false, false, false}}, { { true, false, true}, { false, false, false}, { true, true, true}, { true, true, true}, { false, false, false}}, { { true, false, true}, { true, true, true}, { true, true, true}, { true, true, true}, { true, false, true}}, { { false, false, false}, { true, true, true}, { false, false, false}, { false, false, false}, { false, false, false}}, { { true, false, true}, { true, true, true}, { false, true, false}, { false, false, false}, { false, false, false}}};
				levelArray = new bool[3,5,5] { { { true, false, true, true, true}, { true, true, true, false, false}, { false, false, true, true, true}, { false, false, true, true, false}, { false, false, true, false, false}}, { { false, false, false, false, false}, { true, true, true, false, false}, { true, false, true, true, true}, { false, false, true, true, false}, { false, false, false, false, false}}, { { true, false, true, true, true}, { true, true, true, false, false}, { false, false, true, true, true}, { false, false, true, true, false}, { false, false, true, false, false}}};
			}
			if(level == 5)
			{
				
			}
			if(level == 6)
			{
				
			}
			if(level == 7)
			{
				
			}
			if(level == 8)
			{
				
			}
			if(level == 9)
			{
				
			}
			if(level == 10)
			{
				
			}
			if(level == 11)
			{
				
			}
			if(level == 12)
			{
				
			}
			if(level == 13)
			{
				
			}
			if(level == 14)
			{
				
			}
			if(level == 15)
			{
				
			}
		}
		if(stage == 2)
		{
			prepareToStart ();

			if(level == 1)
			{

			}
			if(level == 2)
			{

			}
			if(level == 3)
			{

			}
			if(level == 4)
			{
				
			}
			if(level == 5)
			{
				
			}
			if(level == 6)
			{
				
			}
			if(level == 7)
			{
				
			}
			if(level == 8)
			{
				
			}
			if(level == 9)
			{
				
			}
			if(level == 10)
			{
				
			}
			if(level == 11)
			{
				
			}
			if(level == 12)
			{
				
			}
			if(level == 13)
			{
				
			}
			if(level == 14)
			{
				
			}
			if(level == 15)
			{
				
			}
		}
		if(stage == 3)
		{
			prepareToStart ();

			if(level == 1)
			{

			}
			if(level == 2)
			{

			}
			if(level == 3)
			{

			}
			if(level == 4)
			{
				
			}
			if(level == 5)
			{
				
			}
			if(level == 6)
			{
				
			}
			if(level == 7)
			{
				
			}
			if(level == 8)
			{
				
			}
			if(level == 9)
			{
				
			}
			if(level == 10)
			{
				
			}
			if(level == 11)
			{
				
			}
			if(level == 12)
			{
				
			}
			if(level == 13)
			{
				
			}
			if(level == 14)
			{
				
			}
			if(level == 15)
			{
				
			}
		}
		if(stage == 4)
		{
			prepareToStart ();

			try
			{
				System.IO.StreamReader file = new System.IO.StreamReader(Application.persistentDataPath + "/custom" + level + ".txt");

				uint d1 = uint.Parse(file.ReadLine());
				uint d2 = uint.Parse(file.ReadLine());
				uint d3 = uint.Parse(file.ReadLine());

				levelArray = new bool[d1,d2,d3];

				for (int x=0; x < d1; x++)
				{
					for (int y=0; y < d2; y++)
					{
						for (int z=0; z < d3; z++)
						{
							uint temp = uint.Parse(file.ReadLine());

							if(temp == 1)
							{
								levelArray[x,y,z] = true;
							}
							else
							{
								levelArray[x,y,z] = false;
							}

						}
					}
				}

				file.Close();
			}
			catch (System.Exception e)
			{
				Debug.Log(e);
			}
		}
		if(stage == 5)
		{
			File.Delete(Application.persistentDataPath + "/custom" + level + ".txt");

			for(uint i = level + 1; i < customStageCount + 1; i++)
			{
				File.Move(Application.persistentDataPath + "/custom" + i + ".txt",Application.persistentDataPath + "/custom" + (i - 1) + ".txt");
			}

			stage = 4;

			customStageCount--;

			buttonLevelBack();
			buttonStage4();

			return;
		}
		
		SpawnCubes ();
	}

	void buttonEditPause()
	{
		pause = true;

		openPauseMenu ();
	}

	void buttonEditPlus()
	{
		if(pause)
			return;

		breaking = false;
	}

	void buttonEditMinus()
	{
		if(pause)
			return;
		
		breaking = true;
	}

	void buttonEditOK()
	{
		convertEditToLevel ();
		customStageCount++;
		saveLevel (customStageCount);
		pauseExit ();
	}

	void prepareToCreateLevel()
	{
		foreach(GameObject button in buttonsMenu)
		{
			button.SetActive(false);
		}
		
		foreach(GameObject go in buttonsLevels)
		{
			go.SetActive(false);
		}
		
		foreach(GameObject go in buttonsEdit)
		{
			go.SetActive(true);
		}
		
		breaking = false;
		pause = false;
		inMenu = false;
		
		creative = true;
		lastH = 2;
		editorX = 1;
		editorY = 1;
		editorZ = 1;
	}

	void convertEditToLevel()
	{
		levelArray = new bool[(int)editorX, (int)editorY, (int)editorZ];

		uint lX = 0;
		uint lY = 0;
		uint lZ = 0;

		for (float x = -editorX / 2.0f + 0.5f; x < editorX / 2.0f; x++,lX++) 
		{
			lY = 0;
			for (float y = -editorY / 2.0f + 0.5f; y < editorY / 2.0f; y++, lY++) 
			{
				lZ = 0;
				for (float z = -editorZ / 2.0f + 0.5f; z < editorZ / 2.0f; z++, lZ++) 
				{
					bool found = false;
					for(int i = 0; i < creativeCubes.Count; i++)
					{
						Vector3 vec = creativeCubes[i].cube.transform.position;
						if((vec.x == x) && (vec.y == y) && (vec.z ==z))
						{
							levelArray[lX,lY,lZ] = true;
							Destroy(creativeCubes[i].cube);
							creativeCubes.Remove(creativeCubes[i]);
							found = true;
							break;

						}
					}
					if(!found)
					{
						levelArray[lX,lY,lZ] = false;
					}
				}
			}
		}
	}

	void savingTest()
	{
		string fileName = Application.persistentDataPath + "/test.txt";
		
		try
		{
			
			if (!File.Exists(fileName))
			{
				Debug.Log("Opened file!");

				File.WriteAllText(fileName,"5 0 2 5 0");
			}
			
			else
			{
				Debug.Log("File is exist! Loading!");
				loadFile();
			}
		}
		
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}

	void loadFile()
	{
		Debug.Log("Reading");
		
		string fileName = Application.persistentDataPath + "/test.txt";
		
		string testText = File.ReadAllText(fileName);
		
		
		Debug.Log (testText);
	}
	
	uint loadLevelCount()
	{
		uint levelCount = 0;

		for(int i = 1; i < 15; i++)
		{
			string filePath = Application.persistentDataPath + "/custom" + i + ".txt";

			if(File.Exists(filePath))
			{
				levelCount++;
			}
			else
			{
				break;
			}
		}
		customStageCount = levelCount;

		return levelCount;
	}

	void testSaveLevel()
	{
		levelArray = new bool[3, 5, 5] { { { true, false, false, true, true }, { true, true, true, false, false }, { false, false, true, true, true }, { false, false, true, true, false },
				{ false, false, true, false, false }}, { { false, false, false, false, false }, { true, true, true, false, false }, { false, false, true, true, true }, 
				{ false, false, true, true, false },{ false, false, false, false, false }},{ { true, false, false, true, true }, { true, true, true, false, false }, 
				{ false, false, true, true, true }, { false, false, true, true, false },{ false, false, true, false, false }}};
	
		saveLevel (1);
	}

	void saveLevel(uint level)
	{
		try
		{
			System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath + "/custom" + level + ".txt");
			
			file.WriteLine (levelArray.GetLength (0).ToString ());
			file.WriteLine (levelArray.GetLength (1).ToString ());
			file.WriteLine (levelArray.GetLength (2).ToString ());
			
			for (int x=0; x<levelArray.GetLength(0); x++)
			{
				for (int y=0; y<levelArray.GetLength(1); y++)
				{
					for (int z=0; z<levelArray.GetLength(2); z++)
					{
						if(levelArray[x,y,z])
						{
							file.WriteLine("1");
						}
						else
						{
							file.WriteLine("0");
						}
					}
				}
			}
			
			file.Close ();

			//TEST
			file = new System.IO.StreamWriter(Application.persistentDataPath + "/test" + level + ".txt");

			file.Write("levelArray = new bool[");
			file.Write(levelArray.GetLength (0).ToString ());
			file.Write(",");
			file.Write(levelArray.GetLength (1).ToString ());
			file.Write(",");
			file.Write(levelArray.GetLength (2).ToString ());
			file.Write("] ");

			file.Write("{ ");
			for (int x=0; x<levelArray.GetLength(0); x++)
			{
				file.Write("{ ");
				for (int y=0; y<levelArray.GetLength(1); y++)
				{
					file.Write("{ ");
					for (int z=0; z<levelArray.GetLength(2); z++)
					{
						if(levelArray[x,y,z])
						{
							file.Write("true");
						}
						else
						{
							file.Write("false");
						}
						if(z + 1 != levelArray.GetLength(2))
							file.Write(", ");
					}
					file.Write("}");
					if(y + 1 != levelArray.GetLength(1))
						file.Write(", ");

				}
				file.Write("}");
				if(x + 1 != levelArray.GetLength(0))
					file.Write(", ");
			}
			file.Write("};");
			
			file.Close ();

			/*
			{ { { true, false, false, true, true }, { true, true, true, false, false }, { false, false, true, true, true }, { false, false, true, true, false },
			{ false, false, true, false, false }}, { { false, false, false, false, false }, { true, true, true, false, false }, { false, false, true, true, true }, 
			{ false, false, true, true, false },{ false, false, false, false, false }},{ { true, false, false, true, true }, { true, true, true, false, false }, 
			{ false, false, true, true, true }, { false, false, true, true, false },{ false, false, true, false, false }}}
			*/
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}

	void loadLevel()
	{

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
