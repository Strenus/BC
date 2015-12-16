
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameScript : MonoBehaviour {

	public Cube[, ,] grid;
	public bool[, ,] levelArray;

	public Material[] numMat = new Material[10];

	public GameObject cubesParent;

	List<GameObject> fragments = new List<GameObject>();
	List<Cube> creativeCubes = new List<Cube>();

	public GameObject[] ClipArrow = new GameObject[3];
	public short movingArrow = -1;
	public float arrowOffset;

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
	float touchDistance = 0;
	bool creativeCameraAdjust = true;
	float editorX = 1;
	float editorY = 1;
	float editorZ = 1;

	ushort ticks = 0;
	ushort finishTimer = 360;
	bool breaking = true;
	bool pause = true;
	bool inMenu = true;
	bool creative = false;
	public float camSen = 1.0f;
	uint stage = 1;
	uint level;
	uint customStageCount = 0;

	Texture2D colorPaletteTexture;
	public GameObject colorPalette;
	Color creativeColor = new Color (0.8f, 0.8f, 0.8f, 1);
	bool creativeColoring = false;


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


		//--Configure Color Palette----------------------------------------------------------------
		byte[] fileData;		
		fileData = File.ReadAllBytes(Application.dataPath + "/Resources/Textures/colors.png");
		colorPaletteTexture = new Texture2D(Screen.width * 2 / 3, Screen.height * 2 / 3);
		colorPaletteTexture.LoadImage(fileData);		
		colorPalette.guiTexture.texture = colorPaletteTexture;
		colorPalette.guiTexture.pixelInset = new Rect(- Screen.width / 3, - Screen.height / 3, Screen.width * 2 / 3, Screen.height * 2 / 3);



		savingTest ();
	}

	void colorPaletteUpdate()
	{


		StartCoroutine(readingPalette());


	}

	IEnumerator readingPalette()
	{
		yield return new WaitForEndOfFrame();

		if((Input.GetTouch(0).position.x > Screen.width / 6) && (Input.GetTouch(0).position.x < Screen.width * 5 / 6)
		   && (Input.GetTouch(0).position.y > Screen.height / 6) && (Input.GetTouch(0).position.y < Screen.height * 5 / 6))
		{
			try
			{
				colorPaletteTexture.ReadPixels( new Rect(0, 0,Screen.width, Screen.height), 0, 0);
			}
			catch
			{

			}		
			
			creativeColor = colorPaletteTexture.GetPixel((int) (Input.GetTouch(0).position.x), (int) (Input.GetTouch(0).position.y));

			buttonsEdit[2].guiTexture.color = creativeColor;
		}
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
					//pause = true;
					//solved = true;
					//openPauseMenu();



					if(solved == false)
					{
						Debug.Log("yaaaaaas");

						completedLevel();
						solved = true;
						finishTimer = 360;
					}
				}
			}

			if(solved)
			{
				foreach(Cube cube in grid)
				{
					if(cube.cube == null)
						continue;
					
					for(int i= 1; i< cube.cube.renderer.materials.Length; i++)
					{
						Color tempColor = cube.cube.renderer.materials[i].color;
						tempColor.a -= 0.01f;
						cube.cube.renderer.materials[i].color = tempColor;

						if(cube.cube.renderer.materials[i].color.a < 0.01f)
						{
							Material tempMat = cube.cube.renderer.materials[0];
							cube.cube.renderer.materials = new Material[1];
							cube.cube.renderer.material = tempMat;
						}
					}

					int tempMin = Mathf.Min (levelArray.GetLength (0), Mathf.Max (levelArray.GetLength (1), levelArray.GetLength (2)));

					cubesParent.transform.Rotate(Vector3.up, 0.08f / tempMin);
				}

				finishTimer--;

				if(finishTimer == 0)
				{
					pause = true;
					openPauseMenu();
				}
			}
		}

		//---TouchControl------------------------------------------------------
		Camera cam = GameObject.FindGameObjectWithTag ("MainCamera").camera;

		if(!pause)
		{
			if (Input.touchCount == 1)
			{
				if(Input.GetTouch(0).phase == TouchPhase.Began)
				{
					Ray ray = cam.ScreenPointToRay (Input.GetTouch(0).position);
					RaycastHit hit = new RaycastHit();
					
					if (Physics.Raycast (ray, out hit)) 
					{
						justTouched = hit.collider.gameObject;

						if((justTouched.name == "Arrow X") || (justTouched.name == "Arrow Y") || (justTouched.name == "Arrow Z"))
						{
							touchArrow(justTouched.name);
						}
					}
				}

				if((movingArrow >=0) && (Input.GetTouch(0).phase == TouchPhase.Moved))
				{
					moveArrow();
				}
				
				if(Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					movingArrow = -1;

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
									if(colorPalette.activeSelf)
									{
										justTouched = null;
										return;
									}

									if(creativeColoring)
									{
										hit.collider.renderer.materials[0].color = creativeColor;
										justTouched = null;
										return;
									}

									if((breaking) && (creativeCubes.Count > 1))
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

										float[] x = new float[creativeCubes.Count];
										float[] y = new float[creativeCubes.Count];
										float[] z = new float[creativeCubes.Count];
										
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

											//centering objects

											if((maxX - minX) < editorX)
											{
												foreach(Cube cube in creativeCubes)
												{
													if(hit.collider.gameObject.transform.position.x > 0)
													{
														cube.cube.transform.Translate((editorX - maxX + minX) * 0.5f,0,0);
													}
													else
													{
														cube.cube.transform.Translate(-(editorX - maxX + minX) * 0.5f,0,0);
													}
												}
											}
											if((maxY - minY) < editorY)
											{
												foreach(Cube cube in creativeCubes)
												{
													if(hit.collider.gameObject.transform.position.y > 0)
													{
														cube.cube.transform.Translate(0,(editorY - maxY + minY) * 0.5f,0);
													}
													else
													{
														cube.cube.transform.Translate(0,-(editorY - maxY + minY) * 0.5f,0);
													}
												}
											}
											if((maxZ - minZ) < editorZ)
											{
												foreach(Cube cube in creativeCubes)
												{
													if(hit.collider.gameObject.transform.position.z > 0)
													{
														cube.cube.transform.Translate(0,0,(editorZ - maxZ + minZ) * 0.5f);
													}
													else
													{
														cube.cube.transform.Translate(0,0,-(editorZ - maxZ + minZ) * 0.5f);
													}
												}
											}


											//Camera adjustment
											if((creativeCameraAdjust) && (h > 2))
											{
												Camera.main.fieldOfView = 4 * h;
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
					if(colorPalette.activeSelf == true)
					{
						colorPaletteUpdate();

						if((Input.GetTouch(0).position.x < Screen.width / 6) || (Input.GetTouch(0).position.x > Screen.width * 5 / 6)
						   || (Input.GetTouch(0).position.y < Screen.height / 6) || (Input.GetTouch(0).position.y > Screen.height * 5 / 6))
						{
							if(Input.GetTouch(0).phase == TouchPhase.Began)
							{
								colorPalette.SetActive(false);
							}
						}

						justTouched = null;
						return;
					}

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
						ghostCube.cube.renderer.material.color = creativeColor;
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

							//centering object
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

							//Camera Adjustment
							if((creativeCameraAdjust) && (h > 2))
							{
								Camera.main.fieldOfView = 4 * h;
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
				if((Input.GetTouch (0).phase == TouchPhase.Moved) && (justTouched == null))
				{
					float angle;
					if(!(((cam.transform.position.y > 19.8f ) && (Input.GetTouch(0).deltaPosition.y < 0.0f))
					     || ((cam.transform.position.y < -19.8f ) && (Input.GetTouch(0).deltaPosition.y > 0.0f))))
					{
						angle = -Input.GetTouch(0).deltaPosition.y * 800.0f * camSen / Screen.height;
						cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, angle);
						//cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, -Input.GetTouch(0).deltaPosition.y * 300.0f / Screen.height);
					}
					angle = Input.GetTouch(0).deltaPosition.x * 600.0f * camSen / Screen.height;
					float modifier = -0.04f* Mathf.Abs(cam.transform.position.y) + 1;
					cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.up, angle * modifier);
					//cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.up, Input.GetTouch(0).deltaPosition.x * 230.0f / Screen.height);
				}

				touchDistance = 0;
			}

			if (Input.touchCount == 2)
			{
				if((Input.GetTouch(0).phase == TouchPhase.Moved) && (Input.GetTouch(1).phase == TouchPhase.Moved))
				{
					Vector2 touch0, touch1;
					touch0 = Input.GetTouch(0).position;
					touch1 = Input.GetTouch(1).position;
					
					if(touchDistance == 0)
						touchDistance = Vector2.Distance(touch0, touch1);
					
					if(touchDistance - Vector2.Distance(touch0, touch1) > 2.0f)
					{
						Camera.main.fieldOfView++;
					}
					if(touchDistance - Vector2.Distance(touch0, touch1) < -2.0f)
					{
						Camera.main.fieldOfView--;
					}
					
					
					touchDistance = Vector2.Distance(touch0, touch1);
					creativeCameraAdjust = false;
				}
				
				if((Input.GetTouch(0).phase == TouchPhase.Ended) || (Input.GetTouch(1).phase == TouchPhase.Ended)
				   || (Input.GetTouch(0).phase == TouchPhase.Canceled) || (Input.GetTouch(1).phase == TouchPhase.Canceled))
				{
					touchDistance = 0;
				}
			}

			//Debug.Log(touchDistance);
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
		//---Placing Camera To Default Position------------------------------

		int tempMax = Mathf.Max (levelArray.GetLength (0), Mathf.Max (levelArray.GetLength (1), levelArray.GetLength (2)));

		GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera");
		camera.transform.position = new Vector3 (0, 0, 20);
		camera.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
		camera.camera.fieldOfView = 4 * tempMax;

		//---Deleting previous cubes--------------------------------------------------
		if(grid != null)
		{
			foreach (Cube cube in grid) 
			{
				if(cube != null)
					Destroy(cube.cube);
			}
			grid = null;
		}
		//---Spawning Cubes--------------------------------------------------
		grid = new Cube[levelArray.GetLength (0), levelArray.GetLength (1), levelArray.GetLength (2)];

		for (int x=0; x<levelArray.GetLength(0); x++)
		{
			for (int y=0; y<levelArray.GetLength(1); y++)
			{
				for (int z=0; z<levelArray.GetLength(2); z++)
				{
					
					grid[x,y,z] = new Cube();
					grid[x,y,z].cube.transform.position = new Vector3(x - levelArray.GetLength(0)/2.0f + 0.5f,y - levelArray.GetLength(1)/2.0f + 0.5f,z - levelArray.GetLength(2)/2.0f + 0.5f);

					grid[x,y,z].cube.gameObject.name = "Cube " + x + " " + y + " " + z;

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

					grid[x,y,z].cube.transform.parent = cubesParent.transform;
					
					
				}
			}
		}

		//---Setting color to cubes-----------------------------------------------
		if(level != 0)
		{
			try
			{
				//System.IO.StreamReader file = new System.IO.StreamReader(Application.persistentDataPath + "/custom" + level + ".txt");
				
				uint tempLevel = (stage - 1) * 15 + level;

				System.IO.StreamReader file;
				
				if(stage <= 3)
				{
					file = new System.IO.StreamReader(Application.dataPath + "/Resources/Levels/color" + tempLevel + ".txt");
				}
				else
				{
					file = new System.IO.StreamReader(Application.persistentDataPath + "/custom" + level + "color.txt");
				}
				
				for (int x=0; x<levelArray.GetLength(0); x++)
				{
					for (int y=0; y<levelArray.GetLength(1); y++)
					{
						for (int z=0; z<levelArray.GetLength(2); z++)
						{
							if(levelArray[x,y,z] == false)
								continue;

							grid[x,y,z].color = new Color(float.Parse(file.ReadLine()), float.Parse(file.ReadLine()), float.Parse(file.ReadLine()), 1);
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

		//---Clipping Planes------------------------------------------------------

		Vector3 temp;

		temp = grid [0, grid.GetLength (1) - 1, 0].cube.transform.position;
		ClipArrow[0] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
		ClipArrow[0].name = "Arrow X";
		//mats[side + 1] = new Material(Shader.Find ("Diffuse"));
		ClipArrow[0].renderer.material.color = new Color (0.75f, 0.25f, 0.25f, 1.0f);
		ClipArrow[0].transform.position = new Vector3 (temp.x - 0.6f, temp.y + 0.5f, temp.z - 0.5f);
		ClipArrow[0].transform.Rotate (0.0f, 0.0f, 270.0f);

		temp = grid [grid.GetLength (0) - 1, grid.GetLength (1) - 1, 0].cube.transform.position;
		ClipArrow[1] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
		ClipArrow[1].name = "Arrow Y";
		ClipArrow[1].renderer.material.color = new Color (0.25f, 0.75f, 0.25f, 1.0f);
		ClipArrow[1].transform.position = new Vector3 (temp.x + 0.5f, temp.y + 0.6f, temp.z - 0.5f);
		ClipArrow[1].transform.Rotate (180.0f, 0.0f, 0.0f);

		temp = grid [grid.GetLength (0) - 1, grid.GetLength (1) - 1, grid.GetLength (2) - 1].cube.transform.position;

		ClipArrow[2] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
		ClipArrow[2].name = "Arrow Z";
		ClipArrow[2].renderer.material.color = new Color (0.25f, 0.25f, 0.75f, 1.0f);
		ClipArrow[2].transform.position = new Vector3 (temp.x + 0.5f, temp.y + 0.5f, temp.z + 0.6f);
		ClipArrow[2].transform.Rotate (270.0f, 0.0f, 0.0f);

	}

	void spawnEditCubes()
	{
		//---Placing Camera To Default Position------------------------------
		
		GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera");
		camera.transform.position = new Vector3 (0, 0, 20);
		camera.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
		camera.camera.fieldOfView = 10.0f;

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
			if(cube.renderer.materials[0].color.r == 0.8f)
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
			if(cube.tag == "cube")
				brushCube(cube);
		}
	}

	void touchArrow(string name)
	{
		if(name == "Arrow X")
		{
			movingArrow = 0;

			Vector3 touchPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.GetTouch(0).position.x , Input.GetTouch(0).position.y, 20.0f ));
			arrowOffset = touchPos.x - ClipArrow[0].transform.position.x;
		}
		
		if(name == "Arrow Y")
		{
			movingArrow = 1;

			Vector3 touchPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.GetTouch(0).position.x , Input.GetTouch(0).position.y, 20.0f ));
			arrowOffset = touchPos.y - ClipArrow[1].transform.position.y;
		
		}
		
		if(name == "Arrow Z")
		{
			movingArrow = 2;

			Vector3 touchPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.GetTouch(0).position.x , Input.GetTouch(0).position.y, 20.0f ));
			arrowOffset = touchPos.z - ClipArrow[2].transform.position.z;

		}
	}

	void moveArrow()
	{
		if(movingArrow == 0)
		{
			Vector3 wantedPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.GetTouch(0).position.x , Input.GetTouch(0).position.y, 20.0f ));

			ClipArrow[0].transform.position = new Vector3 (wantedPos.x - arrowOffset, ClipArrow[0].transform.position.y, ClipArrow[0].transform.position.z);

			if(wantedPos.x - arrowOffset > levelArray.GetLength(0)/2.0f - 1.1f) 
				ClipArrow[0].transform.position = new Vector3 (levelArray.GetLength(0)/2.0f - 1.1f, ClipArrow[0].transform.position.y, ClipArrow[0].transform.position.z);


			if(wantedPos.x - arrowOffset < - levelArray.GetLength(0)/2.0f - 0.1f)
				ClipArrow[0].transform.position = new Vector3 (- levelArray.GetLength(0)/2.0f - 0.1f, ClipArrow[0].transform.position.y, ClipArrow[0].transform.position.z);
		}

		if(movingArrow == 1)
		{
			Vector3 wantedPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.GetTouch(0).position.x , Input.GetTouch(0).position.y, 20.0f ));

												
			ClipArrow[1].transform.position = new Vector3 (ClipArrow[1].transform.position.x, wantedPos.y - arrowOffset, ClipArrow[1].transform.position.z);

			if(wantedPos.y - arrowOffset > levelArray.GetLength(1)/2.0f + 0.1f) 
			{
				ClipArrow[1].transform.position = new Vector3 (ClipArrow[1].transform.position.x, levelArray.GetLength(1)/2.0f + 0.1f, ClipArrow[1].transform.position.z);
			}
			if(wantedPos.y - arrowOffset < - levelArray.GetLength(1)/2.0f + 1.1f)
			{
				ClipArrow[1].transform.position = new Vector3 (ClipArrow[1].transform.position.x, - levelArray.GetLength(1)/2.0f + 1.1f, ClipArrow[1].transform.position.z);
			}
		}

		if(movingArrow == 2)
		{
			Vector3 wantedPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.GetTouch(0).position.x , Input.GetTouch(0).position.y, 20.0f ));

			ClipArrow[2].transform.position = new Vector3 (ClipArrow[2].transform.position.x, ClipArrow[2].transform.position.y, wantedPos.z - arrowOffset);

			if(wantedPos.z - arrowOffset > levelArray.GetLength(2)/2.0f + 0.1f)
			{
				ClipArrow[2].transform.position = new Vector3 (ClipArrow[2].transform.position.x, ClipArrow[2].transform.position.y, levelArray.GetLength(2)/2.0f + 0.1f);
			}
			if(wantedPos.z - arrowOffset < - levelArray.GetLength(2)/2.0f + 1.1f)
			{
				ClipArrow[2].transform.position = new Vector3 (ClipArrow[2].transform.position.x, ClipArrow[2].transform.position.y, - levelArray.GetLength(2)/2.0f + 1.1f);
			}
		}

		foreach(Cube cube in grid)
		{
			if(cube.cube == null)
				continue;

			if((cube.cube.transform.position.x - 0.5f < ClipArrow[0].transform.position.x) || (cube.cube.transform.position.y + 0.5f > ClipArrow[1].transform.position.y)
			   || (cube.cube.transform.position.z + 0.5f > ClipArrow[2].transform.position.z))
			{
				cube.cube.collider.enabled = false;
				
				foreach(Material mat in cube.cube.renderer.materials)
				{
					int tempMax = Mathf.Max (levelArray.GetLength (0), Mathf.Max (levelArray.GetLength (1), levelArray.GetLength (2)));
					
					mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.4f / tempMax);
				}
				//cube.cube.SetActive(false);
			}
			else
			{
				cube.cube.collider.enabled = true;
				
				foreach(Material mat in cube.cube.renderer.materials)
				{
					mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1.0f);
				}
				
				//cube.cube.SetActive(true);
			}
		}

		//(x - levelArray.GetLength(0)/2.0f + 0.5f,y - levelArray.GetLength(1)/2.0f + 0.5f,z - levelArray.GetLength(2)/2.0f + 0.5f);

			/*
			temp = grid [0, grid.GetLength (1) - 1, 0].cube.transform.position;
			ClipArrow[0] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
			ClipArrow[0].name = "Arrow X";
			ClipArrow[0].transform.position = new Vector3 (temp.x - 0.6f, temp.y + 0.5f, temp.z - 0.5f);
			ClipArrow[0].transform.Rotate (0.0f, 0.0f, 270.0f);

			temp = grid [grid.GetLength (0) - 1, grid.GetLength (1) - 1, 0].cube.transform.position;
			ClipArrow[1] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
			ClipArrow[1].name = "Arrow Y";
			ClipArrow[1].transform.position = new Vector3 (temp.x + 0.5f, temp.y + 0.6f, temp.z - 0.5f);
			ClipArrow[1].transform.Rotate (180.0f, 0.0f, 0.0f);

			temp = grid [grid.GetLength (0) - 1, grid.GetLength (1) - 1, grid.GetLength (2) - 1].cube.transform.position;

			ClipArrow[2] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
			ClipArrow[2].name = "Arrow Z";
			ClipArrow[2].transform.position = new Vector3 (temp.x + 0.5f, temp.y + 0.5f, temp.z + 0.6f);
			ClipArrow[2].transform.Rotate (270.0f, 0.0f, 0.0f);
			*/
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
		cube.renderer.material.color = new Color(0.5f,0.8f,0.8f,1);

		score--;

		starsIngame[score].guiTexture.color = new Color (40.0f/255, 40.0f/255, 40.0f/255, 0.5f);

		if(score == 0)
		{
			openMainMenu();
		}


	}

	void brushCube (GameObject cube)
	{
		if(cube.renderer.materials[0].color.r == 0.8f)
		{
			cube.renderer.materials[0].color = new Color(0.5f,0.8f,0.8f,1);
		}
		else
		{
			cube.renderer.materials[0].color = new Color(0.8f,0.8f,0.8f,1);
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

		for (int i=0; i<3; i++) 
		{
			if(ClipArrow[i] == null)
				break;

			Destroy(ClipArrow[i]);
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

		for (int i=0; i<3; i++) 
		{
			Destroy(ClipArrow[i]);
		}

		foreach (Cube cube in grid) 
		{
			cube.cube.transform.position = new Vector3(cube.cube.transform.position.x, cube.cube.transform.position.y + 1.8f, cube.cube.transform.position.z);
		}
		
		Camera cam = GameObject.FindGameObjectWithTag ("MainCamera").camera;
		
		cam.fieldOfView = 40;
		cam.transform.position = new Vector3 (-5.22f, 5.29f, 6.69f);
		cam.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
	}

	void completedLevel()
	{
		foreach(Cube cube in grid)
		{
			if(cube.cube == null)
				continue;

			Material[] temp = new Material[cube.cube.renderer.materials.Length + 1];

			cube.cube.renderer.materials.CopyTo(temp, 1);
			temp[0] = new Material(Shader.Find ("Diffuse"));
			temp[0].color = cube.color;

			cube.cube.renderer.materials = temp;
		}

		for (int i=0; i<3; i++) 
		{
			if(ClipArrow[i] == null)
				break;
			
			Destroy(ClipArrow[i]);
		}
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
				//magnet
				levelArray = new bool[4,1,4] {{{false, true, true, true}}, {{true, false, false, false}}, {{true, false, false, false}}, {{false, true, true, true}}};
			}
			if(level == 2)
			{
				//dolphin
				levelArray = new bool[3,6,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, true, true, false, false, false, false, false, false}, {false, false, true, true, true, true, true, false, false}, {false, false, false, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
			}
			if(level == 3)
			{
				//table
				levelArray = new bool[3,3,5] {{{true, false, false, false, true}, {true, false, false, false, true}, {true, true, true, true, true}}, {{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}}, {{true, false, false, false, true}, {true, false, false, false, true}, {true, true, true, true, true}}};

			}
			if(level == 4)
			{
				//chair
				levelArray = new bool[3,4,2] {{{true, true}, {true, true}, {true, false}, {true, false}}, {{false, true}, {true, false}, {true, false}, {true, false}}, {{true, true}, {true, true}, {true, false}, {true, false}}};

			}
			if(level == 5)
			{
				//ball
				levelArray = new bool[5,5,5] {{{false, false, false, false, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, false, false, false, false}}, {{false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, true, true, true, false}}, {{false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, true, true, true, false}}, {{false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, true, true, true, false}}, {{false, false, false, false, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, false, false, false, false}}};

			}
			if(level == 6)
			{
				//plane
				levelArray = new bool[8,4,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, false, false, false}, {true, true, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, false, false, false}, {true, true, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 7)
			{
				//computer
				levelArray = new bool[5,5,5] {{{false, false, false, true, true}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}}, {{true, false, false, true, true}, {true, true, false, false, false}, {true, true, false, false, false}, {true, true, false, false, false}, {false, true, false, false, false}}, {{true, false, false, true, true}, {true, true, false, false, false}, {true, true, false, false, false}, {true, true, false, false, false}, {false, true, false, false, false}}, {{true, false, false, true, true}, {true, true, false, false, false}, {true, true, false, false, false}, {true, true, false, false, false}, {false, true, false, false, false}}, {{false, false, false, true, true}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}}};

			}
			if(level == 8)
			{
				//piano
				levelArray = new bool[7,2,4] {{{true, true, true, true}, {true, false, false, false}}, {{true, true, true, true}, {true, true, true, false}}, {{true, true, true, true}, {true, false, false, false}}, {{true, true, true, true}, {true, true, true, false}}, {{true, true, true, true}, {true, false, false, false}}, {{true, true, true, true}, {true, true, true, false}}, {{true, true, true, true}, {true, false, false, false}}};

			}
			if(level == 9)
			{
				//house
				//levelArray = new bool[5,5,5] {{{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}, {{true, true, true, true, true}, {true, false, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{true, true, true, true, true}, {false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}}, {{true, false, true, true, true}, {true, false, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}};
				levelArray = new bool[5,5,5] {{{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}, {{true, true, true, true, true}, {true, false, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{true, true, true, true, true}, {false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}}, {{true, true, true, true, true}, {true, true, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}};

			}
			if(level == 10)
			{
				//phone
				levelArray = new bool[3,4,9] {{{true, true, true, false, false, false, true, true, true}, {false, true, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, false, false, false, true, true, true}, {true, true, true, false, false, false, true, true, true}, {false, true, true, true, true, true, true, true, false}, {false, false, false, true, true, true, false, false, false}}, {{true, true, true, false, false, false, true, true, true}, {false, true, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 11)
			{
				//shopping bag
				levelArray = new bool[3,6,6] {{{true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {false, true, false, false, true, false}, {false, false, true, true, false, false}}, {{true, true, true, true, true, true}, {true, false, false, false, false, true}, {true, false, false, false, false, true}, {true, false, false, false, false, true}, {false, false, false, false, false, false}, {false, false, false, false, false, false}}, {{true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {false, true, false, false, true, false}, {false, false, true, true, false, false}}};

			}
			if(level == 12)
			{
				//kitchen sink
				levelArray = new bool[5,5,7] {{{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {true, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {true, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {true, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, true}, {false, false, true, true, true, false, false}, {false, false, false, true, false, false, false}}};

			}
			if(level == 13)
			{
				//scales
				levelArray = new bool[9,7,3] {{{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, false, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{true, true, true}, {false, false, false}, {false, false, false}, {false, false, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{true, true, true}, {false, true, false}, {false, true, false}, {false, true, false}, {false, true, false}, {false, true, false}, {false, true, false}}, {{true, true, true}, {false, false, false}, {false, false, false}, {false, false, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, false, false}, {false, false, false}}};

			}
			if(level == 14)
			{
				//space shuttle
				levelArray = new bool[9,5,9] {{{false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, true}, {false, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, false, false}, {false, true, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 15)
			{
				//trophy
				levelArray = new bool[3,6,5] {{{false, true, true, true, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, true, false, false}, {false, true, true, true, false}, {false, true, true, true, false}}, {{false, true, true, true, false}, {false, false, true, false, false}, {false, false, true, false, false}, {false, true, true, true, false}, {true, true, false, true, true}, {true, true, false, true, true}}, {{false, true, true, true, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, true, false, false}, {false, true, true, true, false}, {false, true, true, true, false}}};

			}
		}
		if(stage == 2)
		{
			prepareToStart ();

			if(level == 1)
			{
				//hippo
				levelArray = new bool[3,6,6] {{{true, false, false, true, false, false}, {true, true, true, true, true, true}, {true, true, true, true, false, true}, {true, true, true, true, false, false}, {false, false, true, true, false, false}, {false, false, false, true, true, false}}, {{false, false, false, false, false, false}, {true, true, true, true, true, true}, {true, true, true, true, false, false}, {true, true, true, true, false, false}, {false, false, true, true, false, false}, {false, false, false, true, false, false}}, {{false, true, false, true, false, false}, {true, true, true, true, true, true}, {true, true, true, true, false, true}, {true, true, true, true, false, false}, {false, false, true, true, false, false}, {false, false, false, true, true, false}}};

			}
			if(level == 2)
			{
				//motorbike
				levelArray = new bool[3,5,8] {{{false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {false, false, true, true, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, true, false}}, {{false, true, false, false, false, false, true, false}, {true, true, true, true, true, true, true, true}, {false, true, true, true, true, false, true, false}, {true, true, true, true, true, true, true, false}, {false, false, false, false, false, false, true, true}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {false, false, true, true, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, true, false}}};

			}
			if(level == 3)
			{
				//helicopter
				levelArray = new bool[5,5,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, true}}, {{false, false, false, false, false, true, true, true, false}, {false, true, false, false, false, true, true, true, false}, {true, true, true, false, false, true, true, false, false}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, true, false}}, {{false, false, false, false, true, true, true, true, true}, {false, false, true, true, true, true, true, true, true}, {false, true, false, false, false, true, true, true, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, true, false, false}}, {{false, false, false, false, false, true, true, true, false}, {false, false, false, false, false, true, true, true, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, true, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, true}}};

			}
			if(level == 4)
			{
				//steam engine
				levelArray = new bool[5,6,9] {{{true, true, false, true, true, false, true, true, false}, {true, true, true, true, true, false, true, true, true}, {true, true, true, true, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, false, true, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true, false}, {false, false, false, true, false, true, false, true, false}, {false, true, true, true, true, false, false, true, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}, {{true, true, false, true, true, false, true, true, false}, {true, true, true, true, true, false, true, true, true}, {true, true, true, true, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, false, true, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}};

			}
			if(level == 5)
			{
				//fisherman
				levelArray = new bool[4,9,8] {{{true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true}, {false, false, true, false, false, false, true, false}, {false, false, true, false, false, false, true, false}, {false, true, false, false, false, false, false, false}, {false, true, false, true, false, false, false, false}, {false, true, false, false, false, false, false, false}, {true, true, true, false, false, false, false, false}, {false, true, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, true, false, false, false, false}, {false, true, false, true, false, false, false, false}, {false, true, false, false, true, false, false, false}, {true, true, true, false, true, false, false, false}, {false, true, true, false, false, true, false, false}, {false, false, false, false, false, false, true, false}}, {{true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}};

			}
			if(level == 6)
			{
				//vulture
				levelArray = new bool[5,9,5] {{{false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, true}, {false, false, false, false, true}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}}, {{false, true, true, true, false}, {false, true, true, true, false}, {false, true, true, true, true}, {false, false, true, true, false}, {false, false, true, false, false}, {false, false, true, true, false}, {false, false, true, true, false}, {false, false, false, false, false}, {false, false, false, false, false}}, {{false, true, true, true, false}, {false, true, true, false, false}, {false, true, true, true, false}, {false, true, false, false, false}, {false, true, true, true, false}, {false, false, true, true, true}, {false, false, true, true, true}, {false, false, false, false, true}, {false, false, false, false, true}}, {{false, true, true, true, true}, {false, true, false, true, false}, {false, true, true, true, false}, {false, false, true, true, false}, {false, false, true, false, false}, {false, false, true, true, false}, {false, false, true, true, false}, {false, false, false, false, false}, {false, false, false, false, true}}, {{false, false, false, true, true}, {false, false, false, false, false}, {true, true, false, false, false}, {true, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}}};

			}
			if(level == 7)
			{
				//bulldog
				levelArray = new bool[5,5,6] {{{true, false, false, false, true, false}, {false, false, false, true, false, false}, {false, false, false, true, false, false}, {false, false, false, false, false, false}, {false, false, false, true, false, false}}, {{true, false, false, false, false, false}, {true, true, true, true, true, false}, {true, true, true, true, true, true}, {false, true, true, true, true, true}, {false, false, true, true, true, false}}, {{false, false, false, false, false, false}, {true, true, true, true, true, false}, {true, true, true, true, true, false}, {true, true, true, true, true, true}, {true, false, true, true, true, false}}, {{true, false, false, false, false, false}, {true, true, true, true, true, false}, {true, true, true, true, true, true}, {false, true, true, true, true, true}, {false, false, true, true, true, false}}, {{true, false, true, true, false, false}, {false, false, true, false, false, false}, {false, false, false, true, false, false}, {false, false, false, false, false, false}, {false, false, false, true, false, false}}};

			}
			if(level == 8)
			{
				//swallow
				levelArray = new bool[8,2,8] {{{false, false, true, false, false, false, false, true}, {false, false, false, false, false, false, false, false}}, {{false, false, true, false, false, false, true, true}, {false, false, false, false, false, false, false, false}}, {{true, true, true, true, false, true, true, true}, {false, false, true, false, false, false, false, false}}, {{false, false, true, true, true, true, true, false}, {false, false, false, true, true, false, false, false}}, {{false, false, false, true, true, true, false, false}, {false, false, false, true, true, true, false, false}}, {{false, false, true, true, true, true, true, false}, {false, false, false, false, true, true, true, false}}, {{false, true, true, true, false, true, true, false}, {false, false, false, false, false, true, true, false}}, {{true, true, true, false, false, false, false, true}, {false, false, false, false, false, false, false, false}}};

			}
			if(level == 9)
			{
				//rhino
				levelArray = new bool[5,5,9] {{{true, false, false, false, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {false, false, false, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, true, true, false, true, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, false, false, true}, {false, true, true, true, true, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, true, true, false, true, false, false}}, {{true, false, false, false, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {false, false, false, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 10)
			{
				//baseball
				levelArray = new bool[3,8,3] {{{false, true, false}, {false, true, false}, {false, true, true}, {false, true, true}, {false, true, true}, {false, true, true}, {false, true, true}, {false, false, false}}, {{false, false, false}, {false, false, true}, {false, false, true}, {false, false, false}, {true, false, true}, {false, true, true}, {false, true, true}, {false, false, false}}, {{false, false, false}, {false, false, false}, {false, false, false}, {false, false, false}, {false, true, true}, {false, false, true}, {false, false, true}, {false, false, true}}};

			}
			if(level == 11)
			{
				//water wheel
				levelArray = new bool[5,8,9] {{{true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, true, true, true, false, false, false, false}, {false, true, false, true, false, true, false, false, false}, {true, false, false, true, false, false, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, false, false, true, false, false, true, false, false}, {false, true, false, true, false, true, false, false, false}, {false, false, true, true, true, false, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, false, true, false, false, false, false, false}, {false, true, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, false, false, true, false, false, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, true, false, false, false, true, false, false, false}, {false, false, false, true, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, true, true, true, false, false, false, false}, {false, true, false, true, false, true, false, false, false}, {true, false, false, true, false, false, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, false, false, true, false, false, true, false, false}, {false, true, false, true, false, true, false, false, false}, {false, false, true, true, true, false, false, false, false}}, {{true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 12)
			{
				//toilet
				//levelArray = new bool[5,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, true, true, true, false, false}, {false, false, true, false, false, true, false}, {false, true, false, false, false, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}};
				levelArray = new bool[5,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, true, true, true, false, false}, {false, false, true, true, true, true, false}, {false, true, false, false, false, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}};

			}
			if(level == 13)
			{
				//biplane
				levelArray = new bool[9,4,8] {{{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, true, false, false}, {true, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {true, true, false, false, true, true, true, false}, {false, false, false, false, true, true, true, false}, {false, false, false, false, true, true, false, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true}, {true, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {true, true, false, false, true, true, true, true}, {false, false, false, false, true, true, true, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, true, false, false}, {true, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}};

			}
			if(level == 14)
			{
				//bumblebee
				levelArray = new bool[3,7,8] {{{false, false, true, false, true, false, false, false}, {false, false, false, true, false, true, false, true}, {false, true, true, true, true, false, true, false}, {false, true, true, true, true, false, false, false}, {false, false, true, true, true, false, false, true}, {true, true, true, true, false, false, true, false}, {true, true, true, false, false, false, true, true}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{false, false, true, false, true, false, false, false}, {false, false, false, true, false, true, false, true}, {false, true, true, true, true, false, true, false}, {false, true, true, true, true, false, false, false}, {false, false, true, true, true, false, false, true}, {true, true, true, true, false, false, true, false}, {true, true, true, false, false, false, true, true}}};

			}
			if(level == 15)
			{
				//helmet
				levelArray = new bool[8,7,9] {{{false, false, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {false, true, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, true}, {true, false, false, false, false, false, false, false, true}, {true, false, false, false, false, false, false, true, true}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, true}, {true, false, false, false, false, false, false, false, true}, {true, false, false, false, false, false, false, true, true}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{false, true, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {false, true, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
		}
		if(stage == 3)
		{
			prepareToStart ();

			if(level == 1)
			{
				//whale
				levelArray = new bool[8,7,9] {{{false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, true, true, false}, {false, false, false, true, true, true, true, true, true}, {false, false, false, true, true, true, true, true, true}, {true, false, false, false, true, true, true, true, true}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, true}, {true, false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, true}, {true, false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, true, true, false}, {false, false, false, true, true, true, true, true, true}, {false, false, false, true, true, true, true, true, true}, {true, false, false, false, true, true, true, true, true}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false, false}}, {{false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 2)
			{
				//tank
				levelArray = new bool[7,6,7] {{{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, true, true, true, true, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, true, true, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, false}, {true, true, true, true, false, false, false}, {true, true, true, true, false, false, false}, {false, true, true, true, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, true}, {true, true, true, true, false, false, false}, {true, true, true, true, true, true, true}, {false, true, true, true, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, false}, {true, true, true, true, false, false, false}, {true, true, true, true, false, false, false}, {false, true, true, true, false, false, false}}, {{false, false, false, false, false, false, false}, {false, true, true, true, true, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, true, true, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}};

			}
			if(level == 3)
			{
				//farmer
				//levelArray = new bool[5,7,8] {{{false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, true, true, true, true}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, true, true, true, false}, {false, true, false, false, false, true, true, false}, {false, true, false, false, true, true, true, true}, {false, true, false, false, false, false, false, false}}, {{false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {true, true, true, true, true, true, true, true}, {false, true, false, false, false, true, true, false}, {false, true, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}}};
				levelArray = new bool[5,7,8] {{{false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, true, true, true, true}, {false, false, false, false, false, true, true, false}}, {{false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, true, true, true, false}, {false, true, false, false, false, true, true, false}, {false, true, false, false, true, true, true, true}, {false, true, false, false, false, true, true, false}}, {{false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {true, true, true, true, true, true, true, true}, {false, true, false, false, false, true, true, false}, {false, true, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}}};

			}
			if(level == 4)
			{
				//bonsai tree
				levelArray = new bool[4,8,8] {{{false, false, false, false, false, false, false, false}, {false, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, true, false, false, false, true, true, true}, {false, false, false, true, false, false, true, false}, {false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true}, {false, false, false, false, true, false, false, false}, {false, false, false, true, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, false, true, true, false, false, false}, {false, false, true, true, true, true, false, false}, {false, false, false, true, true, false, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, false, false, true, false, false, false, false}, {false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false}}};

			}
			if(level == 5)
			{
				//truck
				levelArray = new bool[5,5,8] {{{false, true, true, false, false, true, true, false}, {false, true, true, false, false, true, true, false}, {true, true, true, true, true, true, true, true}, {true, true, true, true, false, false, true, true}, {false, true, true, true, false, false, true, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, true, true, true, true, true, true}, {false, false, false, true, true, false, true, true}, {false, false, false, false, true, false, true, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, true, true, true, true, true, true}, {false, true, true, true, true, false, true, true}, {false, false, true, true, true, false, true, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, true, true, true, true, true, true}, {false, false, true, true, true, false, true, true}, {false, false, true, true, true, false, true, true}}, {{false, true, true, false, false, true, true, false}, {false, true, true, false, false, true, true, false}, {true, true, true, true, true, true, true, true}, {true, true, true, true, false, false, true, true}, {false, true, true, true, false, false, true, true}}};

			}
			if(level == 6)
			{
				//excavator
				levelArray = new bool[6,8,9] {{{true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {false, false, true, true, true, false, false, false, false}, {false, false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, true, true, false}, {true, true, true, true, true, false, false, true, true}, {true, true, true, true, true, false, false, false, true}, {false, false, true, true, true, false, false, false, false}, {false, false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, true, true, false}, {true, true, true, true, true, false, false, true, true}, {true, true, true, true, false, false, false, false, true}, {true, false, false, true, true, false, false, false, true}, {true, false, false, false, true, true, false, false, true}, {true, false, false, false, false, true, true, false, true}, {false, false, false, false, false, false, true, true, true}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, true, true, false}, {true, true, true, true, true, false, false, true, true}, {true, true, false, false, false, false, false, false, true}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 7)
			{
				//umbrella
				levelArray = new bool[7,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, true, false, false, true}, {false, true, true, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, true, false, false, false, true, true}, {false, false, true, true, true, false, false}, {true, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, false, false, false, true}, {false, true, false, false, false, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, false, false, false}, {true, false, false, true, false, false, true}, {true, false, false, true, false, false, true}, {true, true, false, true, false, true, true}, {false, false, true, true, true, false, false}, {false, false, false, true, false, false, false}}, {{false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, false, false, false, true}, {false, true, false, false, false, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, true, false, false, false, true, true}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, true, false, false, true}, {false, true, true, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}};

			}
			if(level == 8)
			{
				//polar bear
				levelArray = new bool[5,7,9] {{{true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, true, true, false, false}, {false, true, true, false, false, true, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, true, true, false, false}, {false, true, true, false, false, true, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}};

			}
			if(level == 9)
			{
				//frog
				levelArray = new bool[6,5,8] {{{true, true, true, true, false, false, true, true}, {true, true, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false}, {false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{true, true, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false}, {false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, true, false}}, {{false, false, false, false, false, false, false, false}, {false, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true}, {false, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false}, {false, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true}, {false, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}}, {{true, true, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false}, {false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, true, false}}, {{true, true, true, true, false, false, true, true}, {true, true, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false}, {false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}}};

			}
			if(level == 10)
			{
				//elephant
				levelArray = new bool[5,6,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, false, false, false, true, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, true, true, false, true, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false, false, true}, {true, true, true, true, true, true, false, false, true}, {true, true, true, true, true, true, true, false, true}, {false, false, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, false}, {false, false, false, false, false, true, false, false, false}}, {{false, true, false, false, true, false, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, true, true, false, true, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 11)
			{
				//chicken
				levelArray = new bool[7,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, true, true, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, false, false, false}, {true, true, true, true, true, false, false}, {false, false, true, true, true, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, false, false}, {false, true, true, true, true, false, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, false, true, true, false}, {false, false, false, false, false, true, false}}, {{false, false, true, true, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, true}, {false, false, false, false, true, true, false}}, {{false, false, true, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, true, true, true, false}, {false, false, false, true, true, true, true}, {false, false, false, true, true, true, false}, {false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, true}, {false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}}};

			}
			if(level == 12)
			{
				//owl
				//levelArray = new bool[9,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, true}, {false, false, true, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}};
				levelArray = new bool[9,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false}, {true, true, true, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, true}, {false, false, true, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}};


			}
			if(level == 13)
			{
				//phonograph
				levelArray = new bool[7,9,7] {{{false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, true, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}}, {{false, false, true, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, true, false}, {false, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}}, {{false, true, true, true, true, false, false}, {false, false, true, true, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, true, false}, {true, true, false, false, true, false, false}, {false, true, true, true, false, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}}, {{false, true, true, true, true, false, false}, {false, true, true, true, true, false, false}, {true, true, false, false, false, false, true}, {false, false, true, false, false, true, false}, {true, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}}, {{true, true, true, true, true, false, false}, {true, true, true, true, true, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, true}, {true, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, false, false}, {false, false, true, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}};

			}
			if(level == 14)
			{
				//oasis
				levelArray = new bool[7,7,9] {{{true, true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, true}, {false, false, false, false, false, true, true, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, true}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, true}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, true, false, false, true, false, false, true}, {false, false, false, true, true, true, true, true, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, true}, {false, false, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {true, true, true, false, false, true, true, true, true}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, true, true, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

			}
			if(level == 15)
			{
				//gorilla
				levelArray = new bool[9,9,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, true, true, false}}, {{true, true, true, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {false, true, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, true, true, false}}, {{true, true, true, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {false, true, true, false, false, false, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true, false}, {false, false, false, true, true, true, true, false, false}, {false, false, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, false, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true, false}, {false, false, false, true, true, true, true, false, false}, {false, false, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true, false}, {false, false, false, true, true, true, true, false, false}, {false, false, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {false, true, true, false, false, false, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, false, false, false, false, true, true}, {true, true, false, false, false, false, false, false, true}, {false, true, true, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, true, true}, {false, false, false, false, false, false, false, false, true}, {false, false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};

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

		this.level = level;
		
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
		creativeColoring = false;

		buttonsEdit [0].guiTexture.color = creativeColor;
		buttonsEdit [1].guiTexture.color = new Color (0, 0, 0, 1);
	}

	void buttonEditMinus()
	{
		if(pause)
			return;
		
		breaking = true;
		creativeColoring = false;

		buttonsEdit [0].guiTexture.color = new Color (0, 0, 0, 1);
		buttonsEdit [1].guiTexture.color = creativeColor;
	}

	void buttonEditOK()
	{
		convertEditToLevel ();
		customStageCount++;
		saveLevel (customStageCount);
		pauseExit ();
	}

	void buttonEditColor()
	{
		if(pause)
		{
			return;
		}

		creativeColoring = true;
		breaking = true;

		if(colorPalette.activeSelf)
		{
			colorPalette.SetActive(false);
		}
		else
		{
			colorPalette.SetActive(true);
		}

		buttonsEdit [0].guiTexture.color = new Color (0, 0, 0, 1);
		buttonsEdit [1].guiTexture.color = new Color (0, 0, 0, 1);
	}

	void testButton()
	{
		try
		{
			uint temp = (stage - 1) * 15 + level;

			System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath + "/color" + temp + ".txt");

			foreach(Cube cube in grid)
			{
				if(cube.cube == null)
					continue;

				file.WriteLine(cube.cube.renderer.material.color.r);
				file.WriteLine(cube.cube.renderer.material.color.g);
				file.WriteLine(cube.cube.renderer.material.color.b);
			}
			
			file.Close ();			
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
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
		creativeCameraAdjust = true;
		
		creative = true;
		editorX = 1;
		editorY = 1;
		editorZ = 1;
	}

	void testButton2()
	{
		for (int x=0; x<levelArray.GetLength(0); x++)
		{
			for (int y=0; y<levelArray.GetLength(1); y++)
			{
				for (int z=0; z<levelArray.GetLength(2); z++)
				{
					if(grid[x,y,z].cube == null)
						continue;

					if(levelArray[x,y,z] == false)
					{
						GameObject.Destroy(grid[x,y,z].cube);
						continue;
					}
					
					//grid[x,y,z].cube.renderer.materials = new Material[1];
					//grid[x,y,z].cube.renderer.material = new Material(Shader.Find ("Diffuse"));
					//grid[x,y,z].cube.renderer.material.color = new Color(1, 1, 1, 1);
				}
			}
		}
	}

	void convertEditToLevel()
	{
		levelArray = new bool[(int)editorX, (int)editorY, (int)editorZ];
		grid = new Cube[(int)editorX,(int)editorY,(int)editorZ];

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

							creativeCubes[i].color = creativeCubes[i].cube.renderer.material.color;
							grid[lX,lY,lZ] = creativeCubes[i];
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

		for(int i = 1; i <= 15; i++)
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

			//--Save color information-------------------------------------------------

			file = new System.IO.StreamWriter(Application.persistentDataPath + "/custom" + level + "color.txt");

			foreach(Cube cube in grid)
			{
				if((cube == null) || (cube.cube == null))
					continue;
					
				file.WriteLine(cube.cube.renderer.material.color.r);
				file.WriteLine(cube.cube.renderer.material.color.g);
				file.WriteLine(cube.cube.renderer.material.color.b);
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

			file.Write("{");
			for (int x=0; x<levelArray.GetLength(0); x++)
			{
				file.Write("{");
				for (int y=0; y<levelArray.GetLength(1); y++)
				{
					file.Write("{");
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
	public Color color;

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
