using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameScript : MonoBehaviour {

	public Cube[, ,] grid;
	public bool[, ,] levelArray;

	public Material[] numMat = new Material[10];

	public GameObject cubesParent;
	public GameObject endAnimationParent;
	int[] endRandom;
	public List<levelAnimation> animations = new List<levelAnimation>();
	levelAnimation currentAnimation;
	bool isRotation = false;

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

	uint progress = 0;
	ushort[] scores = new ushort[45];

	ushort score;
	bool solved = false;

	GameObject justTouched;
	Cube ghostCube;
	float touchDistance = 0;
	bool creativeCameraAdjust = true;
	float editorX = 1;
	float editorY = 1;
	float editorZ = 1;
	public GUITexture backgroundImage;

	ushort ticks = 0;
	ushort finishTimer = 0;
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

		//--Load Progress---------------------------------------------------------------------
		loadProgress();
		//--Unlock all------------------------------------------------------------------------
		progress = 45;
		//saveProgress ();

		foreach(GameObject go in starsIngame)
		{
			go.guiTexture.pixelInset = new Rect (-Screen.height / 20, -Screen.height / 20, Screen.height / 10, Screen.height / 10);
		}

		spawnMenuCubes ();

		Debug.Log (Screen.width);
		Debug.Log (Screen.height);


		//--Configure Color Palette----------------------------------------------------------------
		//byte[] fileData;		
		//fileData = File.ReadAllBytes(Application.dataPath + "/Resources/Textures/colors.png");
		/*fileData = Resources.Load ("/Textures/colors.png") as byte[];
		colorPaletteTexture = new Texture2D(Screen.width * 2 / 3, Screen.height * 2 / 3);
		colorPaletteTexture.LoadImage(fileData);		
		colorPalette.guiTexture.texture = colorPaletteTexture;
		colorPalette.guiTexture.pixelInset = new Rect(- Screen.width / 3, - Screen.height / 3, Screen.width * 2 / 3, Screen.height * 2 / 3);*/

		colorPaletteTexture = colorPalette.guiTexture.texture as Texture2D;

		//colorPalette.guiTexture.pixelInset = new Rect(- Screen.width / 3, - Screen.height / 3, Screen.width * 2 / 3, Screen.height * 2 / 3);
		//Context.getCacheDir() or (which is way easier) the Environment.getExternalStorageDirectory().


		//testConvert ();
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
					if(solved == false)
					{
						//Debug.Log("yaaaaaas");

						completedLevel();
						solved = true;
						finishTimer = 0;

						/*uint tempLevel = (stage - 1) * 15 + level;
						if((progress < tempLevel) && (stage < 4))
						{
							progress = tempLevel;
							scores[tempLevel - 1] = score;
							saveProgress();
						}*/
					}
				}
			}

			if(solved)
			{
				endAnimationUpdate();



				finishTimer++;

				/*
				if(finishTimer > 360)
				{
					pause = true;
					openPauseMenu();
				}
				*/
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
									//Creative Animations
									if(buttonsEdit[6].activeSelf == true)
									{
										levelAnimation tempAnim = currentAnimation;
										
										currentAnimation = new levelAnimation();
										currentAnimation.origin = tempAnim.origin;
										currentAnimation.cubes = new List<Cube>();
										
										foreach(Cube tempCube in tempAnim.cubes)
										{
											currentAnimation.cubes.Add(tempCube);
										}

										if(hit.collider.gameObject.tag != "cube")
										{
											return;
										}

										if(hit.collider.renderer.materials[0].color.Equals(new Color(0.5f,0.8f,0.8f,1)))
										{

											foreach(Cube cube in grid)
											{
												if(cube == null)
													continue;

												if(cube.cube.Equals(hit.collider.gameObject))
												{
													currentAnimation.cubes.Remove(cube);
													cube.cube.renderer.material.color = cube.color;

													if(currentAnimation.cubes.Count == 0)
													{
														Destroy(ClipArrow[0]);
														Destroy(ClipArrow[1]);
														Destroy(ClipArrow[2]);
													}
													else
													{
														Vector3 temp = currentAnimation.cubes[currentAnimation.cubes.Count - 1].cube.transform.position;

														int x = 0;
														int y = 0;
														int z = 0;

														for (int i=0; i<levelArray.GetLength(0); i++)
														{
															if((x != 0) || (y != 0) || (z != 0))
																break;

															for (int j=0; j<levelArray.GetLength(1); j++)
															{
																for (int k=0; k<levelArray.GetLength(2); k++)
																{
																	if(grid[i,j,k] == null)
																		continue;

																	if(temp.Equals(grid[i,j,k].cube.transform.position))
																	{
																		x = i;
																		y = j;
																		z = k;

																		break;
																	}
																}
															}
														}

														if((x > 0) && (grid[x - 1,y,z] != null))
														{
															ClipArrow[0].transform.position = new Vector3 (temp.x + 0.6f, temp.y, temp.z);
															ClipArrow[0].transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.back);
														}
														else
														{
															ClipArrow[0].transform.position = new Vector3 (temp.x - 0.6f, temp.y, temp.z);
															ClipArrow[0].transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.back);
														}
														ClipArrow[0].transform.parent = grid[x,y,z].cube.transform;
														
														if((y < levelArray.GetLength(1) - 1) && (grid[x,y + 1,z] != null))
														{
															ClipArrow[1].transform.position = new Vector3 (temp.x, temp.y - 0.6f, temp.z);
															ClipArrow[1].transform.rotation = Quaternion.AngleAxis(0.0f, Vector3.left);
														}
														else
														{
															ClipArrow[1].transform.position = new Vector3 (temp.x, temp.y + 0.6f, temp.z);
															ClipArrow[1].transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.left);
														}
														ClipArrow[1].transform.parent = grid[x,y,z].cube.transform;
														
														if((z < levelArray.GetLength(2) - 1) && (grid[x,y,z + 1] != null))
														{
															ClipArrow[2].transform.position = new Vector3 (temp.x, temp.y, temp.z - 0.6f);
															ClipArrow[2].transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.left);
														}
														else
														{
															ClipArrow[2].transform.position = new Vector3 (temp.x, temp.y, temp.z + 0.6f);
															ClipArrow[2].transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.left);
														}
														ClipArrow[2].transform.parent = grid[x,y,z].cube.transform;
													}
													break;
												}
											}

											justTouched = null;
											return;
										}

										hit.collider.renderer.materials[0].color = new Color(0.5f,0.8f,0.8f,1);


										for (int x=0; x<levelArray.GetLength(0); x++)
										{
											for (int y=0; y<levelArray.GetLength(1); y++)
											{
												for (int z=0; z<levelArray.GetLength(2); z++)
												{
													if(grid[x,y,z] == null)
														continue;
													
													if(grid[x,y,z].cube.Equals(hit.collider.gameObject))
													{
														currentAnimation.cubes.Add(grid[x,y,z]);
														
														Vector3 temp = grid[x,y,z].cube.transform.position;
														
														if(currentAnimation.cubes.Count == 1)
														{
															currentAnimation.origin = grid[x,y,z].cube.transform.position;
															
															ClipArrow[0] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
															ClipArrow[0].name = "Arrow X";
															ClipArrow[0].renderer.material.color = new Color (0.75f, 0.25f, 0.25f, 1.0f);
															
															ClipArrow[1] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
															ClipArrow[1].name = "Arrow Y";
															ClipArrow[1].renderer.material.color = new Color (0.25f, 0.75f, 0.25f, 1.0f);
															
															
															ClipArrow[2] = GameObject.Instantiate(Resources.Load("Arrow")) as GameObject;
															ClipArrow[2].name = "Arrow Z";
															ClipArrow[2].renderer.material.color = new Color (0.25f, 0.25f, 0.75f, 1.0f);
														}
														
														if((x > 0) && (grid[x - 1,y,z] != null))
														{
															ClipArrow[0].transform.position = new Vector3 (temp.x + 0.6f, temp.y, temp.z);
															ClipArrow[0].transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.back);
														}
														else
														{
															ClipArrow[0].transform.position = new Vector3 (temp.x - 0.6f, temp.y, temp.z);
															ClipArrow[0].transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.back);
														}
														ClipArrow[0].transform.parent = grid[x,y,z].cube.transform;
														
														if((y < levelArray.GetLength(1) - 1) && (grid[x,y + 1,z] != null))
														{
															ClipArrow[1].transform.position = new Vector3 (temp.x, temp.y - 0.6f, temp.z);
															ClipArrow[1].transform.rotation = Quaternion.AngleAxis(0.0f, Vector3.left);
														}
														else
														{
															ClipArrow[1].transform.position = new Vector3 (temp.x, temp.y + 0.6f, temp.z);
															ClipArrow[1].transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.left);
														}
														ClipArrow[1].transform.parent = grid[x,y,z].cube.transform;
														
														if((z < levelArray.GetLength(2) - 1) && (grid[x,y,z + 1] != null))
														{
															ClipArrow[2].transform.position = new Vector3 (temp.x, temp.y, temp.z - 0.6f);
															ClipArrow[2].transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.left);
														}
														else
														{
															ClipArrow[2].transform.position = new Vector3 (temp.x, temp.y, temp.z + 0.6f);
															ClipArrow[2].transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.left);
														}
														ClipArrow[2].transform.parent = grid[x,y,z].cube.transform;

														break;
													}

												}
											}
										}
													
													justTouched = null;
										return;
									}

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
						readingPalette();

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

				//--Camera control---------------------------------------------------------------------

				cam.transform.LookAt (new Vector3(0, 0, 0), Vector3.up);
				if((Input.GetTouch (0).phase == TouchPhase.Moved) && (justTouched == null))
				{
					float angle;
					if(!(((cam.transform.position.y > 19.8f ) && (Input.GetTouch(0).deltaPosition.y < 0.0f))
					     || ((cam.transform.position.y < -19.8f ) && (Input.GetTouch(0).deltaPosition.y > 0.0f))))
					{
						angle = -Input.GetTouch(0).deltaPosition.y * 2400.0f * camSen / Screen.height;
						cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, angle);
						//cam.transform.RotateAround (new Vector3 (0, 0, 0), cam.transform.right, -Input.GetTouch(0).deltaPosition.y * 300.0f / Screen.height);
					}
					angle = Input.GetTouch(0).deltaPosition.x * 1800.0f * camSen / Screen.height;
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

		cubesParent.transform.rotation = Quaternion.AngleAxis(360, Vector3.down);

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
				
				System.IO.StringReader file;
				
				if(stage <= 3)
				{
					//file = new System.IO.Strea
					//file = new System.IO.StreamReader(Application.dataPath + "/Resources/Levels/color" + tempLevel + ".txt");
					TextAsset bindata= Resources.Load("Levels/colori" + tempLevel) as TextAsset;	
					file = new System.IO.StringReader(bindata.text);
				}
				else
				{
					System.IO.StreamReader tempFile = new System.IO.StreamReader(Application.persistentDataPath + "/custom" + level + "color.txt");
					file = new System.IO.StringReader(tempFile.ReadToEnd());
				}
				
				for (int x=0; x<levelArray.GetLength(0); x++)
				{
					for (int y=0; y<levelArray.GetLength(1); y++)
					{
						for (int z=0; z<levelArray.GetLength(2); z++)
						{

							if(levelArray[x,y,z] == false)
								continue;

							grid[x,y,z].color = hexToColor(file.ReadLine());
							/*
							grid[x,y,z].color = new Color(float.Parse(file.ReadLine()), float.Parse(file.ReadLine()), float.Parse(file.ReadLine()), 1);
							*/
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

		cubesParent.transform.rotation = Quaternion.AngleAxis(360, Vector3.down);

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
		if(creative)
		{

			levelAnimation anim = null;

			foreach(levelAnimation animation in animations)
			{
				if(currentAnimation.cubes.Count != animation.cubes.Count)
				{
					continue;
				}
				if(isRotation && ( currentAnimation.origin != animation.origin))
				{
					continue;
				}

				int tempCount = 0;

				foreach(Cube cube in currentAnimation.cubes)
				{
					if(animation.cubes.Contains(cube))
					{
						tempCount++;
					}
					else
					{
						break;
					}
				}

				if(tempCount < animation.cubes.Count)
				{
					continue;
				}

				anim = animation;

				break;
			}

			if(anim == null)
			{
				anim = new levelAnimation();
				anim.origin = new Vector3(currentAnimation.origin.x, currentAnimation.origin.y, currentAnimation.origin.z);
				anim.cubes = new List<Cube>();
				foreach(Cube cube in grid)
				{
					if(cube == null)
						continue;
					
					if(cube.cube.renderer.material.color.Equals(new Color(0.5f,0.8f,0.8f,1)))
					{
						anim.cubes.Add(cube);
					}
				}

				animations.Add(anim);
				Debug.Log("Anim Count: " + animations.Count);
			}

			if(movingArrow == 0)
			{
				float tempAngle = Input.GetTouch(0).deltaPosition.x  * Camera.main.transform.forward.z + Input.GetTouch(0).deltaPosition.y  * Camera.main.transform.forward.y;
				float tempTrans = Input.GetTouch(0).deltaPosition.x  * Camera.main.transform.forward.z -
					Input.GetTouch(0).deltaPosition.y  * Camera.main.transform.forward.y * Camera.main.transform.forward.x * 2.0f;
				
				if(isRotation)
				{
					anim.angleRight += tempAngle / 5.0f;
				}
				else
				{
					anim.translation = new Vector3(anim.translation.x + tempTrans / 150.0f, anim.translation.y, anim.translation.z);
				}

				foreach(Cube cube in anim.cubes)
				{
					if(cube.cube == null)
					{
						continue;
					}

					if(isRotation)
					{
						cube.cube.transform.RotateAround(anim.origin, Vector3.right, tempAngle / 5.0f);
					}
					else
					{
						cube.cube.transform.Translate(tempTrans / 150.0f, 0, 0);
					}
				}
			}

			if(movingArrow == 1)
			{
				Vector2 tempVect = new Vector2(-ClipArrow[1].transform.up.x * Camera.main.transform.forward.y, ClipArrow[1].transform.up.y);

				float tempAngle = - Input.GetTouch(0).deltaPosition.x * tempVect.x + Input.GetTouch(0).deltaPosition.y * tempVect.y;
				tempAngle *= - ClipArrow[1].transform.forward.z;

				float tempTrans = Input.GetTouch(0).deltaPosition.y;

				if(isRotation)
				{
					anim.angleForward += tempAngle / 5.0f;
				}
				else
				{
					anim.translation = new Vector3(anim.translation.x, anim.translation.y + tempTrans / 200.0f, anim.translation.z);
				}

				foreach(Cube cube in anim.cubes)
				{
					if(cube.cube == null)
					{
						continue;
					}

					if(isRotation)
					{
						cube.cube.transform.RotateAround(anim.origin, Vector3.forward, tempAngle / 5.0f);
					}
					else
					{
						cube.cube.transform.Translate(0, tempTrans / 200.0f, 0);
					}
				}
			}

			if(movingArrow == 2)
			{
				Vector3 camVect = - 1.5f * Camera.main.transform.forward;
				Vector2 tempVect = new Vector2(ClipArrow[2].transform.up.x, ClipArrow[2].transform.up.z);

				float tempAngle = Input.GetTouch(0).deltaPosition.x * tempVect.x * camVect.z + 2 * Input.GetTouch(0).deltaPosition.y * tempVect.y * camVect.z * camVect.y;
				tempAngle += Input.GetTouch(0).deltaPosition.x * camVect.x * -tempVect.y;

				float tempTrans = - Input.GetTouch(0).deltaPosition.x  * Camera.main.transform.forward.x - 
					Input.GetTouch(0).deltaPosition.y  * Camera.main.transform.forward.y * Camera.main.transform.forward.z;
				
				if(isRotation)
				{
					anim.angleUp += tempAngle / 5.0f;					
				}
				else
				{
					anim.translation = new Vector3(anim.translation.x, anim.translation.y, anim.translation.z + tempTrans / 100.0f);
				}

				foreach(Cube cube in anim.cubes)
				{
					if(cube.cube == null)
					{
						continue;
					}

					if(isRotation)
					{				
						cube.cube.transform.RotateAround(anim.origin, Vector3.up, tempAngle / 5.0f);
					}
					else
					{
						cube.cube.transform.Translate(0, 0, tempTrans / 100.0f);
					}
				}
			}

			return;
		}


		Vector3 wantedPos = Camera.main.ScreenToWorldPoint(new Vector3 (Input.GetTouch(0).position.x , Input.GetTouch(0).position.y, 20.0f ));

		if(movingArrow == 0)
		{
			ClipArrow[0].transform.position = new Vector3 (wantedPos.x - arrowOffset, ClipArrow[0].transform.position.y, ClipArrow[0].transform.position.z);

			if(wantedPos.x - arrowOffset > levelArray.GetLength(0)/2.0f - 1.1f) 
				ClipArrow[0].transform.position = new Vector3 (levelArray.GetLength(0)/2.0f - 1.1f, ClipArrow[0].transform.position.y, ClipArrow[0].transform.position.z);


			if(wantedPos.x - arrowOffset < - levelArray.GetLength(0)/2.0f - 0.1f)
				ClipArrow[0].transform.position = new Vector3 (- levelArray.GetLength(0)/2.0f - 0.1f, ClipArrow[0].transform.position.y, ClipArrow[0].transform.position.z);
		}

		if(movingArrow == 1)
		{										
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
			pause = true;
			openPauseMenu();
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

	void readingPalette()
	{
		if((Input.GetTouch(0).position.x > Screen.width / 6) && (Input.GetTouch(0).position.x < Screen.width * 5 / 6)
		   && (Input.GetTouch(0).position.y > Screen.height / 6) && (Input.GetTouch(0).position.y < Screen.height * 5 / 6))
		{
			int tempX = (int) ((Input.GetTouch(0).position.x - Screen.width / 6) * (512.0f / (Screen.width * 2 / 3)));
			int tempY = (int) ((Input.GetTouch(0).position.y - Screen.height / 6) * (512.0f / (Screen.height * 2 / 3)));

			creativeColor = colorPaletteTexture.GetPixel(tempX, tempY);
			buttonsEdit[2].guiTexture.color = creativeColor;

		}
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

		for (int i =0; i < endAnimationParent.transform.childCount; i++)
		{
			Destroy(endAnimationParent.transform.GetChild(i).gameObject);
		}

		grid = null;
		creativeCubes.Clear ();
		animations.Clear ();
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

		Camera.main.transform.GetChild(0).light.intensity = 0.3f;
		backgroundImage.color = new Color (0.5f, 0.5f, 0.5f);

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
		startEndAnimation();

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
		if (progress < 15)
		{
			buttonsMenu[4].SendMessage("Start");
			buttonsMenu[4].guiTexture.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

			ButtonMenuScript tempScript = buttonsMenu [4].GetComponent("ButtonMenuScript") as ButtonMenuScript;
			tempScript.enabled = false;
		}
		else
		{
			buttonsMenu[4].guiTexture.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			
			ButtonMenuScript tempScript = buttonsMenu [4].GetComponent("ButtonMenuScript") as ButtonMenuScript;
			tempScript.enabled = true;
		}

		buttonsMenu[5].SetActive (true);
		if (progress < 30)
		{
			buttonsMenu[5].SendMessage("Start");
			buttonsMenu[5].guiTexture.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
			
			ButtonMenuScript tempScript = buttonsMenu [5].GetComponent("ButtonMenuScript") as ButtonMenuScript;
			tempScript.enabled = false;
		}
		else
		{
			buttonsMenu[5].guiTexture.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			
			ButtonMenuScript tempScript = buttonsMenu [5].GetComponent("ButtonMenuScript") as ButtonMenuScript;
			tempScript.enabled = true;
		}


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

	void buttonCamera()
	{

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
		buttonsMenu[8].SetActive (false);
		buttonsMenu[9].SetActive (false);
		buttonsMenu[10].SetActive (false);
		buttonsMenu[11].SetActive (false);
		buttonsMenu[12].SetActive (false);

		buttonsMenu[13].SetActive (true);
		buttonsMenu[14].SetActive (true);
		buttonsMenu[15].SetActive (true);
	}

	void buttonSetYes()
	{
		progress = 0;
		scores = new ushort[45];

		saveProgress ();

		buttonsMenu[8].SetActive (true);
		buttonsMenu[9].SetActive (true);
		buttonsMenu[10].SetActive (true);
		buttonsMenu[11].SetActive (true);
		buttonsMenu[12].SetActive (true);

		buttonsMenu[13].SetActive (false);
		buttonsMenu[14].SetActive (false);
		buttonsMenu[15].SetActive (false);
	}

	void buttonSetNo()
	{
		buttonsMenu[8].SetActive (true);
		buttonsMenu[9].SetActive (true);
		buttonsMenu[10].SetActive (true);
		buttonsMenu[11].SetActive (true);
		buttonsMenu[12].SetActive (true);

		buttonsMenu[13].SetActive (false);
		buttonsMenu[14].SetActive (false);
		buttonsMenu[15].SetActive (false);
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
		
		/*foreach(GameObject go in buttonsLevels)
		{
			go.SetActive(true);
		}*/

		buttonsLevels[0].SetActive(true);

		for(int i = 1; i < 16; i++)
		{
			//uint tempLevel = (stage - 1) * 15 + level;
			if(progress - (stage - 1) * 15 >= i - 1)
			{
				buttonsLevels[i].SetActive(true);
				if(scores[i + (stage - 1) * 15 - 1] == 5)
				{
					buttonsLevels[i].guiTexture.color = new Color(0.5f, 0.5f, 0f, 0.5f);
				}
				else
				{
					buttonsLevels[i].guiTexture.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
				}

				ButtonLevelScript tempScript = buttonsLevels[i].GetComponent("ButtonLevelScript") as ButtonLevelScript;
				tempScript.enabled = true;

				
				/*{
					buttonsMenu[4].SendMessage("Start");
					buttonsMenu[4].guiTexture.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
					
					ButtonMenuScript tempScript = buttonsMenu [4].GetComponent("ButtonMenuScript") as ButtonMenuScript;
					tempScript.enabled = false;
				}
				else
				{
					buttonsMenu[4].guiTexture.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
					
					ButtonMenuScript tempScript = buttonsMenu [4].GetComponent("ButtonMenuScript") as ButtonMenuScript;
					tempScript.enabled = true;
				}*/
			}
			else
			{
				buttonsLevels[i].SetActive(true);
				buttonsLevels[i].guiTexture.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

				buttonsLevels[i].SendMessage("Start");
				ButtonLevelScript tempScript = buttonsLevels[i].GetComponent("ButtonLevelScript") as ButtonLevelScript;
				tempScript.enabled = false;
			}
		}

		buttonsLevels[16].SetActive(true);

		//buttonsLevels [17].SetActive (false);
		//buttonsLevels [18].SetActive (false);
		//buttonsLevels [19].SetActive (false);

		
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

		for(int i = 1; i <= customStageCount; i++)
		{
			buttonsLevels[i].SetActive(true);

			buttonsLevels[i].guiTexture.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			ButtonLevelScript tempScript = buttonsLevels[i].GetComponent("ButtonLevelScript") as ButtonLevelScript;
			tempScript.enabled = true;
		}

		buttonsLevels [0].SetActive(true);
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

			if(stage == 4)
			{
				if(level >= customStageCount)
				{
					buttonStart();
					buttonStage4();
					return;
				}
			}

			if(level < 15)
			{
				buttonLevel(level + 1);
			}
			else
			{
				stage++;
				buttonLevel(1);
			}
		}
		else
		{
			if(score == 0)
			{
				openMainMenu();
				buttonLevel(level);
			}

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
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 2)
			{
				//dolphin
				levelArray = new bool[3,6,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, true, true, false, false, false, false, false, false}, {false, false, true, true, true, true, true, false, false}, {false, false, false, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg3", typeof(Texture2D)) as Texture2D;
			}
			if(level == 3)
			{
				//table
				levelArray = new bool[3,3,5] {{{true, false, false, false, true}, {true, false, false, false, true}, {true, true, true, true, true}}, {{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}}, {{true, false, false, false, true}, {true, false, false, false, true}, {true, true, true, true, true}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 4)
			{
				//chair
				levelArray = new bool[3,4,2] {{{true, true}, {true, true}, {true, false}, {true, false}}, {{false, true}, {true, false}, {true, false}, {true, false}}, {{true, true}, {true, true}, {true, false}, {true, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 5)
			{
				//ball
				levelArray = new bool[5,5,5] {{{false, false, false, false, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, false, false, false, false}}, {{false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, true, true, true, false}}, {{false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, true, true, true, false}}, {{false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, true, true, true, false}}, {{false, false, false, false, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, true, true, true, false}, {false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
			if(level == 6)
			{
				//plane
				levelArray = new bool[8,4,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, false, false, false}, {true, true, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, false, false, false}, {true, true, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg6", typeof(Texture2D)) as Texture2D;
			}
			if(level == 7)
			{
				//computer
				levelArray = new bool[5,5,5] {{{false, false, false, true, true}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}}, {{true, false, false, true, true}, {true, true, false, false, false}, {true, true, false, false, false}, {true, true, false, false, false}, {false, true, false, false, false}}, {{true, false, false, true, true}, {true, true, false, false, false}, {true, true, false, false, false}, {true, true, false, false, false}, {false, true, false, false, false}}, {{true, false, false, true, true}, {true, true, false, false, false}, {true, true, false, false, false}, {true, true, false, false, false}, {false, true, false, false, false}}, {{false, false, false, true, true}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}, {false, true, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 8)
			{
				//piano
				levelArray = new bool[7,2,4] {{{true, true, true, true}, {true, false, false, false}}, {{true, true, true, true}, {true, true, true, false}}, {{true, true, true, true}, {true, false, false, false}}, {{true, true, true, true}, {true, true, true, false}}, {{true, true, true, true}, {true, false, false, false}}, {{true, true, true, true}, {true, true, true, false}}, {{true, true, true, true}, {true, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 9)
			{
				//house
				//levelArray = new bool[5,5,5] {{{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}, {{true, true, true, true, true}, {true, false, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{true, true, true, true, true}, {false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}}, {{true, false, true, true, true}, {true, false, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}};
				levelArray = new bool[5,5,5] {{{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}, {{true, true, true, true, true}, {true, false, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{true, true, true, true, true}, {false, true, true, true, false}, {true, true, true, true, true}, {true, true, true, true, true}, {true, true, true, true, true}}, {{true, true, true, true, true}, {true, true, true, false, true}, {true, true, true, true, true}, {true, true, true, true, true}, {false, false, false, false, false}}, {{false, false, false, false, false}, {false, false, false, false, false}, {true, true, true, true, true}, {false, false, false, false, false}, {false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
			if(level == 10)
			{
				//phone
				levelArray = new bool[3,4,9] {{{true, true, true, false, false, false, true, true, true}, {false, true, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, false, false, false, true, true, true}, {true, true, true, false, false, false, true, true, true}, {false, true, true, true, true, true, true, true, false}, {false, false, false, true, true, true, false, false, false}}, {{true, true, true, false, false, false, true, true, true}, {false, true, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 11)
			{
				//shopping bag
				levelArray = new bool[3,6,6] {{{true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {false, true, false, false, true, false}, {false, false, true, true, false, false}}, {{true, true, true, true, true, true}, {true, false, false, false, false, true}, {true, false, false, false, false, true}, {true, false, false, false, false, true}, {false, false, false, false, false, false}, {false, false, false, false, false, false}}, {{true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {true, true, true, true, true, true}, {false, true, false, false, true, false}, {false, false, true, true, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 12)
			{
				//kitchen sink
				levelArray = new bool[5,5,7] {{{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {true, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {true, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {true, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, true}, {false, false, true, true, true, false, false}, {false, false, false, true, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg3", typeof(Texture2D)) as Texture2D;
			}
			if(level == 13)
			{
				//scales
				levelArray = new bool[9,7,3] {{{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, false, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{true, true, true}, {false, false, false}, {false, false, false}, {false, false, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{true, true, true}, {false, true, false}, {false, true, false}, {false, true, false}, {false, true, false}, {false, true, false}, {false, true, false}}, {{true, true, true}, {false, false, false}, {false, false, false}, {false, false, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, false, false}, {false, true, false}, {false, false, false}, {false, false, false}}, {{false, false, false}, {true, true, true}, {false, true, false}, {false, true, false}, {false, false, false}, {false, false, false}, {false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 14)
			{
				//space shuttle
				levelArray = new bool[9,5,9] {{{false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, true}, {false, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, false, false}, {false, true, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg7", typeof(Texture2D)) as Texture2D;
			}
			if(level == 15)
			{
				//trophy
				levelArray = new bool[3,6,5] {{{false, true, true, true, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, true, false, false}, {false, true, true, true, false}, {false, true, true, true, false}}, {{false, true, true, true, false}, {false, false, true, false, false}, {false, false, true, false, false}, {false, true, true, true, false}, {true, true, false, true, true}, {true, true, false, true, true}}, {{false, true, true, true, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, true, false, false}, {false, true, true, true, false}, {false, true, true, true, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
		}
		if(stage == 2)
		{
			prepareToStart ();

			if(level == 1)
			{
				//hippo
				levelArray = new bool[3,6,6] {{{true, false, false, true, false, false}, {true, true, true, true, true, true}, {true, true, true, true, false, true}, {true, true, true, true, false, false}, {false, false, true, true, false, false}, {false, false, false, true, true, false}}, {{false, false, false, false, false, false}, {true, true, true, true, true, true}, {true, true, true, true, false, false}, {true, true, true, true, false, false}, {false, false, true, true, false, false}, {false, false, false, true, false, false}}, {{false, true, false, true, false, false}, {true, true, true, true, true, true}, {true, true, true, true, false, true}, {true, true, true, true, false, false}, {false, false, true, true, false, false}, {false, false, false, true, true, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg8", typeof(Texture2D)) as Texture2D;
			}
			if(level == 2)
			{
				//motorbike
				levelArray = new bool[3,5,8] {{{false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {false, false, true, true, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, true, false}}, {{false, true, false, false, false, false, true, false}, {true, true, true, true, true, true, true, true}, {false, true, true, true, true, false, true, false}, {true, true, true, true, true, true, true, false}, {false, false, false, false, false, false, true, true}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {false, false, true, true, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, true, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
			if(level == 3)
			{
				//helicopter
				levelArray = new bool[5,5,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, true}}, {{false, false, false, false, false, true, true, true, false}, {false, true, false, false, false, true, true, true, false}, {true, true, true, false, false, true, true, false, false}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, true, false}}, {{false, false, false, false, true, true, true, true, true}, {false, false, true, true, true, true, true, true, true}, {false, true, false, false, false, true, true, true, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, true, false, false}}, {{false, false, false, false, false, true, true, true, false}, {false, false, false, false, false, true, true, true, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, true, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, true}}};
				backgroundImage.texture = Resources.Load("Textures/bg6", typeof(Texture2D)) as Texture2D;
			}
			if(level == 4)
			{
				//steam engine
				levelArray = new bool[5,6,9] {{{true, true, false, true, true, false, true, true, false}, {true, true, true, true, true, false, true, true, true}, {true, true, true, true, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, false, true, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true, false}, {false, false, false, true, false, true, false, true, false}, {false, true, true, true, true, false, false, true, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}, {{true, true, false, true, true, false, true, true, false}, {true, true, true, true, true, false, true, true, true}, {true, true, true, true, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, false, true, false, false, false, false, false}, {false, true, true, true, true, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg9", typeof(Texture2D)) as Texture2D;
			}
			if(level == 5)
			{
				//fisherman
				levelArray = new bool[4,9,8] {{{true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true}, {false, false, true, false, false, false, true, false}, {false, false, true, false, false, false, true, false}, {false, true, false, false, false, false, false, false}, {false, true, false, true, false, false, false, false}, {false, true, false, false, false, false, false, false}, {true, true, true, false, false, false, false, false}, {false, true, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, true, false, false, false, false}, {false, true, false, true, false, false, false, false}, {false, true, false, false, true, false, false, false}, {true, true, true, false, true, false, false, false}, {false, true, true, false, false, true, false, false}, {false, false, false, false, false, false, true, false}}, {{true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg2", typeof(Texture2D)) as Texture2D;
			}
			if(level == 6)
			{
				//vulture
				levelArray = new bool[5,9,5] {{{false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, true}, {false, false, false, false, true}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}}, {{false, true, true, true, false}, {false, true, true, true, false}, {false, true, true, true, true}, {false, false, true, true, false}, {false, false, true, false, false}, {false, false, true, true, false}, {false, false, true, true, false}, {false, false, false, false, false}, {false, false, false, false, false}}, {{false, true, true, true, false}, {false, true, true, false, false}, {false, true, true, true, false}, {false, true, false, false, false}, {false, true, true, true, false}, {false, false, true, true, true}, {false, false, true, true, true}, {false, false, false, false, true}, {false, false, false, false, true}}, {{false, true, true, true, true}, {false, true, false, true, false}, {false, true, true, true, false}, {false, false, true, true, false}, {false, false, true, false, false}, {false, false, true, true, false}, {false, false, true, true, false}, {false, false, false, false, false}, {false, false, false, false, true}}, {{false, false, false, true, true}, {false, false, false, false, false}, {true, true, false, false, false}, {true, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}, {false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg8", typeof(Texture2D)) as Texture2D;
			}
			if(level == 7)
			{
				//bulldog
				levelArray = new bool[5,5,6] {{{true, false, false, false, true, false}, {false, false, false, true, false, false}, {false, false, false, true, false, false}, {false, false, false, false, false, false}, {false, false, false, true, false, false}}, {{true, false, false, false, false, false}, {true, true, true, true, true, false}, {true, true, true, true, true, true}, {false, true, true, true, true, true}, {false, false, true, true, true, false}}, {{false, false, false, false, false, false}, {true, true, true, true, true, false}, {true, true, true, true, true, false}, {true, true, true, true, true, true}, {true, false, true, true, true, false}}, {{true, false, false, false, false, false}, {true, true, true, true, true, false}, {true, true, true, true, true, true}, {false, true, true, true, true, true}, {false, false, true, true, true, false}}, {{true, false, true, true, false, false}, {false, false, true, false, false, false}, {false, false, false, true, false, false}, {false, false, false, false, false, false}, {false, false, false, true, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
			if(level == 8)
			{
				//swallow
				levelArray = new bool[8,2,8] {{{false, false, true, false, false, false, false, true}, {false, false, false, false, false, false, false, false}}, {{false, false, true, false, false, false, true, true}, {false, false, false, false, false, false, false, false}}, {{true, true, true, true, false, true, true, true}, {false, false, true, false, false, false, false, false}}, {{false, false, true, true, true, true, true, false}, {false, false, false, true, true, false, false, false}}, {{false, false, false, true, true, true, false, false}, {false, false, false, true, true, true, false, false}}, {{false, false, true, true, true, true, true, false}, {false, false, false, false, true, true, true, false}}, {{false, true, true, true, false, true, true, false}, {false, false, false, false, false, true, true, false}}, {{true, true, true, false, false, false, false, true}, {false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg6", typeof(Texture2D)) as Texture2D;
			}
			if(level == 9)
			{
				//rhino
				levelArray = new bool[5,5,9] {{{true, false, false, false, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {false, false, false, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, true, true, false, true, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, false, false, true}, {false, true, true, true, true, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, true, true, false, true, false, false}}, {{true, false, false, false, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {false, false, false, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg8", typeof(Texture2D)) as Texture2D;
			}
			if(level == 10)
			{
				//baseball
				levelArray = new bool[3,8,3] {{{false, true, false}, {false, true, false}, {false, true, true}, {false, true, true}, {false, true, true}, {false, true, true}, {false, true, true}, {false, false, false}}, {{false, false, false}, {false, false, true}, {false, false, true}, {false, false, false}, {true, false, true}, {false, true, true}, {false, true, true}, {false, false, false}}, {{false, false, false}, {false, false, false}, {false, false, false}, {false, false, false}, {false, true, true}, {false, false, true}, {false, false, true}, {false, false, true}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
			if(level == 11)
			{
				//water wheel
				levelArray = new bool[5,8,9] {{{true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, true, true, true, false, false, false, false}, {false, true, false, true, false, true, false, false, false}, {true, false, false, true, false, false, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, false, false, true, false, false, true, false, false}, {false, true, false, true, false, true, false, false, false}, {false, false, true, true, true, false, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, false, true, false, false, false, false, false}, {false, true, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, false, false, true, false, false, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, true, false, false, false, true, false, false, false}, {false, false, false, true, false, false, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, true, true, true, false, false, false, false}, {false, true, false, true, false, true, false, false, false}, {true, false, false, true, false, false, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, false, false, true, false, false, true, false, false}, {false, true, false, true, false, true, false, false, false}, {false, false, true, true, true, false, false, false, false}}, {{true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg2", typeof(Texture2D)) as Texture2D;
			}
			if(level == 12)
			{
				//toilet
				//levelArray = new bool[5,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, true, true, true, false, false}, {false, false, true, false, false, true, false}, {false, true, false, false, false, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}};
				levelArray = new bool[5,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, true, true, true, false, false}, {false, false, true, true, true, true, false}, {false, true, false, false, false, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, true, true, true, false, false}, {false, false, true, true, true, true, true}, {true, true, false, false, false, false, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, true, true}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg3", typeof(Texture2D)) as Texture2D;
			}
			if(level == 13)
			{
				//biplane
				levelArray = new bool[9,4,8] {{{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, true, false, false}, {true, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {true, true, false, false, true, true, true, false}, {false, false, false, false, true, true, true, false}, {false, false, false, false, true, true, false, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, true, false}, {true, true, false, true, true, true, true, true}, {true, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {true, true, false, false, true, true, true, true}, {false, false, false, false, true, true, true, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, true, false, false}, {true, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg6", typeof(Texture2D)) as Texture2D;
			}
			if(level == 14)
			{
				//bumblebee
				levelArray = new bool[3,7,8] {{{false, false, true, false, true, false, false, false}, {false, false, false, true, false, true, false, true}, {false, true, true, true, true, false, true, false}, {false, true, true, true, true, false, false, false}, {false, false, true, true, true, false, false, true}, {true, true, true, true, false, false, true, false}, {true, true, true, false, false, false, true, true}}, {{false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{false, false, true, false, true, false, false, false}, {false, false, false, true, false, true, false, true}, {false, true, true, true, true, false, true, false}, {false, true, true, true, true, false, false, false}, {false, false, true, true, true, false, false, true}, {true, true, true, true, false, false, true, false}, {true, true, true, false, false, false, true, true}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
			if(level == 15)
			{
				//helmet
				levelArray = new bool[8,7,9] {{{false, false, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {false, true, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, true}, {true, false, false, false, false, false, false, false, true}, {true, false, false, false, false, false, false, true, true}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, true, true}, {true, false, false, false, false, false, false, false, true}, {true, false, false, false, false, false, false, true, true}, {false, true, false, false, false, false, false, false, false}, {false, true, false, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false, false}}, {{false, true, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {true, false, false, false, false, false, false, false, false}, {true, false, false, false, false, true, true, true, false}, {false, true, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
		}
		if(stage == 3)
		{
			prepareToStart ();

			if(level == 1)
			{
				//whale
				levelArray = new bool[8,7,9] {{{false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, true, true, false}, {false, false, false, true, true, true, true, true, true}, {false, false, false, true, true, true, true, true, true}, {true, false, false, false, true, true, true, true, true}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, true}, {true, false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, true, true}, {true, false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, true, true, true, false}, {false, false, false, true, true, true, true, true, true}, {false, false, false, true, true, true, true, true, true}, {true, false, false, false, true, true, true, true, true}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false, false}}, {{false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg3", typeof(Texture2D)) as Texture2D;
			}
			if(level == 2)
			{
				//tank
				levelArray = new bool[7,6,7] {{{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, true, true, true, true, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, true, true, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, false}, {true, true, true, true, false, false, false}, {true, true, true, true, false, false, false}, {false, true, true, true, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, true}, {true, true, true, true, false, false, false}, {true, true, true, true, true, true, true}, {false, true, true, true, false, false, false}}, {{false, false, false, false, false, false, false}, {true, true, true, true, true, true, true}, {true, true, true, true, true, true, false}, {true, true, true, true, false, false, false}, {true, true, true, true, false, false, false}, {false, true, true, true, false, false, false}}, {{false, false, false, false, false, false, false}, {false, true, true, true, true, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, true, true, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg9", typeof(Texture2D)) as Texture2D;
			}
			if(level == 3)
			{
				//farmer
				//levelArray = new bool[5,7,8] {{{false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, true, true, true, true}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, true, true, true, false}, {false, true, false, false, false, true, true, false}, {false, true, false, false, true, true, true, true}, {false, true, false, false, false, false, false, false}}, {{false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {true, true, true, true, true, true, true, true}, {false, true, false, false, false, true, true, false}, {false, true, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}}};
				levelArray = new bool[5,7,8] {{{false, false, true, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false}, {false, false, false, false, true, true, true, true}, {false, false, false, false, false, true, true, false}}, {{false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, false, false}, {false, false, false, false, true, true, true, false}, {false, true, false, false, false, true, true, false}, {false, true, false, false, true, true, true, true}, {false, true, false, false, false, true, true, false}}, {{false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false}, {true, true, true, true, true, true, true, true}, {false, true, false, false, false, true, true, false}, {false, true, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg11", typeof(Texture2D)) as Texture2D;
			}
			if(level == 4)
			{
				//bonsai tree
				levelArray = new bool[4,8,8] {{{false, false, false, false, false, false, false, false}, {false, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false}, {false, true, false, false, false, true, true, true}, {false, false, false, true, false, false, true, false}, {false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false}}, {{false, false, true, true, true, true, true, false}, {false, true, true, true, true, true, true, true}, {false, false, false, false, true, false, false, false}, {false, false, false, true, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, false, true, true, false, false, false}, {false, false, true, true, true, true, false, false}, {false, false, false, true, true, false, false, false}}, {{false, false, false, false, false, false, false, false}, {false, false, true, true, true, true, true, false}, {false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false}, {false, true, false, false, false, false, false, false}, {false, false, false, true, false, false, false, false}, {false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg5", typeof(Texture2D)) as Texture2D;
			}
			if(level == 5)
			{
				//truck
				levelArray = new bool[5,5,8] {{{false, true, true, false, false, true, true, false}, {false, true, true, false, false, true, true, false}, {true, true, true, true, true, true, true, true}, {true, true, true, true, false, false, true, true}, {false, true, true, true, false, false, true, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, true, true, true, true, true, true}, {false, false, false, true, true, false, true, true}, {false, false, false, false, true, false, true, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, true, true, true, true, true, true}, {false, true, true, true, true, false, true, true}, {false, false, true, true, true, false, true, true}}, {{false, false, false, false, false, false, false, false}, {true, true, true, true, false, false, true, false}, {false, true, true, true, true, true, true, true}, {false, false, true, true, true, false, true, true}, {false, false, true, true, true, false, true, true}}, {{false, true, true, false, false, true, true, false}, {false, true, true, false, false, true, true, false}, {true, true, true, true, true, true, true, true}, {true, true, true, true, false, false, true, true}, {false, true, true, true, false, false, true, true}}};
				backgroundImage.texture = Resources.Load("Textures/bg9", typeof(Texture2D)) as Texture2D;
			}
			if(level == 6)
			{
				//excavator
				levelArray = new bool[6,8,9] {{{true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {false, false, true, true, true, false, false, false, false}, {false, false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, true, true, false}, {true, true, true, true, true, false, false, true, true}, {true, true, true, true, true, false, false, false, true}, {false, false, true, true, true, false, false, false, false}, {false, false, true, true, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, true, true, false}, {true, true, true, true, true, false, false, true, true}, {true, true, true, true, false, false, false, false, true}, {true, false, false, true, true, false, false, false, true}, {true, false, false, false, true, true, false, false, true}, {true, false, false, false, false, true, true, false, true}, {false, false, false, false, false, false, true, true, true}}, {{false, false, false, false, false, false, false, false, false}, {false, true, true, true, false, false, true, true, false}, {true, true, true, true, true, false, false, true, true}, {true, true, false, false, false, false, false, false, true}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg9", typeof(Texture2D)) as Texture2D;
			}
			if(level == 7)
			{
				//umbrella
				levelArray = new bool[7,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, true, false, false, true}, {false, true, true, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, true, false, false, false, true, true}, {false, false, true, true, true, false, false}, {true, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, false, false, false, true}, {false, true, false, false, false, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, false, false, false}, {true, false, false, true, false, false, true}, {true, false, false, true, false, false, true}, {true, true, false, true, false, true, true}, {false, false, true, true, true, false, false}, {false, false, false, true, false, false, false}}, {{false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, false, false, false, true}, {false, true, false, false, false, true, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, true, false, false, false, true, true}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, false, false, true, false, false, true}, {false, true, true, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg6", typeof(Texture2D)) as Texture2D;
			}
			if(level == 8)
			{
				//polar bear
				levelArray = new bool[5,7,9] {{{true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {true, true, true, true, true, false, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{true, true, false, true, true, false, false, false, false}, {true, true, false, true, true, false, false, false, false}, {true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, true}, {true, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, true, true, false, false}, {false, true, true, false, false, true, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true, false}, {true, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, false, false, true, true, false, false}, {false, true, true, false, false, true, true, false, false}, {true, true, true, true, true, true, true, false, false}, {true, true, true, true, true, true, false, false, false}, {true, true, true, true, true, true, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg10", typeof(Texture2D)) as Texture2D;
			}
			if(level == 9)
			{
				//frog
				levelArray = new bool[6,5,8] {{{true, true, true, true, false, false, true, true}, {true, true, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false}, {false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}}, {{true, true, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false}, {false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, true, false}}, {{false, false, false, false, false, false, false, false}, {false, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true}, {false, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false}, {false, true, true, true, true, true, false, false}, {true, true, true, true, true, true, true, true}, {false, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false}}, {{true, true, false, false, false, false, true, false}, {false, true, false, false, false, false, false, false}, {false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true}, {false, false, false, false, false, false, true, false}}, {{true, true, true, true, false, false, true, true}, {true, true, false, false, false, true, false, false}, {false, false, true, true, true, true, false, false}, {false, false, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg2", typeof(Texture2D)) as Texture2D;
			}
			if(level == 10)
			{
				//elephant
				levelArray = new bool[5,6,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, false, false, false, true, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, true, true, false, true, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false, false, true}, {true, true, true, true, true, true, false, false, true}, {true, true, true, true, true, true, true, false, true}, {false, false, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, false}, {false, false, false, false, false, true, false, false, false}}, {{false, true, false, false, true, false, false, false, false}, {false, true, true, true, true, false, false, false, false}, {false, true, true, true, true, true, true, false, false}, {false, false, false, false, true, true, false, true, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg8", typeof(Texture2D)) as Texture2D;
			}
			if(level == 11)
			{
				//chicken
				levelArray = new bool[7,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, true, true, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, false, false, false}, {true, true, true, true, true, false, false}, {false, false, true, true, true, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, true, true, true, false, false}, {false, true, true, true, true, false, false}, {false, false, true, true, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, false, true, true, false}, {false, false, false, false, false, true, false}}, {{false, false, true, true, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, true}, {false, false, false, false, true, true, false}}, {{false, false, true, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, true, true, true, false}, {false, false, false, true, true, true, true}, {false, false, false, true, true, true, false}, {false, false, false, true, true, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, true}, {false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg11", typeof(Texture2D)) as Texture2D;
			}
			if(level == 12)
			{
				//owl
				//levelArray = new bool[9,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {true, false, false, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, true}, {false, false, true, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}};
				levelArray = new bool[9,8,7] {{{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{true, false, false, false, false, false, false}, {true, true, true, false, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, true}, {false, true, true, true, true, true, true}, {false, false, true, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, true, false, false}, {false, true, true, true, false, false, false}, {true, true, true, true, true, false, false}, {true, true, true, true, true, true, false}, {true, true, true, true, true, true, false}, {false, true, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}}, {{false, false, false, true, true, true, false}, {false, false, false, true, false, false, false}, {false, false, true, false, false, false, false}, {false, true, true, true, false, false, false}, {false, false, true, true, true, true, false}, {false, false, false, true, true, true, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, true, false}}, {{false, false, false, false, true, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, true, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, true, true, false, false}, {false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, true, false, false, false, false, false}, {false, false, true, true, false, false, false}, {false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg8", typeof(Texture2D)) as Texture2D;
			}
			if(level == 13)
			{
				//phonograph
				levelArray = new bool[7,9,7] {{{false, false, false, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}, {{false, false, true, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}}, {{false, false, true, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, true, false}, {false, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}}, {{false, true, true, true, true, false, false}, {false, false, true, true, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, true, false}, {true, true, false, false, true, false, false}, {false, true, true, true, false, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}}, {{false, true, true, true, true, false, false}, {false, true, true, true, true, false, false}, {true, true, false, false, false, false, true}, {false, false, true, false, false, true, false}, {true, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, true, false, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}}, {{true, true, true, true, true, false, false}, {true, true, true, true, true, false, false}, {true, false, false, false, false, false, false}, {true, false, false, false, false, false, true}, {true, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, true, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}}, {{false, true, true, true, true, false, false}, {false, false, true, true, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, true}, {false, false, false, false, false, false, false}, {false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg4", typeof(Texture2D)) as Texture2D;
			}
			if(level == 14)
			{
				//oasis
				levelArray = new bool[7,7,9] {{{true, true, true, true, true, true, true, true, true}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, true, true, true, true, true, true, true, true}, {false, false, false, false, false, true, true, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, true}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, false, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, true}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, true, false, false, true, false, false, true}, {false, false, false, true, true, true, true, true, false}}, {{true, true, true, true, true, true, true, true, true}, {false, false, false, false, true, true, true, true, true}, {false, false, false, false, false, false, false, true, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, true, false, false, false}}, {{true, true, true, true, true, true, true, true, true}, {true, true, true, false, false, true, true, true, true}, {false, true, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}}, {{false, true, true, true, true, true, true, true, false}, {false, true, true, true, true, true, true, false, false}, {false, true, true, false, true, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, true, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg6", typeof(Texture2D)) as Texture2D;
			}
			if(level == 15)
			{
				//gorilla
				levelArray = new bool[9,9,9] {{{false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, true, true, false}}, {{true, true, true, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {false, true, true, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, false, false, true, false, false}, {false, false, false, false, false, false, true, true, false}}, {{true, true, true, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {false, true, true, false, false, false, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true, false}, {false, false, false, true, true, true, true, false, false}, {false, false, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, true, false, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true, false}, {false, false, false, true, true, true, true, false, false}, {false, false, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {true, true, true, true, false, false, false, false, false}, {false, true, true, true, true, true, false, false, false}, {false, false, true, true, true, true, true, true, false}, {false, false, true, true, true, true, true, true, false}, {false, false, false, true, true, true, true, false, false}, {false, false, false, false, true, true, true, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, false, false, false, false, false, false}, {true, true, false, false, false, false, false, false, false}, {false, true, true, false, false, false, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, true, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{true, true, true, false, false, false, false, true, true}, {true, true, false, false, false, false, false, false, true}, {false, true, true, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, true, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}, {{false, false, false, false, false, false, false, true, true}, {false, false, false, false, false, false, false, false, true}, {false, false, false, false, false, false, true, true, false}, {false, false, false, false, false, true, true, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, true, true, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}, {false, false, false, false, false, false, false, false, false}}};
				backgroundImage.texture = Resources.Load("Textures/bg12", typeof(Texture2D)) as Texture2D;
			}
		}
		if(stage == 4)
		{
			this.level = level;

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

				SpawnCubes ();

				file = new System.IO.StreamReader(Application.persistentDataPath + "/custom" + level + "animation.txt");

				animations.Clear();

				int animCount = int.Parse(file.ReadLine());

				for(int i = 0; i < animCount; i++)
				{
					levelAnimation animation = new levelAnimation();


					int cubeCount = int.Parse(file.ReadLine());

					for(int j = 0; j < cubeCount; j++)
					{
						int x = int.Parse(file.ReadLine());
						int y = int.Parse(file.ReadLine());
						int z = int.Parse(file.ReadLine());

						animation.cubes.Add(grid[x,y,z]);
					}

					animation.origin = new Vector3(float.Parse(file.ReadLine()), float.Parse(file.ReadLine()), float.Parse(file.ReadLine()));

					animation.translation = new Vector3(float.Parse(file.ReadLine()), float.Parse(file.ReadLine()), float.Parse(file.ReadLine()));

					animation.angleRight = float.Parse(file.ReadLine());
					animation.angleForward = float.Parse(file.ReadLine());
					animation.angleUp = float.Parse(file.ReadLine());

					animations.Add(animation);
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
			try
			{
				File.Delete(Application.persistentDataPath + "/custom" + level + ".txt");
				File.Delete(Application.persistentDataPath + "/custom" + level + "color.txt");
				File.Delete(Application.persistentDataPath + "/custom" + level + "animation.txt");

				for(uint i = level + 1; i < customStageCount + 1; i++)
				{
					File.Move(Application.persistentDataPath + "/custom" + i + ".txt",Application.persistentDataPath + "/custom" + (i - 1) + ".txt");
				}
			}
			catch
			{

			}

			stage = 4;

			customStageCount--;

			buttonLevelBack();
			buttonStage4();

			return;
		}

		this.level = level;

		if(stage != 4)
			SpawnCubes ();
	}

	void startEndAnimation()
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

		uint tempLevel = (stage - 1) * 15 + level;

		if(tempLevel == 1)
		{
			GameObject[] cubes = new GameObject[6];

			for(int i = 0; i < 6; i++)
			{
				cubes[i] = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cubes[i].transform.parent = endAnimationParent.transform;
				cubes[i].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

				cubes[i].renderer.materials = new Material[1];
				cubes[i].renderer.material = new Material(Shader.Find ("Diffuse"));
				cubes[i].renderer.material.color = new Color(0.2f, 0.2f, 0.2f);

			}

			cubes[0].transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
			cubes[0].transform.position = new Vector3(-1.3f, 0.1f, 20);

			cubes[1].transform.position = new Vector3(1.7f, -0.22f, 20);
			cubes[1].transform.rotation = Quaternion.AngleAxis(340, Vector3.forward);

			cubes[2].transform.position = new Vector3(-1.7f, 0.16f, 20);
			cubes[2].transform.rotation = Quaternion.AngleAxis(25, Vector3.forward);

			cubes[3].transform.position = new Vector3(-1.5f, -0.2f, 20);
			cubes[3].transform.rotation = Quaternion.AngleAxis(325, Vector3.forward);

			cubes[4].transform.localScale = new Vector3(0.6f, 0.3f, 0.3f);
			cubes[4].transform.position = new Vector3(1.5f, 0.1f, 20);
			cubes[4].transform.rotation = Quaternion.AngleAxis(28, Vector3.forward);

			cubes[5].transform.localScale = new Vector3(0.3f, 0.3f, 0.15f);
			cubes[5].transform.position = new Vector3(1.6f, 0, 20);
			cubes[5].transform.rotation = Quaternion.AngleAxis(325, Vector3.forward);

			
			return;
		}
		
		if(tempLevel == 2)
		{
			
			
			return;
		}
		
		if(tempLevel == 3)
		{
			
			
			return;
		}
		
		if(tempLevel == 4)
		{
			
			
			return;
		}
		
		if(tempLevel == 5)
		{
			
			
			return;
		}
		
		if(tempLevel == 6)
		{
			
			
			return;
		}
		
		if(tempLevel == 7)
		{
			
			
			return;
		}
		
		if(tempLevel == 8)
		{
			endRandom = new int[10];
			
			return;
		}
		
		if(tempLevel == 9)
		{
			
			
			return;
		}
		
		if(tempLevel == 10)
		{
			
			
			return;
		}
		
		if(tempLevel == 11)
		{
			GameObject[] cubes = new GameObject[6];
			
			for(int i = 0; i < 6; i++)
			{
				cubes[i] = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cubes[i].transform.parent = endAnimationParent.transform;				
				cubes[i].renderer.materials = new Material[1];
				cubes[i].renderer.material = new Material(Shader.Find ("Diffuse"));
				cubes[i].renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
				
			}

			cubes[0].transform.position = new Vector3(0, 20, -1.5f);

			cubes[1].transform.position = new Vector3(0, 20, -0.5f);

			cubes[2].transform.localScale = new Vector3(1,2,2);
			cubes[2].transform.position = new Vector3(0, 20, 1f);
			
			cubes[3].transform.position = new Vector3(0, 20, -0.5f);

			cubes[4].transform.position = new Vector3(0, 20, -1.5f);

			cubes[5].transform.position = new Vector3(0, 20, -0.5f);

			
			return;
		}
		
		if(tempLevel == 12)
		{
			for(int x = 1; x < 4; x++)
			{
				for(int z = 1; z < 6; z++)
				{
					grid[x,1,z].cube.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
					grid[x,1,z].cube.renderer.material.color = new Color(grid[1,1,1].color.r, grid[1,1,1].color.g, grid[1,1,1].color.b, 0.4f);
				}
			}
			
			return;
		}
		
		if(tempLevel == 13)
		{
			GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
			cube.transform.parent = endAnimationParent.transform;	
			cube.transform.position = new Vector3(-2,20,1);
			cube.transform.localScale = new Vector3(0.6f, 1, 0.6f);
			cube.renderer.materials = new Material[1];
			cube.renderer.material = new Material(Shader.Find ("Diffuse"));
			cube.renderer.material.color = new Color(0.4f, 0.4f, 0.4f);
			
			
			return;
		}
		
		if(tempLevel == 14)
		{
			
			
			return;
		}
		
		if(tempLevel == 15)
		{
			
			
			return;
		}
		
		if(tempLevel == 16)
		{
			
			
			return;
		}
		
		if(tempLevel == 17)
		{
			
			
			return;
		}
		
		if(tempLevel == 18)
		{
			
			
			return;
		}
		
		if(tempLevel == 19)
		{
			
			
			return;
		}
		
		if(tempLevel == 20)
		{
			
			
			return;
		}
		
		if(tempLevel == 21)
		{
			
			
			return;
		}
		
		if(tempLevel == 22)
		{
			
			
			return;
		}
		
		if(tempLevel == 23)
		{
			
			
			return;
		}
		
		if(tempLevel == 24)
		{
			
			
			return;
		}
		
		if(tempLevel == 25)
		{
			
			
			return;
		}
		
		if(tempLevel == 26)
		{
			
			
			return;
		}
		
		if(tempLevel == 27)
		{
			
			
			return;
		}
		
		if(tempLevel == 28)
		{
			
			
			return;
		}
		
		if(tempLevel == 29)
		{
			
			
			return;
		}
		
		if(tempLevel == 30)
		{
			
			
			return;
		}
		
		if(tempLevel == 31)
		{
			grid[0,3,2].cube.AddComponent<Rigidbody>();
			grid[0,3,2].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[0,3,2].cube.rigidbody.AddForce (new Vector3(Random.Range (-100f, -50f),Random.Range (-200f, -100f),Random.Range (-100f, -50f)));
			
			grid[1,5,4].cube.AddComponent<Rigidbody>();
			grid[1,5,4].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[1,5,4].cube.rigidbody.AddForce (new Vector3(Random.Range (-100f, -50f),Random.Range (-100f, -50f),Random.Range (-100f, -50f)));
			
			grid[2,6,5].cube.AddComponent<Rigidbody>();
			grid[2,6,5].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[2,6,5].cube.rigidbody.AddForce (new Vector3(Random.Range (-100f, -50f),0,Random.Range (-100f, -50f)));
			
			grid[3,5,6].cube.AddComponent<Rigidbody>();
			grid[3,5,6].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[3,5,6].cube.rigidbody.AddForce (new Vector3(Random.Range (-160f, -80f),250f,Random.Range (-50f, 50f)));
			
			grid[4,4,6].cube.AddComponent<Rigidbody>();
			grid[4,4,6].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[4,4,6].cube.rigidbody.AddForce (new Vector3(Random.Range (200f, 150f),500f,Random.Range (-50f, 50f)));
			
			grid[5,5,5].cube.AddComponent<Rigidbody>();
			grid[5,5,5].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[5,5,5].cube.rigidbody.AddForce (new Vector3(Random.Range (100f, 50f),250f,Random.Range (-100f, -50f)));
			
			grid[6,6,6].cube.AddComponent<Rigidbody>();
			grid[6,6,6].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[6,6,6].cube.rigidbody.AddForce (new Vector3(Random.Range (100f, 50f),0,Random.Range (-50f, 50f)));
			
			grid[7,5,7].cube.AddComponent<Rigidbody>();
			grid[7,5,7].cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
			grid[7,5,7].cube.rigidbody.AddForce (new Vector3(Random.Range (100f, 50f),Random.Range (-100f, -50f),Random.Range (0f, 50f)));
			
			return;
		}
		
		if(tempLevel == 32)
		{
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.position = grid[0,0,5].cube.transform.position;
				cube.renderer.materials = grid[0,0,5].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[0,1,4].cube.transform.position;
				cube.renderer.materials = grid[0,1,4].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[0,2,5].cube.transform.position;
				cube.renderer.materials = grid[0,2,5].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[0,0,1].cube.transform.position;
				cube.renderer.materials = grid[0,0,1].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[0,1,2].cube.transform.position;
				cube.renderer.materials = grid[0,1,2].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[0,2,1].cube.transform.position;
				cube.renderer.materials = grid[0,2,1].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[6,0,5].cube.transform.position;
				cube.renderer.materials = grid[6,0,5].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[6,1,4].cube.transform.position;
				cube.renderer.materials = grid[6,1,4].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[6,2,5].cube.transform.position;
				cube.renderer.materials = grid[6,2,5].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[6,0,1].cube.transform.position;
				cube.renderer.materials = grid[6,0,1].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[6,1,2].cube.transform.position;
				cube.renderer.materials = grid[6,1,2].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;	
				cube.transform.position = grid[6,2,1].cube.transform.position;
				cube.renderer.materials = grid[6,2,1].cube.renderer.materials;
				cube.transform.localScale = cube.transform.localScale * 0.98f;
			}

			grid[0,1,6].cube.transform.localScale = grid[0,1,6].cube.transform.localScale * 0.98f;
			grid[0,1,0].cube.transform.localScale = grid[0,1,0].cube.transform.localScale * 0.98f;
			grid[6,1,6].cube.transform.localScale = grid[6,1,6].cube.transform.localScale * 0.98f;
			grid[6,1,0].cube.transform.localScale = grid[6,1,0].cube.transform.localScale * 0.98f;
			
			return;
		}
		
		if(tempLevel == 33)
		{
			
			
			return;
		}
		
		if(tempLevel == 34)
		{
			
			
			return;
		}
		
		if(tempLevel == 35)
		{
			
			
			return;
		}
		
		if(tempLevel == 36)
		{
			
			
			return;
		}
		
		if(tempLevel == 37)
		{
			grid[0,7,6].cube.AddComponent<Rigidbody>();
			grid[0,7,6].cube.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),-200f,Random.Range (-100f, 100f)));
			
			grid[1,6,0].cube.AddComponent<Rigidbody>();
			grid[1,6,0].cube.rigidbody.AddForce (new Vector3(-100f,-200f,-100f));
			
			grid[6,1,0].cube.AddComponent<Rigidbody>();
			grid[6,1,0].cube.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),-200f,Random.Range (-100f, 100f)));
			
			return;
		}
		
		if(tempLevel == 38)
		{
			
			
			return;
		}
		
		if(tempLevel == 39)
		{
			
			
			return;
		}
		
		if(tempLevel == 40)
		{
			
			
			return;
		}
		
		if(tempLevel == 41)
		{
			
			
			return;
		}
		
		if(tempLevel == 42)
		{
			
			
			return;
		}
		
		if(tempLevel == 43)
		{
			
			
			return;
		}
		
		if(tempLevel == 44)
		{
			
			
			return;
		}
		
		if(tempLevel == 45)
		{
			
			
			return;
		}
	}
	
	void endAnimationUpdate()
	{
		int finishTimer = this.finishTimer % 360;

		if(this.finishTimer % 360 == 0)
			Debug.Log("360");

		foreach(Cube cube in grid)
		{
			if(cube.cube == null)
				continue;
			
			for(int i= 1; i< cube.cube.renderer.materials.Length; i++)
			{
				Color tempColor = cube.cube.renderer.materials[i].color;
				tempColor.a -= 0.01f;
				cube.cube.renderer.materials[i].color = tempColor;
				
				
			}
			
			if((cube.cube.renderer.materials.Length > 1) && (cube.cube.renderer.materials[1].color.a < 0.01f))
			{
				Material tempMat = cube.cube.renderer.materials[0];
				cube.cube.renderer.materials = new Material[1];
				cube.cube.renderer.material = tempMat;
			}

			//try to rotate camera around object
		}
		
		uint tempLevel = (stage - 1) * 15 + level;

		GameObject[] cubes = new GameObject[endAnimationParent.transform.childCount];

		for(int i = 0; i < cubes.Length; i++)
		{
			cubes[i] = endAnimationParent.transform.GetChild(i).gameObject;
		}

		if(stage == 4)
		{
			foreach(levelAnimation animation in animations)
			{
				foreach(Cube cube in animation.cubes)
				{
					float modif = 1.0f;
					
					if(this.finishTimer % 240 > 120)
						modif = -1.0f;
					
					cube.cube.transform.RotateAround(animation.origin, Vector3.forward, modif * animation.angleForward / 120.0f);
					cube.cube.transform.RotateAround(animation.origin, Vector3.right, modif * animation.angleRight / 120.0f);
					cube.cube.transform.RotateAround(animation.origin, Vector3.up, modif * animation.angleUp / 120.0f);
					
					
					cube.cube.transform.Translate(modif * animation.translation / 120.0f);
				}
			}
			
			return;
		}

		if(tempLevel == 1)
		{
			if(cubes[0].transform.position.z > 2.15f)
			{
				cubes[0].transform.Translate(0,0,-0.6f);
			}

			if(finishTimer > 40)
			{
				if(cubes[1].transform.position.z > 2.15f)
				{
					cubes[1].transform.Translate(0,0,-0.5f);
				}
			}

			if(finishTimer > 90)
			{
				if(cubes[2].transform.position.z > 2.15f)
				{
					cubes[2].transform.Translate(0,0,-0.5f);
				}
			}

			if(finishTimer > 180)
			{
				if(cubes[3].transform.position.z > 2.15f)
				{
					cubes[3].transform.Translate(0,0,-0.6f);
				}
			}

			if(finishTimer > 100)
			{
				if(cubes[4].transform.position.z > 2.15f)
				{
					cubes[4].transform.Translate(0,0,-0.6f);
				}
			}

			if(finishTimer > 280)
			{
				if(cubes[5].transform.position.z > 2.22f)
				{
					cubes[5].transform.Translate(0,0,-0.6f);
				}
				else
				{
					cubes[5].transform.position = new Vector3(cubes[5].transform.position.x, cubes[5].transform.position.y, 2.22f);
				}
			}
			
			return;
		}

		if(tempLevel == 2)
		{
			Vector3 tempPoint = new Vector3(0, -0.5f, 0.5f);

			float tempAngle = 0.8f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI/2);

			grid[0,2,4].cube.transform.RotateAround(tempPoint,Vector3.forward, tempAngle);
			grid[0,2,5].cube.transform.RotateAround(tempPoint,Vector3.forward, tempAngle);

			grid[2,2,4].cube.transform.RotateAround(tempPoint,Vector3.back, tempAngle);
			grid[2,2,5].cube.transform.RotateAround(tempPoint,Vector3.back, tempAngle);


			tempPoint = new Vector3(0, 0, -0.5f);
			tempAngle = 0.4f * Mathf.Sin(finishTimer / 15.0f - Mathf.PI/2);

			grid[1,3,3].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,2,3].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,2,2].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,1,2].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,1,1].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,1,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,0,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);

			tempPoint = new Vector3(0, 0, -1.5f);

			grid[1,2,2].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,1,2].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,1,1].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,1,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,0,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);

			tempPoint = new Vector3(0, 0, -2.5f);

			grid[1,1,1].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,1,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,0,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);

			tempPoint = new Vector3(0, 0, -3.5f);

			grid[1,1,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,0,0].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			
			return;
		}

		if(tempLevel == 3)
		{
			
			
			return;
		}

		if(tempLevel == 4)
		{
			
			
			return;
		}

		if(tempLevel == 5)
		{
			cubesParent.transform.Rotate((360 - finishTimer) / 50.0f,0,0);
			cubesParent.transform.position = new Vector3(0, ((360 - finishTimer)*(360 - finishTimer) / 21600.0f) * Mathf.Abs(Mathf.Sin(finishTimer / 20.0f)), 0);

			return;
		}

		if(tempLevel == 6)
		{
			cubesParent.transform.rotation = Quaternion.AngleAxis(4 * Mathf.Sin(finishTimer / 15.0f), Vector3.forward);

			if(finishTimer % 30 == 10)
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
				cube.transform.position = grid[2,0,4].cube.transform.position;
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube.renderer.material.color = new Color(1, 1, 1, 0.7f);

				GameObject cube2 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube2.transform.parent = endAnimationParent.transform;
				cube2.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
				cube2.transform.position = grid[5,0,4].cube.transform.position;	
				cube2.renderer.materials = new Material[1];
				cube2.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube2.renderer.material.color = new Color(1, 1, 1, 0.7f);
			}

			for(int i=0; i < endAnimationParent.transform.childCount; i++)
			{
				Transform cube = endAnimationParent.transform.GetChild(i);

				cube.Translate(0,0,-0.5f);

				cube.gameObject.renderer.material.color = new Color(1,1,1, cube.gameObject.renderer.material.color.a - 0.002f);

				if(cube.gameObject.renderer.material.color.a < 0.01f)
				{
					Destroy(cube.gameObject);
				}
			}
			
			
			return;
		}

		if(tempLevel == 7)
		{
			if(finishTimer % 20 == 5)
			{

				grid[1,2,1].cube.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
				grid[1,3,1].cube.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
				grid[2,2,1].cube.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
				grid[2,3,1].cube.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
				grid[3,2,1].cube.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
				grid[3,3,1].cube.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
			}
			
			return;
		}

		if(tempLevel == 8)
		{

			if(finishTimer % 50 == 1)
			{
				int randRange = Random.Range(0,10);

				while(endRandom[randRange] != 0)
				{
					randRange = Random.Range(0,10);

				}
				endRandom[randRange] = 1;
			}

			for(int i = 0; i < endRandom.Length; i++)
			{
				if(endRandom[i] == 0)
					continue;

				Vector3 tempPoint = new Vector3(-3 + i, -0.5f, -1);

				if(i < 7)
				{
					float tempAngle = 0.3f * Mathf.Sin((endRandom[i] - 1)*Mathf.Deg2Rad);

					grid[i,0,1].cube.transform.RotateAround(tempPoint,Vector3.right,tempAngle);
					grid[i,0,2].cube.transform.RotateAround(tempPoint,Vector3.right,tempAngle);
					grid[i,0,3].cube.transform.RotateAround(tempPoint,Vector3.right,tempAngle);
				}
				else
				{
					float tempAngle = 0.4f * Mathf.Sin((endRandom[i] - 1)*Mathf.Deg2Rad);

					grid[1 + 2*(i - 7),1,1].cube.transform.RotateAround(tempPoint,Vector3.right,tempAngle);
					grid[1 + 2*(i - 7),1,2].cube.transform.RotateAround(tempPoint,Vector3.right,tempAngle);
				}

				endRandom[i] += 3;
				
				if(endRandom[i] >= 360)
				{
					endRandom[i] = 0;
				}
			}


			//grid[0,0,1]
			//grid[0,0,2]
			//grid[0,0,3]


			
			
			return;
		}

		if(tempLevel == 9)
		{
			backgroundImage.color = new Color(0.5f - finishTimer*0.0011f, 0.5f - finishTimer*0.0011f, 0.5f - finishTimer*0.0011f);

			if(finishTimer < 350)
				Camera.main.transform.GetChild(0).light.intensity = 0.3f - (finishTimer*0.00085f);

			if(finishTimer == 200)
			{
				grid[2,1,1].cube.renderer.material = new Material(Shader.Find ("Diffuse"));
				grid[2,1,1].cube.renderer.material.color = new Color(0.5f, 0.5f, 0);

				grid[2,1,3].cube.renderer.material = new Material(Shader.Find ("Diffuse"));
				grid[2,1,3].cube.renderer.material.color = new Color(0.5f, 0.5f, 0);

				GameObject light1 = new GameObject();
				light1.transform.parent = endAnimationParent.transform;
				Light lightComp1 = light1.AddComponent<Light>();
				lightComp1.type = LightType.Point;
				lightComp1.intensity = 0.9f;
				lightComp1.range = 4;

				light1.transform.position = new Vector3(-1.0f, -1.0f, -1.0f);

				GameObject light2 = new GameObject();
				light2.transform.parent = endAnimationParent.transform;
				Light lightComp2 = light2.AddComponent<Light>();
				lightComp2 = lightComp1;
				light2.transform.position = new Vector3(0.0f, -1.0f, -2.0f);

				GameObject light3 = new GameObject();
				light3.transform.parent = endAnimationParent.transform;
				Light lightComp3 = light3.AddComponent<Light>();
				lightComp3 = lightComp2;
				light3.transform.position = new Vector3(-1.0f, -1.0f, 1.0f);

				GameObject light4 = new GameObject();
				light4.transform.parent = endAnimationParent.transform;
				Light lightComp4 = light4.AddComponent<Light>();
				lightComp4 = lightComp3;
				light4.transform.position = new Vector3(1.0f, -1.0f, 1.0f);

				GameObject light5 = new GameObject();
				light5.transform.parent = endAnimationParent.transform;
				Light lightComp5 = light5.AddComponent<Light>();
				lightComp5 = lightComp4;
				light5.transform.position = new Vector3(0.0f, -1.0f, 2.0f);

				lightComp1 = lightComp5;
			}

			if(finishTimer > 358)
			{
				Camera.main.transform.GetChild(0).light.intensity = 0.3f;

				for(int i = 0; i < endAnimationParent.transform.childCount; i++)
				{
					Destroy(endAnimationParent.transform.GetChild(i).gameObject);
				}
			}
			
			return;
		}

		if(tempLevel == 10)
		{
			if(finishTimer % 120 < 80)
				cubesParent.transform.rotation = Quaternion.AngleAxis(2 * Mathf.Sin(finishTimer), new Vector3(1,1,1));
			//cubesParent.transform.rotation = Quaternion.AngleAxis(Mathf.Sin(finishTimer), Vector3.left);
			//cubesParent.transform.rotation = Quaternion.AngleAxis(Mathf.Sin(finishTimer), Vector3.up);
			
			return;
		}

		if(tempLevel == 11)
		{
			if(cubes[0].transform.position.y > -1.5f)
			{
				cubes[0].transform.Translate(0,-0.5f,0);
			}
			
			if(finishTimer > 40)
			{
				if(cubes[1].transform.position.y > -1.5f)
				{
					cubes[1].transform.Translate(0,-0.5f,0);
				}
			}
			
			if(finishTimer > 90)
			{
				if(cubes[2].transform.position.y > -1f)
				{
					cubes[2].transform.Translate(0,-1,0);
				}
			}
			
			if(finishTimer > 150)
			{
				if(cubes[3].transform.position.y > -0.5f)
				{
					cubes[3].transform.Translate(0,-0.5f,0);
				}
			}
			
			if(finishTimer > 180)
			{
				if(cubes[4].transform.position.y > -0.5f)
				{
					cubes[4].transform.Translate(0,-0.5f,0);
				}
			}
			
			if(finishTimer > 240)
			{
				if(cubes[5].transform.position.y > 0.5f)
				{
					cubes[5].transform.Translate(0,-0.5f,0);
				}
			}
			
			return;
		}

		if(tempLevel == 12)
		{
			if(finishTimer == 100)
			{

				for(int x = 1; x < 4; x++)
				{
					for(int z = 1; z < 6; z++)
					{
						Destroy(grid[x,1,z].cube);
					}
				}

				Color tempColor = grid[1,1,1].color;
				tempColor.a = 0.5f;

				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.position = new Vector3(0,-1,0);
				cube.transform.localScale = new Vector3(3,1,5);
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube.renderer.material.color = tempColor;

				GameObject cube1 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube1.transform.parent = endAnimationParent.transform;
				cube1.transform.position = new Vector3(0,2,0);
				cube1.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
				cube1.renderer.materials = new Material[1];
				cube1.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube1.renderer.material.color = tempColor;
			}

			if((finishTimer > 140) && (finishTimer < 160))
			{
				Transform water = endAnimationParent.transform.GetChild(1);

				water.Translate(0,-0.08f,0);
				water.localScale = new Vector3(0.8f, water.localScale.y + 0.08f, 0.8f);

			}

			if((finishTimer > 130) && (finishTimer < 150))
			{
				grid[4,3,2].cube.transform.Rotate(0,0,3);
			}
			
			
			return;
		}

		if(tempLevel == 13)
		{

			Vector3 tempPoint = new Vector3(0, 2, 0);
			
			float tempAngle;

			if(finishTimer < 124)
			{
				tempAngle = 0.1f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI/2);
			}
			else
			{
				float tempNum = (280 - finishTimer) / 500.0f;
				if(tempNum < 0)
					tempNum = 0;

				tempAngle = tempNum * Mathf.Sin(finishTimer / 20.0f - Mathf.PI * 3 / 2);
			}

			for (int x= 0; x< 3; x++)
			{
				for (int y= 0; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, Vector3.forward, tempAngle);
					}
				}
			}

			for (int x= 6; x< 9; x++)
			{
				for (int y= 0; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, Vector3.forward, tempAngle);
					}
				}
			}

			grid[3,5,1].cube.transform.RotateAround(tempPoint, Vector3.forward, tempAngle);
			grid[4,5,1].cube.transform.RotateAround(tempPoint, Vector3.forward, tempAngle);
			grid[5,5,1].cube.transform.RotateAround(tempPoint, Vector3.forward, tempAngle);
			endAnimationParent.transform.RotateAround(tempPoint, Vector3.forward, tempAngle);

			if((finishTimer >= 40) && (finishTimer < 124))
			{
				Transform weight = endAnimationParent.transform.GetChild(0);

				weight.Translate (0,-0.25f,0);
			}

			return;
		}

		if(tempLevel == 14)
		{
			cubesParent.transform.rotation = Quaternion.AngleAxis(4 * Mathf.Sin(finishTimer / 15.0f), Vector3.forward);

			if(finishTimer % 10 == 8)
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.localScale = new Vector3(0.8f, 0.8f, 0.6f);
				cube.transform.position = new Vector3(-1,-1,-3);
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube.renderer.material.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);

				GameObject cube1 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube1.transform.parent = endAnimationParent.transform;
				cube1.transform.localScale = new Vector3(0.8f, 0.8f, 0.4f);
				cube1.transform.position = new Vector3(-1,-1,-3.5f);	
				cube1.renderer.materials = new Material[1];
				cube1.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube1.renderer.material.color = new Color(0.8f, 0.5f, 0.2f, 0.7f);

				GameObject cube2 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube2.transform.parent = endAnimationParent.transform;
				cube2.transform.localScale = new Vector3(0.8f, 0.8f, 0.4f);
				cube2.transform.position = new Vector3(-1,-1,-3.9f);	
				cube2.renderer.materials = new Material[1];
				cube2.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube2.renderer.material.color = new Color(0.8f, 0.8f, 0.2f, 0.7f);

				GameObject cube3 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube3.transform.parent = endAnimationParent.transform;
				cube3.transform.localScale = new Vector3(0.8f, 0.8f, 0.6f);
				cube3.transform.position = new Vector3(0,0,-3);
				cube3.renderer.materials = new Material[1];
				cube3.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube3.renderer.material.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
					
				GameObject cube4 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube4.transform.parent = endAnimationParent.transform;
				cube4.transform.localScale = new Vector3(0.8f, 0.8f, 0.4f);
				cube4.transform.position = new Vector3(0,0,-3.5f);	
				cube4.renderer.materials = new Material[1];
				cube4.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube4.renderer.material.color = new Color(0.8f, 0.5f, 0.2f, 0.7f);
					
				GameObject cube5 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube5.transform.parent = endAnimationParent.transform;
				cube5.transform.localScale = new Vector3(0.8f, 0.8f, 0.4f);
				cube5.transform.position = new Vector3(0,0,-3.9f);	
				cube5.renderer.materials = new Material[1];
				cube5.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube5.renderer.material.color = new Color(0.8f, 0.8f, 0.2f, 0.7f);

				GameObject cube6 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube6.transform.parent = endAnimationParent.transform;
				cube6.transform.localScale = new Vector3(0.8f, 0.8f, 0.6f);
				cube6.transform.position = new Vector3(1,-1,-3);
				cube6.renderer.materials = new Material[1];
				cube6.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube6.renderer.material.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
					
				GameObject cube7 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube7.transform.parent = endAnimationParent.transform;
				cube7.transform.localScale = new Vector3(0.8f, 0.8f, 0.4f);
				cube7.transform.position = new Vector3(1,-1,-3.5f);	
				cube7.renderer.materials = new Material[1];
				cube7.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube7.renderer.material.color = new Color(0.8f, 0.5f, 0.2f, 0.7f);
					
				GameObject cube8 = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube8.transform.parent = endAnimationParent.transform;
				cube8.transform.localScale = new Vector3(0.8f, 0.8f, 0.4f);
				cube8.transform.position = new Vector3(1,-1,-3.9f);	
				cube8.renderer.materials = new Material[1];
				cube8.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube8.renderer.material.color = new Color(0.8f, 0.8f, 0.2f, 0.7f);
			}

			for(int i=0; i < endAnimationParent.transform.childCount; i++)
			{
				Transform cube = endAnimationParent.transform.GetChild(i);

				cube.Translate(0,0,-0.6f);

				Color tempColor = cube.gameObject.renderer.material.color;
				tempColor.a -= 0.05f;

				cube.gameObject.renderer.material.color = tempColor;

				if(cube.gameObject.renderer.material.color.a < 0.1f)
				{
					Destroy(cube.gameObject);
				}
			}
			
			
			return;
		}

		if(tempLevel == 15)
		{
			if(finishTimer % 2 == 1)
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.localScale = new Vector3(0.2f, 1, 0.1f);
				cube.transform.position = new Vector3(Random.Range(-7.0f,7.0f), 15,Random.Range(-7.0f,7.0f));
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube.renderer.material.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),0.8f);
			}

			for(int i=0; i < endAnimationParent.transform.childCount; i++)
			{
				Transform cube = endAnimationParent.transform.GetChild(i);
				
				cube.Translate(0,-0.4f,0);
				
				Color tempColor = cube.gameObject.renderer.material.color;
				tempColor.a -= 0.008f;
				
				cube.gameObject.renderer.material.color = tempColor;
				
				if(cube.gameObject.renderer.material.color.a < 0.01f)
				{
					Destroy(cube.gameObject);
				}
			}
			
			
			return;
		}

		if(tempLevel == 16)
		{
			Vector3 tempPoint = new Vector3(0, 0.5f, 0.5f);

			float tempAngle = - 1.1f * Mathf.Sin(finishTimer / 40.0f);
			
			grid[0,4,3].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
			grid[1,4,3].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
			grid[2,4,3].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
			grid[0,5,3].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
			grid[1,5,3].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
			grid[2,5,3].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
			grid[0,5,4].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
			grid[2,5,4].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);

			grid[0,0,0].cube.transform.Translate(0,0,- tempAngle / 100.0f);
			grid[0,0,3].cube.transform.Translate(0,0, tempAngle / 100.0f);
			grid[2,0,1].cube.transform.Translate(0,0, tempAngle / 100.0f);
			grid[2,0,3].cube.transform.Translate(0,0,- tempAngle / 100.0f);
			
			return;
		}

		if(tempLevel == 17)
		{
			Vector3 tempPoint = grid[1,1,6].cube.transform.position;

			grid[1,0,6].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,1,5].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,1,6].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,1,7].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,2,6].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);

			tempPoint = grid[1,1,1].cube.transform.position;

			grid[1,0,1].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,1,0].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,1,1].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,1,2].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);
			grid[1,2,1].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, 5.0f);

			tempPoint = grid[1,4,6].cube.transform.position;
			float tempAngle = - 0.4f * Mathf.Sin(finishTimer / 60.0f - Mathf.PI / 2);

			grid[0,4,6].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,4,6].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[1,4,7].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[2,4,6].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);

			tempAngle = 0.4f * Mathf.Sin(finishTimer / 60.0f);

			cubesParent.transform.rotation = Quaternion.AngleAxis(25 * tempAngle, Vector3.back);
			
			return;
		}

		if(tempLevel == 18)
		{
			Vector3 tempPoint = grid[2,4,6].cube.transform.position;

			grid[0,4,4].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[0,4,8].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[1,4,5].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[1,4,7].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[2,4,6].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[3,4,5].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[3,4,7].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[4,4,4].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);
			grid[4,4,8].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.up, 10.0f);

			tempPoint = grid[1,2,1].cube.transform.position;

			grid[1,1,1].cube.transform.RotateAround(tempPoint, grid[1,2,1].cube.transform.right, 10.0f);
			grid[1,2,0].cube.transform.RotateAround(tempPoint, grid[1,2,1].cube.transform.right, 10.0f);
			grid[1,2,1].cube.transform.RotateAround(tempPoint, grid[1,2,1].cube.transform.right, 10.0f);
			grid[1,2,2].cube.transform.RotateAround(tempPoint, grid[1,2,1].cube.transform.right, 10.0f);
			grid[1,3,1].cube.transform.RotateAround(tempPoint, grid[1,2,1].cube.transform.right, 10.0f);

			cubesParent.transform.rotation = Quaternion.AngleAxis(20 * Mathf.Sin(finishTimer / 50.0f), new Vector3(1.0f,0.3f,0.8f));
			
			return;
		}

		if(tempLevel == 19)
		{
			if(finishTimer % 20 == 10)
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
				cube.transform.position = grid[2,5,7].cube.transform.position;
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube.renderer.material.color = new Color(1, 1, 1, 0.5f);
			}

			for(int i=0; i < endAnimationParent.transform.childCount; i++)
			{
				Transform cube = endAnimationParent.transform.GetChild(i);
				
				cube.Translate(0,0.2f,0);
				cube.localScale = new Vector3(cube.localScale.x + 0.02f, cube.localScale.y + 0.02f, cube.localScale.z + 0.02f);
				
				cube.gameObject.renderer.material.color = new Color(1,1,1, cube.gameObject.renderer.material.color.a - 0.01f);
				
				if(cube.gameObject.renderer.material.color.a < 0.01f)
				{
					Destroy(cube.gameObject);
				}
			}

			Vector3 tempPoint = grid[0,0,0].cube.transform.position + grid[0,0,1].cube.transform.position +
				grid[0,1,0].cube.transform.position + grid[0,1,1].cube.transform.position;
			tempPoint /= 4;
			
			grid[0,0,0].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);
			grid[0,0,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);
			grid[0,1,0].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);
			grid[0,1,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);

			tempPoint = grid[0,0,3].cube.transform.position + grid[0,0,4].cube.transform.position +
				grid[0,1,3].cube.transform.position + grid[0,1,4].cube.transform.position;
			tempPoint /= 4;
			
			grid[0,0,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);
			grid[0,0,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);
			grid[0,1,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);
			grid[0,1,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);

			tempPoint = grid[0,0,6].cube.transform.position + grid[0,0,7].cube.transform.position +
				grid[0,1,6].cube.transform.position + grid[0,1,7].cube.transform.position;
			tempPoint /= 4;

			grid[0,0,6].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);
			grid[0,0,7].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);
			grid[0,1,6].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);
			grid[0,1,7].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);

			tempPoint = grid[4,0,0].cube.transform.position + grid[4,0,1].cube.transform.position +
				grid[4,1,0].cube.transform.position + grid[4,1,1].cube.transform.position;
			tempPoint /= 4;
			
			grid[4,0,0].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);
			grid[4,0,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);
			grid[4,1,0].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);
			grid[4,1,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 10.0f);
			
			tempPoint = grid[4,0,3].cube.transform.position + grid[4,0,4].cube.transform.position +
				grid[4,1,3].cube.transform.position + grid[4,1,4].cube.transform.position;
			tempPoint /= 4;
			
			grid[4,0,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);
			grid[4,0,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);
			grid[4,1,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);
			grid[4,1,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, 10.0f);
			
			tempPoint = grid[4,0,6].cube.transform.position + grid[4,0,7].cube.transform.position +
				grid[4,1,6].cube.transform.position + grid[4,1,7].cube.transform.position;
			tempPoint /= 4;
			
			grid[4,0,6].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);
			grid[4,0,7].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);
			grid[4,1,6].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);
			grid[4,1,7].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, 10.0f);

			cubesParent.transform.rotation = Quaternion.AngleAxis(0.2f * Mathf.Sin(finishTimer / 2.0f), new Vector3(1.0f,0.2f,0.0f));
			
			return;
		}

		if(tempLevel == 20)
		{
			Vector3 tempPoint = new Vector3(0.0f, 1.0f, -2.5f);
			
			float tempAngle = 0.1f * Mathf.Sin(finishTimer / 40.0f - Mathf.PI / 2);
			
			grid[1,6,0].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[1,6,1].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[1,6,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[1,7,1].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[1,7,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[2,6,0].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[2,6,1].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[2,6,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[2,7,1].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);
			grid[2,7,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.2f,0.0f), tempAngle);

			tempAngle = - 0.02f * Mathf.Sin(finishTimer / 20.0f);

			grid[1,1,6].cube.transform.Translate(0,tempAngle,0);
			grid[1,2,6].cube.transform.Translate(0,tempAngle,0);


			tempPoint = new Vector3(0.0f, 0.0f, -2.5f);			
			tempAngle = 0.1f * Mathf.Sin(finishTimer / 40.0f);
			
			grid[0,4,1].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[0,4,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[0,4,3].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[1,4,3].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[2,4,3].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[2,3,3].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[2,5,4].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[2,6,4].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[2,7,5].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[2,8,6].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[3,4,1].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[3,4,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[3,4,3].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);


			tempPoint = new Vector3(-0.5f, -2.0f, -1.5f);			
			tempAngle = 0.1f * Mathf.Sin(finishTimer / 80.0f - Mathf.PI / 2);
			
			grid[1,1,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			grid[1,2,2].cube.transform.RotateAround(tempPoint, new Vector3(1.0f,0.1f,0.0f), tempAngle);
			
			
			return;
		}

		if(tempLevel == 21)
		{
			Vector3 tempPoint = grid[2,5,4].cube.transform.position;
			float tempAngle = 0.55f * Mathf.Sin(finishTimer / 80.0f);
			
			grid[2,5,4].cube.transform.RotateAround(tempPoint, Vector3.down, tempAngle);
			grid[2,6,4].cube.transform.RotateAround(tempPoint, Vector3.down, tempAngle);
			grid[2,7,4].cube.transform.RotateAround(tempPoint, Vector3.down, tempAngle);
			grid[2,8,4].cube.transform.RotateAround(tempPoint, Vector3.down, tempAngle);
			grid[3,8,4].cube.transform.RotateAround(tempPoint, Vector3.down, tempAngle);


			tempPoint = new Vector3(-0.5f, 2.5f, 0.5f);
			tempAngle = 0.5f * Mathf.Sin(finishTimer / 30.0f);
			
			grid[1,5,2].cube.transform.RotateAround(tempPoint, Vector3.back, tempAngle);
			grid[1,5,3].cube.transform.RotateAround(tempPoint, Vector3.back, tempAngle);
			grid[1,6,2].cube.transform.RotateAround(tempPoint, Vector3.back, tempAngle);
			grid[1,6,3].cube.transform.RotateAround(tempPoint, Vector3.back, tempAngle);

			tempPoint = new Vector3(0.5f, 2.5f, 0.5f);
			
			grid[3,5,2].cube.transform.RotateAround(tempPoint, Vector3.back, -tempAngle);
			grid[3,5,3].cube.transform.RotateAround(tempPoint, Vector3.back, -tempAngle);
			grid[3,6,2].cube.transform.RotateAround(tempPoint, Vector3.back, -tempAngle);
			grid[3,6,3].cube.transform.RotateAround(tempPoint, Vector3.back, -tempAngle);

			tempPoint = grid[2,4,1].cube.transform.position;
			tempAngle = 0.2f * Mathf.Sin(finishTimer / 60.0f - Mathf.PI / 2);
			
			grid[2,3,1].cube.transform.RotateAround(tempPoint, Vector3.back, -tempAngle);
			grid[2,4,1].cube.transform.RotateAround(tempPoint, Vector3.back, -tempAngle);
			
			
			return;
		}

		if(tempLevel == 22)
		{
			Vector3 tempPoint = grid[1,4,3].cube.transform.position;
			float tempAngle = 0.2f * Mathf.Sin(finishTimer / 60.0f - Mathf.PI / 2);
			
			grid[0,4,3].cube.transform.RotateAround(tempPoint, grid[0,4,3].cube.transform.forward, tempAngle);

			tempPoint = grid[3,4,3].cube.transform.position;
			
			grid[4,4,3].cube.transform.RotateAround(tempPoint, grid[4,4,3].cube.transform.forward, -tempAngle);

			tempPoint = grid[1,3,5].cube.transform.position;
			tempAngle = 0.1f * Mathf.Sin(finishTimer / 20.0f);
			
			grid[1,2,5].cube.transform.RotateAround(tempPoint, grid[1,2,5].cube.transform.forward, -tempAngle);
			grid[1,3,5].cube.transform.RotateAround(tempPoint, grid[1,2,5].cube.transform.forward, -tempAngle);

			tempPoint = grid[3,3,5].cube.transform.position;
			
			grid[3,2,5].cube.transform.RotateAround(tempPoint, grid[3,2,5].cube.transform.forward, tempAngle);
			grid[3,3,5].cube.transform.RotateAround(tempPoint, grid[3,2,5].cube.transform.forward, tempAngle);

			tempPoint = grid[0,2,3].cube.transform.position;
			tempAngle = - 0.5f * Mathf.Sin(finishTimer / 10.0f - Mathf.PI / 2);
			
			grid[0,0,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,1,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,2,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);

			tempPoint = grid[4,2,3].cube.transform.position;
			
			grid[4,0,2].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[4,0,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[4,1,2].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[4,2,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);


			tempPoint = grid[2,2,0].cube.transform.position;
			tempAngle = 1.2f * Mathf.Sin(finishTimer / 10.0f - Mathf.PI / 2);
			
			grid[2,3,0].cube.transform.RotateAround(tempPoint, Vector3.back, tempAngle);
			grid[2,4,0].cube.transform.RotateAround(tempPoint, Vector3.back, tempAngle);

			tempPoint = grid[2,2,1].cube.transform.position;
			tempAngle = 0.04f * Mathf.Sin(finishTimer / 30.0f);

			for (int x= 1; x< 4; x++)
			{
				for (int y= 1; y< levelArray.GetLength(1); y++)
				{
					for (int z= 2; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
					}
					for (int z= 3; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
					}
					for (int z= 4; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, Vector3.left, tempAngle);
					}
				}
			}
			
			
			return;
		}

		if(tempLevel == 23)
		{
			Vector3 tempPoint = grid[4,0,4].cube.transform.position;
			float tempAngle = 0.4f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI / 2);
			Vector3 tempVector = grid[3,0,2].cube.transform.forward + grid[3,0,2].cube.transform.right;
			
			grid[0,0,7].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[1,0,6].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[1,0,7].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,5].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,6].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,7].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[3,0,5].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[3,0,6].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);

			grid[5,0,2].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);
			grid[5,0,3].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);
			grid[6,0,1].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);
			grid[6,0,2].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);
			grid[6,0,3].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);
			grid[7,0,0].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);
			grid[7,0,1].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);
			grid[7,0,2].cube.transform.RotateAround(tempPoint, tempVector, -tempAngle);

			tempPoint = grid[4,0,4].cube.transform.position;
			tempAngle = 0.2f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI / 2);
			tempVector = grid[5,0,5].cube.transform.up;
			
			grid[5,0,5].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[5,0,6].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[5,1,5].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[5,1,6].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[6,0,5].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[6,0,6].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[6,1,5].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[6,1,6].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);
			grid[7,0,7].cube.transform.RotateAround(tempPoint, Vector3.up, tempAngle);

			tempPoint = grid[3,0,3].cube.transform.position;
			tempAngle = 0.1f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI / 2);

			tempVector = - grid[3,0,2].cube.transform.forward + grid[3,0,2].cube.transform.right;

			grid[0,0,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[1,0,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,0].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,1].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,3].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,1,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[3,0,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);

			tempVector = - grid[0,0,2].cube.transform.forward + grid[0,0,2].cube.transform.right;

			grid[0,0,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[1,0,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,0].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,1].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);
			grid[2,0,2].cube.transform.RotateAround(tempPoint, tempVector, tempAngle);

			tempVector = grid[3,0,2].cube.transform.forward + grid[3,0,2].cube.transform.right;
			cubesParent.transform.rotation = Quaternion.AngleAxis(5.0f * Mathf.Sin(finishTimer / 20.0f), tempVector);
			
			return;
		}

		if(tempLevel == 24)
		{
			Vector3 tempPoint = grid[0,2,3].cube.transform.position + grid[0,2,4].cube.transform.position +
				grid[0,3,3].cube.transform.position + grid[0,3,4].cube.transform.position;
			tempPoint /= 4;
			float tempAngle = -1.0f * Mathf.Sin(finishTimer / 15.0f - Mathf.PI / 2);

			grid[0,0,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,1,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,1,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,2,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,2,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,3,3].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);
			grid[0,3,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.right, tempAngle);


			tempPoint = grid[0,1,0].cube.transform.position + grid[0,1,1].cube.transform.position +
				grid[0,2,0].cube.transform.position + grid[0,2,1].cube.transform.position;
			tempPoint /= 4;
			
			grid[0,0,0].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, -tempAngle);
			grid[0,1,0].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, -tempAngle);
			grid[0,1,1].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, -tempAngle);
			grid[0,2,0].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, -tempAngle);
			grid[0,2,1].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, -tempAngle);


			tempPoint = grid[4,2,3].cube.transform.position + grid[4,2,4].cube.transform.position +
				grid[4,3,3].cube.transform.position + grid[4,3,4].cube.transform.position;
			tempPoint /= 4;
			
			grid[4,0,4].cube.transform.RotateAround(tempPoint, grid[4,0,4].cube.transform.right, -tempAngle);
			grid[4,1,3].cube.transform.RotateAround(tempPoint, grid[4,0,4].cube.transform.right, -tempAngle);
			grid[4,1,4].cube.transform.RotateAround(tempPoint, grid[4,0,4].cube.transform.right, -tempAngle);
			grid[4,2,3].cube.transform.RotateAround(tempPoint, grid[4,0,4].cube.transform.right, -tempAngle);
			grid[4,2,4].cube.transform.RotateAround(tempPoint, grid[4,0,4].cube.transform.right, -tempAngle);
			grid[4,3,3].cube.transform.RotateAround(tempPoint, grid[4,0,4].cube.transform.right, -tempAngle);
			grid[4,3,4].cube.transform.RotateAround(tempPoint, grid[4,0,4].cube.transform.right, -tempAngle);

			tempPoint = grid[4,1,0].cube.transform.position + grid[4,1,1].cube.transform.position +
				grid[4,2,0].cube.transform.position + grid[4,2,1].cube.transform.position;
			tempPoint /= 4;
			
			grid[4,0,0].cube.transform.RotateAround(tempPoint, grid[4,0,0].cube.transform.right, tempAngle);
			grid[4,1,0].cube.transform.RotateAround(tempPoint, grid[4,0,0].cube.transform.right, tempAngle);
			grid[4,1,1].cube.transform.RotateAround(tempPoint, grid[4,0,0].cube.transform.right, tempAngle);
			grid[4,2,0].cube.transform.RotateAround(tempPoint, grid[4,0,0].cube.transform.right, tempAngle);
			grid[4,2,1].cube.transform.RotateAround(tempPoint, grid[4,0,0].cube.transform.right, tempAngle);


			tempPoint = grid[2,2,3].cube.transform.position + grid[2,3,3].cube.transform.position;
			tempPoint /= 2;
			tempAngle = 0.05f * Mathf.Sin(finishTimer / 15.0f - Mathf.PI / 2);

			for (int x= 1; x< 4; x++)
			{
				for (int y= 1; y< 5; y++)
				{
					for (int z= 4; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
					for (int z= 5; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
					for (int z= 6; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
					for (int z= 7; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
				}
			}

			Vector3 tempVector = cubesParent.transform.right + cubesParent.transform.forward * 0.8f + cubesParent.transform.up * 0.1f;
			cubesParent.transform.rotation = Quaternion.AngleAxis(0.5f * Mathf.Sin(finishTimer / 5.0f), tempVector);

			
			return;
		}

		if(tempLevel == 25)
		{
			Vector3 tempPoint = new Vector3();
			float tempAngle = 0.2f * Mathf.Sin(finishTimer / 15.0f);

			for (int x= 0; x< levelArray.GetLength(0); x++)
			{
				for (int y= 1; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						if((x == 0) && ((y == 1) || (y == 2)) && (z == 1))
							continue;
						if(((x == 0) || (x == 1)) && ((y == 5) || (y == 6)) && ((z == 1) || (z == 2)))
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.up, tempAngle);

						if (y > 2)
							grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.up, tempAngle);

						if (y > 3)
							grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.up, tempAngle);
					}
				}
			}

			tempAngle = -0.007f * Mathf.Sin(finishTimer / 15.0f - Mathf.PI / 2);
			
			grid[1,1,2].cube.transform.Translate(0, tempAngle, 0);
			grid[1,2,2].cube.transform.Translate(0, tempAngle, 0);
			
			return;
		}

		if(tempLevel == 26)
		{
			Vector3 tempPoint = grid[2,4,3].cube.transform.position;

			for (int x= 1; x< 4; x++)
			{
				for (int y= 1; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< 7; z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, -0.5f);
					}
				}
			}

			if(finishTimer % 30 == 15)
			{
				grid[3,0,0].cube.renderer.material.color = grid[3,0,levelArray.GetLength(2) - 1].cube.renderer.material.color;
				grid[2,0,0].cube.renderer.material.color = grid[2,0,levelArray.GetLength(2) - 1].cube.renderer.material.color;
				grid[1,0,0].cube.renderer.material.color = grid[1,0,levelArray.GetLength(2) - 1].cube.renderer.material.color;

				for (int z= levelArray.GetLength(2) - 1; z > 0; z--)
				{
					grid[3,0,z].cube.renderer.material.color = grid[3,0,z - 1].cube.renderer.material.color;
					grid[2,0,z].cube.renderer.material.color = grid[2,0,z - 1].cube.renderer.material.color;
					grid[1,0,z].cube.renderer.material.color = grid[1,0,z - 1].cube.renderer.material.color;
				}
			}
			return;
		}

		if(tempLevel == 27)
		{
			
			
			return;
		}

		if(tempLevel == 28)
		{
			Vector3 tempPoint = grid[4,2,7].cube.transform.position;

			grid[3,3,7].cube.transform.RotateAround(tempPoint, grid[3,3,7].cube.transform.forward, -20.0f);
			grid[4,2,7].cube.transform.RotateAround(tempPoint, grid[4,2,7].cube.transform.forward, -20.0f);
			grid[5,1,7].cube.transform.RotateAround(tempPoint, grid[5,1,7].cube.transform.forward, -20.0f);

			tempPoint = grid[4,1,0].cube.transform.position * 0.5f + grid[4,1,1].cube.transform.position * 0.5f; 
			float tempAngle = 0.2f * Mathf.Sin(finishTimer / 50.0f - Mathf.PI * 3 / 2);

			grid[2,1,0].cube.transform.RotateAround(tempPoint, grid[2,1,0].cube.transform.right, tempAngle);
			grid[3,1,0].cube.transform.RotateAround(tempPoint, grid[3,1,0].cube.transform.right, tempAngle);
			grid[5,1,0].cube.transform.RotateAround(tempPoint, grid[5,1,0].cube.transform.right, tempAngle);
			grid[6,1,0].cube.transform.RotateAround(tempPoint, grid[6,1,0].cube.transform.right, tempAngle);

			Vector3 tempVect = cubesParent.transform.right * 0.8f + cubesParent.transform.up * 0.1f + cubesParent.transform.forward;
			cubesParent.transform.rotation = Quaternion.AngleAxis(8.0f * Mathf.Sin(finishTimer / 50.0f),tempVect);
			
			return;
		}

		if(tempLevel == 29)
		{
			Vector3 tempPoint = grid[0,3,3].cube.transform.position;
			float tempAngle = 24.0f * Mathf.Sin(finishTimer / 2.0f);
			
			grid[0,4,2].cube.transform.RotateAround(tempPoint, grid[0,4,2].cube.transform.forward, tempAngle);
			grid[0,4,3].cube.transform.RotateAround(tempPoint, grid[0,4,3].cube.transform.forward, tempAngle);
			grid[0,4,4].cube.transform.RotateAround(tempPoint, grid[0,4,4].cube.transform.forward, tempAngle);
			grid[0,5,0].cube.transform.RotateAround(tempPoint, grid[0,5,0].cube.transform.forward, tempAngle);
			grid[0,5,1].cube.transform.RotateAround(tempPoint, grid[0,5,1].cube.transform.forward, tempAngle);
			grid[0,5,2].cube.transform.RotateAround(tempPoint, grid[0,5,2].cube.transform.forward, tempAngle);
			grid[0,5,3].cube.transform.RotateAround(tempPoint, grid[0,5,3].cube.transform.forward, tempAngle);
			grid[0,6,0].cube.transform.RotateAround(tempPoint, grid[0,6,0].cube.transform.forward, tempAngle);
			grid[0,6,1].cube.transform.RotateAround(tempPoint, grid[0,6,1].cube.transform.forward, tempAngle);
			grid[0,6,2].cube.transform.RotateAround(tempPoint, grid[0,6,2].cube.transform.forward, tempAngle);

			tempPoint = grid[2,3,3].cube.transform.position;
			
			grid[2,4,2].cube.transform.RotateAround(tempPoint, grid[2,4,2].cube.transform.forward, -tempAngle);
			grid[2,4,3].cube.transform.RotateAround(tempPoint, grid[2,4,3].cube.transform.forward, -tempAngle);
			grid[2,4,4].cube.transform.RotateAround(tempPoint, grid[2,4,4].cube.transform.forward, -tempAngle);
			grid[2,5,0].cube.transform.RotateAround(tempPoint, grid[2,5,0].cube.transform.forward, -tempAngle);
			grid[2,5,1].cube.transform.RotateAround(tempPoint, grid[2,5,1].cube.transform.forward, -tempAngle);
			grid[2,5,2].cube.transform.RotateAround(tempPoint, grid[2,5,2].cube.transform.forward, -tempAngle);
			grid[2,5,3].cube.transform.RotateAround(tempPoint, grid[2,5,3].cube.transform.forward, -tempAngle);
			grid[2,6,0].cube.transform.RotateAround(tempPoint, grid[2,6,0].cube.transform.forward, -tempAngle);
			grid[2,6,1].cube.transform.RotateAround(tempPoint, grid[2,6,1].cube.transform.forward, -tempAngle);
			grid[2,6,2].cube.transform.RotateAround(tempPoint, grid[2,6,2].cube.transform.forward, -tempAngle);

			tempPoint = grid[1,4,6].cube.transform.position * 0.5f + grid[1,4,7].cube.transform.position * 0.5f;
			tempAngle = - 0.3f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI / 2);
			
			grid[0,5,6].cube.transform.RotateAround(tempPoint, grid[0,5,6].cube.transform.right, tempAngle);
			grid[0,6,6].cube.transform.RotateAround(tempPoint, grid[0,6,6].cube.transform.right, tempAngle);
			grid[0,6,7].cube.transform.RotateAround(tempPoint, grid[0,6,7].cube.transform.right, tempAngle);
			grid[2,5,6].cube.transform.RotateAround(tempPoint, grid[2,5,6].cube.transform.right, tempAngle);
			grid[2,6,6].cube.transform.RotateAround(tempPoint, grid[2,6,6].cube.transform.right, tempAngle);
			grid[2,6,7].cube.transform.RotateAround(tempPoint, grid[2,6,7].cube.transform.right, tempAngle);

			tempPoint.x = grid[1,3,5].cube.transform.position.x;
			tempPoint.y = grid[1,2,5].cube.transform.position.y * 0.5f + grid[1,3,5].cube.transform.position.y * 0.5f;
			tempPoint.z = grid[1,3,5].cube.transform.position.z * 0.5f + grid[1,3,6].cube.transform.position.z * 0.5f;
			tempAngle = 0.3f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI / 2);
			
			grid[0,1,7].cube.transform.RotateAround(tempPoint, grid[0,1,7].cube.transform.right, tempAngle);
			grid[0,2,6].cube.transform.RotateAround(tempPoint, grid[0,2,6].cube.transform.right, tempAngle);
			grid[2,1,7].cube.transform.RotateAround(tempPoint, grid[2,1,7].cube.transform.right, tempAngle);
			grid[2,2,6].cube.transform.RotateAround(tempPoint, grid[2,2,6].cube.transform.right, tempAngle);

			tempPoint.y = grid[0,1,5].cube.transform.position.y * 0.5f + grid[1,2,5].cube.transform.position.y * 0.5f;
			tempAngle = 0.2f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI * 1.3f / 2);
			
			grid[0,0,4].cube.transform.RotateAround(tempPoint, grid[0,1,7].cube.transform.right, tempAngle);
			grid[0,1,5].cube.transform.RotateAround(tempPoint, grid[0,2,6].cube.transform.right, tempAngle);
			grid[2,0,4].cube.transform.RotateAround(tempPoint, grid[2,1,7].cube.transform.right, tempAngle);
			grid[2,1,5].cube.transform.RotateAround(tempPoint, grid[2,2,6].cube.transform.right, tempAngle);

			tempPoint.z = grid[1,2,3].cube.transform.position.z * 0.5f + grid[1,2,4].cube.transform.position.z * 0.5f;
			tempAngle = 0.15f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI * 1.7f / 2);
			
			grid[0,0,2].cube.transform.RotateAround(tempPoint, grid[0,1,7].cube.transform.right, tempAngle);
			grid[0,1,3].cube.transform.RotateAround(tempPoint, grid[0,2,6].cube.transform.right, tempAngle);
			grid[2,0,2].cube.transform.RotateAround(tempPoint, grid[2,1,7].cube.transform.right, tempAngle);
			grid[2,1,3].cube.transform.RotateAround(tempPoint, grid[2,2,6].cube.transform.right, tempAngle);

			Vector3 tempVect = cubesParent.transform.right * 0.2f + cubesParent.transform.up * 0.1f + cubesParent.transform.forward;
			cubesParent.transform.rotation = Quaternion.AngleAxis(4.0f * Mathf.Sin(finishTimer / 30.0f),tempVect);
			
			return;
		}

		if(tempLevel == 30)
		{
			cubesParent.transform.rotation = Quaternion.AngleAxis(finishTimer,Vector3.up);
			
			return;
		}

		if(tempLevel == 31)
		{
			Vector3 tempPoint = grid[4,2,0].cube.transform.position * 0.5f + grid[3,2,0].cube.transform.position * 0.5f;
			float tempAngle = - 1.5f * Mathf.Sin(finishTimer / 60.0f);
			
			grid[1,4,0].cube.transform.RotateAround(tempPoint, grid[1,4,0].cube.transform.right, tempAngle);
			grid[2,3,0].cube.transform.RotateAround(tempPoint, grid[2,3,0].cube.transform.right, tempAngle);
			grid[2,4,0].cube.transform.RotateAround(tempPoint, grid[2,4,0].cube.transform.right, tempAngle);
			grid[3,2,0].cube.transform.RotateAround(tempPoint, grid[3,2,0].cube.transform.right, tempAngle);
			grid[3,3,0].cube.transform.RotateAround(tempPoint, grid[3,3,0].cube.transform.right, tempAngle);
			grid[4,2,0].cube.transform.RotateAround(tempPoint, grid[4,2,0].cube.transform.right, tempAngle);
			grid[4,3,0].cube.transform.RotateAround(tempPoint, grid[4,3,0].cube.transform.right, tempAngle);
			grid[5,3,0].cube.transform.RotateAround(tempPoint, grid[5,3,0].cube.transform.right, tempAngle);
			grid[5,4,0].cube.transform.RotateAround(tempPoint, grid[5,4,0].cube.transform.right, tempAngle);
			grid[6,4,0].cube.transform.RotateAround(tempPoint, grid[6,4,0].cube.transform.right, tempAngle);

			tempPoint = grid[2,1,4].cube.transform.position;
			tempAngle = 0.5f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI / 2);
			
			grid[0,0,4].cube.transform.RotateAround(tempPoint, grid[0,0,4].cube.transform.forward, tempAngle);
			grid[1,1,4].cube.transform.RotateAround(tempPoint, grid[1,1,4].cube.transform.forward, tempAngle);
			grid[1,1,5].cube.transform.RotateAround(tempPoint, grid[1,1,5].cube.transform.forward, tempAngle);

			tempPoint = grid[5,1,4].cube.transform.position;
			grid[6,1,4].cube.transform.RotateAround(tempPoint, grid[6,1,4].cube.transform.forward, -tempAngle);
			grid[6,1,5].cube.transform.RotateAround(tempPoint, grid[6,1,5].cube.transform.forward, -tempAngle);
			grid[7,0,4].cube.transform.RotateAround(tempPoint, grid[7,0,4].cube.transform.forward, -tempAngle);

			tempPoint = grid[3,1,4].cube.transform.position * 0.25f + grid[3,2,4].cube.transform.position * 0.25f +
				grid[4,1,4].cube.transform.position * 0.25f + grid[4,2,4].cube.transform.position * 0.25f;
			tempAngle = - 0.05f * Mathf.Sin(finishTimer / 60.0f - Mathf.PI / 2);

			for (int x= 1; x< 7; x++)
			{
				for (int y= 0; y< 5; y++)
				{
					for (int z= 0; z< 4; z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);

						if(z < 3)
							grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);

						if(z < 2)
							grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);


					}
				}
			}

			if(finishTimer % 40 == 30)
			{
				tempPoint = grid[3,3,6].cube.transform.position * 0.5f + grid[4,3,6].cube.transform.position * 0.5f;

				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.position = tempPoint;
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Diffuse"));
				cube.renderer.material.color = new Color(0.25f, 0.8f, 1);
			}

			for(int i=0; i < endAnimationParent.transform.childCount; i++)
			{
				GameObject cube = endAnimationParent.transform.GetChild(i).gameObject;

				if(cube.transform.position.y < 1.2f)
				{
					Component tempC = cube.GetComponent<Rigidbody>();
					if(tempC == null)
						cube.transform.Translate(0,0.2f,0);
				}

				if(Mathf.Abs(cube.transform.position.y - 1.2f) < 0.05f)
				{
					Component tempC = cube.GetComponent<Rigidbody>();
					if(tempC != null)
						continue;
					cube.AddComponent<Rigidbody>();
					cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
					cube.rigidbody.AddForce (new Vector3(Random.Range (-200f, 200f),400f,Random.Range (-200f, 200f)));
				}

				if(cube.transform.position.y < -20f)
				{
					Destroy(cube);
				}

			}
			
			return;
		}

		if(tempLevel == 32)
		{
			Vector3 tempPoint = grid[0,1,5].cube.transform.position;
			grid[0,1,6].cube.transform.RotateAround(tempPoint, grid[0,1,6].cube.transform.right, 2.0f);

			for(int i = 0; i < endAnimationParent.transform.childCount; i++)
			{
				GameObject go = endAnimationParent.transform.GetChild(i).gameObject;

				for(int j= 1; j< go.renderer.materials.Length; j++)
				{
					Color tempColor = go.renderer.materials[j].color;
					tempColor.a -= 0.01f;
					go.renderer.materials[j].color = tempColor;
				}
				
				if((go.renderer.materials.Length > 1) && (go.renderer.materials[1].color.a < 0.01f))
				{
					Material tempMat = go.renderer.materials[0];
					go.renderer.materials = new Material[1];
					go.renderer.material = tempMat;
				}

				if(i>2)
					tempPoint = grid[0,1,1].cube.transform.position;
				if(i>5)
					tempPoint = grid[6,1,5].cube.transform.position;
				if(i>8)
					tempPoint = grid[6,1,1].cube.transform.position;

				go.transform.RotateAround(tempPoint, grid[0,1,5].cube.transform.right, 2.0f);
			}

			tempPoint = grid[0,1,1].cube.transform.position;
			grid[0,1,0].cube.transform.RotateAround(tempPoint, grid[0,1,0].cube.transform.right, 2.0f);

			tempPoint = grid[6,1,5].cube.transform.position;
			grid[6,1,6].cube.transform.RotateAround(tempPoint, grid[6,1,6].cube.transform.right, 2.0f);

			tempPoint = grid[6,1,1].cube.transform.position;
			grid[6,1,0].cube.transform.RotateAround(tempPoint, grid[6,1,0].cube.transform.right, 2.0f);


			float tempAngle = - 0.6f * Mathf.Sin(finishTimer / 80.0f - Mathf.PI / 2);
			tempPoint = grid[3,2,1].cube.transform.position * 0.5f + grid[3,2,2].cube.transform.position * 0.5f;

			for (int x= 1; x< 6; x++)
			{
				for (int y= 3; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.up, tempAngle);
					}
				}
			}

			Vector3 tempVect = cubesParent.transform.right + cubesParent.transform.up * 0.1f + cubesParent.transform.forward;
			cubesParent.transform.rotation = Quaternion.AngleAxis(0.3f * Mathf.Sin(finishTimer / 5.0f),tempVect);
			
			return;
		}

		if(tempLevel == 33)
		{
			Vector3 tempPoint = grid[3,4,6].cube.transform.position;

			float tempAngle = - 0.1f * Mathf.Sin(finishTimer / 60.0f);
			
			grid[2,3,4].cube.transform.RotateAround(tempPoint, grid[2,3,4].cube.transform.right, tempAngle);
			grid[2,4,1].cube.transform.RotateAround(tempPoint, grid[2,4,1].cube.transform.right, tempAngle);
			grid[2,5,1].cube.transform.RotateAround(tempPoint, grid[2,5,1].cube.transform.right, tempAngle);
			grid[2,6,1].cube.transform.RotateAround(tempPoint, grid[2,5,1].cube.transform.right, tempAngle);
			grid[3,3,4].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,0].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,1].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,2].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,3].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,4].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,5].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,6].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,4,7].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,5,1].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[3,6,1].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[4,4,1].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[4,5,1].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);
			grid[4,6,1].cube.transform.RotateAround(tempPoint, grid[3,3,4].cube.transform.right, tempAngle);

			tempPoint = grid[1,5,5].cube.transform.position * 0.25f + grid[1,5,6].cube.transform.position * 0.25f +
				grid[2,5,5].cube.transform.position * 0.25f + grid[2,5,6].cube.transform.position * 0.25f;
			tempAngle = 0.1f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI / 2);

			for (int x= 0; x< 4; x++)
			{
				for (int y= 5; y< 7; y++)
				{
					for (int z= 4; z< 8; z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
				}
			}
			grid[1,4,5].cube.transform.RotateAround(tempPoint, grid[1,4,5].cube.transform.right, tempAngle);
			grid[1,4,6].cube.transform.RotateAround(tempPoint, grid[1,4,6].cube.transform.right, tempAngle);
			grid[2,4,5].cube.transform.RotateAround(tempPoint, grid[2,4,5].cube.transform.right, tempAngle);
			grid[2,4,6].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.right, tempAngle);

			tempPoint = grid[1,1,5].cube.transform.position * 0.5f + grid[1,1,6].cube.transform.position * 0.5f;
			tempAngle = - 0.05f * Mathf.Sin(finishTimer / 60.0f);

			for (int x= 0; x< levelArray.GetLength(0); x++)
			{
				for (int y= 1; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);

						if(y > 1)
							grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
						if(y > 2)
							grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
				}
			}

			grid[1,0,6].cube.transform.Translate(0,0,-tempAngle / 15.0f);

			
			return;
		}

		if(tempLevel == 34)
		{
			Vector3 tempPoint = grid[2,2,4].cube.transform.position * 0.5f + grid[2,1,4].cube.transform.position * 0.5f;
			float tempAngle = 0.01f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI / 2);

			for (int x= 0; x< levelArray.GetLength(0); x++)
			{
				for (int y= 2; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						Vector3 tempV = grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.up * 0.2f + grid[x,y,z].cube.transform.forward * 0.5f;
						grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);

						if(y > 2)
						{
							tempV = grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.up * 0.2f + grid[x,y,z].cube.transform.forward * 0.5f;
							grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						}
						if(y > 3)
						{
							tempV = grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.up * 0.2f + grid[x,y,z].cube.transform.forward * 0.5f;
							grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						}
					}
				}
			}
			
			return;
		}

		if(tempLevel == 35)
		{
			Vector3 tempPoint = grid[0,0,5].cube.transform.position * 0.25f + grid[0,0,6].cube.transform.position * 0.25f +
				grid[0,1,5].cube.transform.position * 0.25f + grid[0,1,6].cube.transform.position * 0.25f;
			
			grid[0,0,5].cube.transform.RotateAround(tempPoint, grid[0,0,5].cube.transform.right, -10.0f);
			grid[0,0,6].cube.transform.RotateAround(tempPoint, grid[0,0,6].cube.transform.right, -10.0f);
			grid[0,1,5].cube.transform.RotateAround(tempPoint, grid[0,1,5].cube.transform.right, -10.0f);
			grid[0,1,6].cube.transform.RotateAround(tempPoint, grid[0,1,6].cube.transform.right, -10.0f);

			tempPoint = grid[0,0,1].cube.transform.position * 0.25f + grid[0,0,2].cube.transform.position * 0.25f +
				grid[0,1,1].cube.transform.position * 0.25f + grid[0,1,2].cube.transform.position * 0.25f;
			
			grid[0,0,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, -10.0f);
			grid[0,0,2].cube.transform.RotateAround(tempPoint, grid[0,0,2].cube.transform.right, -10.0f);
			grid[0,1,1].cube.transform.RotateAround(tempPoint, grid[0,1,1].cube.transform.right, -10.0f);
			grid[0,1,2].cube.transform.RotateAround(tempPoint, grid[0,1,2].cube.transform.right, -10.0f);

			tempPoint = grid[4,0,5].cube.transform.position * 0.25f + grid[4,0,6].cube.transform.position * 0.25f +
				grid[4,1,5].cube.transform.position * 0.25f + grid[4,1,6].cube.transform.position * 0.25f;
			
			grid[4,0,5].cube.transform.RotateAround(tempPoint, grid[4,0,5].cube.transform.right, -10.0f);
			grid[4,0,6].cube.transform.RotateAround(tempPoint, grid[4,0,6].cube.transform.right, -10.0f);
			grid[4,1,5].cube.transform.RotateAround(tempPoint, grid[4,1,5].cube.transform.right, -10.0f);
			grid[4,1,6].cube.transform.RotateAround(tempPoint, grid[4,1,6].cube.transform.right, -10.0f);

			tempPoint = grid[4,0,1].cube.transform.position * 0.25f + grid[4,0,2].cube.transform.position * 0.25f +
				grid[4,1,1].cube.transform.position * 0.25f + grid[4,1,2].cube.transform.position * 0.25f;
			
			grid[4,0,1].cube.transform.RotateAround(tempPoint, grid[4,0,1].cube.transform.right, -10.0f);
			grid[4,0,2].cube.transform.RotateAround(tempPoint, grid[4,0,2].cube.transform.right, -10.0f);
			grid[4,1,1].cube.transform.RotateAround(tempPoint, grid[4,1,1].cube.transform.right, -10.0f);
			grid[4,1,2].cube.transform.RotateAround(tempPoint, grid[4,1,2].cube.transform.right, -10.0f);

			cubesParent.transform.rotation = Quaternion.AngleAxis(0.2f * Mathf.Sin(finishTimer / 2.0f), new Vector3(1.0f,0.1f,0.4f));
			
			return;
		}

		if(tempLevel == 36)
		{
			if(finishTimer % 20 == 10)
			{
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
				cube.transform.position = grid[3,6,0].cube.transform.position;
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Transparent/Diffuse"));
				cube.renderer.material.color = new Color(1, 1, 1, 0.5f);
			}
			
			for(int i=0; i < endAnimationParent.transform.childCount; i++)
			{
				Transform cube = endAnimationParent.transform.GetChild(i);
				
				cube.Translate(0,0.2f,0);
				cube.localScale = new Vector3(cube.localScale.x + 0.02f, cube.localScale.y + 0.02f, cube.localScale.z + 0.02f);
				
				cube.gameObject.renderer.material.color = new Color(1,1,1, cube.gameObject.renderer.material.color.a - 0.01f);
				
				if(cube.gameObject.renderer.material.color.a < 0.01f)
				{
					Destroy(cube.gameObject);
				}
			}

			Vector3 tempPoint = grid[3,2,2].cube.transform.position * 0.25f + grid[3,2,3].cube.transform.position * 0.25f +
				grid[3,3,2].cube.transform.position * 0.25f + grid[3,3,3].cube.transform.position * 0.25f;
			
			float tempAngle = - 0.2f * Mathf.Sin(finishTimer / 60.0f - Mathf.PI / 2);
			
			grid[3,3,3].cube.transform.RotateAround(tempPoint, grid[3,3,3].cube.transform.right, tempAngle);
			grid[3,4,3].cube.transform.RotateAround(tempPoint, grid[3,4,3].cube.transform.right, tempAngle);
			grid[3,4,4].cube.transform.RotateAround(tempPoint, grid[3,4,4].cube.transform.right, tempAngle);
			grid[3,5,4].cube.transform.RotateAround(tempPoint, grid[3,5,4].cube.transform.right, tempAngle);
			grid[3,5,5].cube.transform.RotateAround(tempPoint, grid[3,5,5].cube.transform.right, tempAngle);
			grid[3,6,5].cube.transform.RotateAround(tempPoint, grid[3,6,5].cube.transform.right, tempAngle);

			for (int x= 2; x< 5; x++)
			{
				for (int y= 1; y< levelArray.GetLength(1); y++)
				{
					for (int z= 6; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
				}
			}

			tempPoint = grid[3,7,8].cube.transform.position;
			
			tempAngle = 0.4f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI);
			
			grid[3,4,8].cube.transform.RotateAround(tempPoint, grid[3,4,8].cube.transform.right, tempAngle);
			grid[3,5,8].cube.transform.RotateAround(tempPoint, grid[3,5,8].cube.transform.right, tempAngle);
			grid[3,6,8].cube.transform.RotateAround(tempPoint, grid[3,6,8].cube.transform.right, tempAngle);
			grid[3,7,8].cube.transform.RotateAround(tempPoint, grid[3,7,8].cube.transform.right, tempAngle);
			
			for (int x= 2; x< 5; x++)
			{
				for (int y= 1; y< 4; y++)
				{
					for (int z= 6; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
				}
			}

			tempPoint = grid[3,3,8].cube.transform.position * 0.5f + grid[3,4,8].cube.transform.position * 0.5f;
			
			tempAngle = 0.4f * Mathf.Sin(finishTimer / 20.0f - Mathf.PI);

			for (int x= 2; x< 5; x++)
			{
				for (int y= 1; y< 4; y++)
				{
					for (int z= 6; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						grid[x,y,z].cube.transform.RotateAround(tempPoint, grid[x,y,z].cube.transform.right, tempAngle);
					}
				}
			}

			cubesParent.transform.rotation = Quaternion.AngleAxis(0.15f * Mathf.Sin(finishTimer / 3.0f), new Vector3(1.0f,0.1f,0.4f));
			
			return;
		}

		if(tempLevel == 37)
		{
			if(finishTimer % 10 == 8)
			{					
				GameObject cube = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
				cube.transform.parent = endAnimationParent.transform;
				cube.transform.position = new Vector3(Random.Range(-4.0f,4.0f), 10, Random.Range(-4.0f,4.0f));
				cube.transform.localScale = cube.transform.localScale * 0.8f;
				cube.renderer.materials = new Material[1];
				cube.renderer.material = new Material(Shader.Find ("Diffuse"));
				cube.renderer.material.color = new Color(0.25f, 0.5f, 1);
				cube.AddComponent<Rigidbody>();
				cube.rigidbody.AddTorque (new Vector3(Random.Range (-25f, 25f),Random.Range (-25f, 25f),Random.Range (-25f, 25f)));
				cube.rigidbody.AddForce (new Vector3(Random.Range (-100f, 100f),-200f,Random.Range (-100f, 100f)));
				cube.rigidbody.mass = 0.01f;
			}
				
			for(int i=0; i < endAnimationParent.transform.childCount; i++)
			{
				GameObject cube = endAnimationParent.transform.GetChild(i).gameObject;
					
				if(cube.transform.position.y < -20f)
				{
					Destroy(cube);
				}
				
			}
			
			return;
		}

		if(tempLevel == 38)
		{			
			float tempAngle = - 0.007f * Mathf.Sin(finishTimer / 50.0f);
			
			grid[0,0,3].cube.transform.Translate(0,0,tempAngle);
			grid[0,0,4].cube.transform.Translate(0,0,tempAngle);
			grid[0,1,3].cube.transform.Translate(0,0,tempAngle);
			grid[0,1,4].cube.transform.Translate(0,0,tempAngle);
			grid[1,0,3].cube.transform.Translate(0,0,tempAngle);
			grid[1,0,4].cube.transform.Translate(0,0,tempAngle);
			grid[1,1,3].cube.transform.Translate(0,0,tempAngle);
			grid[1,1,4].cube.transform.Translate(0,0,tempAngle);

			grid[0,0,0].cube.transform.Translate(0,0,-tempAngle);
			grid[0,0,1].cube.transform.Translate(0,0,-tempAngle);
			grid[0,1,0].cube.transform.Translate(0,0,-tempAngle);
			grid[0,1,1].cube.transform.Translate(0,0,-tempAngle);
			grid[1,0,0].cube.transform.Translate(0,0,-tempAngle);
			grid[1,0,1].cube.transform.Translate(0,0,-tempAngle);
			grid[1,1,0].cube.transform.Translate(0,0,-tempAngle);
			grid[1,1,1].cube.transform.Translate(0,0,-tempAngle);

			grid[3,0,5].cube.transform.Translate(0,0,-tempAngle);
			grid[3,0,6].cube.transform.Translate(0,0,-tempAngle);
			grid[3,1,5].cube.transform.Translate(0,0,-tempAngle);
			grid[3,1,6].cube.transform.Translate(0,0,-tempAngle);
			grid[4,0,5].cube.transform.Translate(0,0,-tempAngle);
			grid[4,0,6].cube.transform.Translate(0,0,-tempAngle);
			grid[4,1,5].cube.transform.Translate(0,0,-tempAngle);
			grid[4,1,6].cube.transform.Translate(0,0,-tempAngle);

			grid[3,0,1].cube.transform.Translate(0,0,tempAngle);
			grid[3,0,2].cube.transform.Translate(0,0,tempAngle);
			grid[3,1,1].cube.transform.Translate(0,0,tempAngle);
			grid[3,1,2].cube.transform.Translate(0,0,tempAngle);
			grid[4,0,1].cube.transform.Translate(0,0,tempAngle);
			grid[4,0,2].cube.transform.Translate(0,0,tempAngle);
			grid[4,1,1].cube.transform.Translate(0,0,tempAngle);
			grid[4,1,2].cube.transform.Translate(0,0,tempAngle);

			Vector3 tempPoint = grid[2,3,3].cube.transform.position * 0.5f + grid[2,4,3].cube.transform.position * 0.5f;
			tempAngle = 0.07f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI / 2);
			
			for (int x= 0; x< levelArray.GetLength(0); x++)
			{
				for (int y= 2; y< levelArray.GetLength(1); y++)
				{
					for (int z= 4; z< levelArray.GetLength(2); z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						if(((x == 3) || (x == 4)) && (y == 2) && (z == 6))
							continue;
						
						Vector3 tempV = grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.up * 0.5f + grid[x,y,z].cube.transform.forward * 0.2f;
						grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						
						if(z > 4)
						{
							tempV = grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.up * 0.5f + grid[x,y,z].cube.transform.forward * 0.2f;
							grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						}
						if(z > 5)
						{
							tempV = grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.up * 0.5f + grid[x,y,z].cube.transform.forward * 0.2f;
							grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						}
					}
				}
			}

			tempAngle = 0.03f * Mathf.Sin(finishTimer / 40.0f - Mathf.PI / 2);

			for (int x= 0; x< levelArray.GetLength(0); x++)
			{
				for (int y= 0; y< levelArray.GetLength(1); y++)
				{
					for (int z= 0; z< 3; z++)
					{
						if(grid[x,y,z].cube == null)
							continue;
						
						Vector3 tempV = grid[x,y,z].cube.transform.right * 0.5f + grid[x,y,z].cube.transform.up - grid[x,y,z].cube.transform.forward * 0.2f;
						grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						
						if(z < 2)
						{
							tempV = grid[x,y,z].cube.transform.right * 0.5f + grid[x,y,z].cube.transform.up - grid[x,y,z].cube.transform.forward * 0.2f;
							grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						}
						if(z < 1)
						{
							tempV = grid[x,y,z].cube.transform.right * 0.5f + grid[x,y,z].cube.transform.up - grid[x,y,z].cube.transform.forward * 0.2f;
							grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						}
					}
				}
			}

			cubesParent.transform.rotation = Quaternion.AngleAxis(2.0f * Mathf.Sin(finishTimer / 60.0f), new Vector3(0.5f,0.1f,1.0f));
			
			return;
		}

		if(tempLevel == 39)
		{
			Vector3 tempPoint;
			
			float tempAngle;


			if((finishTimer % 360 <= 120) &&(finishTimer >= 60))
			{
				tempPoint = grid[0,1,1].cube.transform.position * 0.5f + grid[0,2,2].cube.transform.position * 0.5f;
				
				tempAngle = - 1.5f * Mathf.Sin((finishTimer - 60) / 10.0f);

				grid[0,0,0].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, tempAngle);
				grid[0,0,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, tempAngle);
				grid[0,0,2].cube.transform.RotateAround(tempPoint, grid[0,0,2].cube.transform.right, tempAngle);
				grid[0,0,3].cube.transform.RotateAround(tempPoint, grid[0,0,3].cube.transform.right, tempAngle);
				grid[0,1,0].cube.transform.RotateAround(tempPoint, grid[0,1,0].cube.transform.right, tempAngle);
				grid[0,1,1].cube.transform.RotateAround(tempPoint, grid[0,1,1].cube.transform.right, tempAngle);

				tempPoint = grid[5,1,1].cube.transform.position * 0.5f + grid[5,2,2].cube.transform.position * 0.5f;

				grid[5,0,0].cube.transform.RotateAround(tempPoint, grid[5,0,0].cube.transform.right, tempAngle);
				grid[5,0,1].cube.transform.RotateAround(tempPoint, grid[5,0,1].cube.transform.right, tempAngle);
				grid[5,0,2].cube.transform.RotateAround(tempPoint, grid[5,0,2].cube.transform.right, tempAngle);
				grid[5,0,3].cube.transform.RotateAround(tempPoint, grid[5,0,3].cube.transform.right, tempAngle);
				grid[5,1,0].cube.transform.RotateAround(tempPoint, grid[5,1,0].cube.transform.right, tempAngle);
				grid[5,1,1].cube.transform.RotateAround(tempPoint, grid[5,1,1].cube.transform.right, tempAngle);

				tempPoint = grid[0,0,0].cube.transform.position * 0.5f + grid[0,1,0].cube.transform.position * 0.5f;			
				tempAngle = 9.0f * Mathf.Sin((finishTimer - 60) / 10.0f);

				grid[0,0,0].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, tempAngle);
				grid[0,0,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, tempAngle);
				grid[0,0,2].cube.transform.RotateAround(tempPoint, grid[0,0,2].cube.transform.right, tempAngle);
				grid[0,0,3].cube.transform.RotateAround(tempPoint, grid[0,0,3].cube.transform.right, tempAngle);

				tempPoint = grid[5,0,0].cube.transform.position * 0.5f + grid[5,1,0].cube.transform.position * 0.5f;			
				tempAngle = 9.0f * Mathf.Sin((finishTimer - 60) / 10.0f);
				
				grid[5,0,0].cube.transform.RotateAround(tempPoint, grid[5,0,0].cube.transform.right, tempAngle);
				grid[5,0,1].cube.transform.RotateAround(tempPoint, grid[5,0,1].cube.transform.right, tempAngle);
				grid[5,0,2].cube.transform.RotateAround(tempPoint, grid[5,0,2].cube.transform.right, tempAngle);
				grid[5,0,3].cube.transform.RotateAround(tempPoint, grid[5,0,3].cube.transform.right, tempAngle);

				tempPoint = grid[0,2,4].cube.transform.position * 0.5f + grid[0,3,3].cube.transform.position * 0.5f;
				tempAngle = - 0.5f * Mathf.Sin((finishTimer - 60) / 10.0f);
				
				grid[0,0,0].cube.transform.RotateAround(tempPoint, grid[0,0,0].cube.transform.right, 2 * tempAngle);
				grid[0,0,1].cube.transform.RotateAround(tempPoint, grid[0,0,1].cube.transform.right, 2 * tempAngle);
				grid[0,0,2].cube.transform.RotateAround(tempPoint, grid[0,0,2].cube.transform.right, 2 * tempAngle);
				grid[0,0,3].cube.transform.RotateAround(tempPoint, grid[0,0,3].cube.transform.right, 2 * tempAngle);
				grid[0,1,0].cube.transform.RotateAround(tempPoint, grid[0,1,0].cube.transform.right, 2 * tempAngle);
				grid[0,1,1].cube.transform.RotateAround(tempPoint, grid[0,1,1].cube.transform.right, 2 * tempAngle);
				grid[0,2,2].cube.transform.RotateAround(tempPoint, grid[0,2,2].cube.transform.right, 2 * tempAngle);
				grid[0,2,3].cube.transform.RotateAround(tempPoint, grid[0,2,3].cube.transform.right, tempAngle);
				grid[0,3,3].cube.transform.RotateAround(tempPoint, grid[0,3,3].cube.transform.right, tempAngle);

				tempPoint = grid[5,2,4].cube.transform.position * 0.5f + grid[5,3,3].cube.transform.position * 0.5f;
				
				grid[5,0,0].cube.transform.RotateAround(tempPoint, grid[5,0,0].cube.transform.right, 2 * tempAngle);
				grid[5,0,1].cube.transform.RotateAround(tempPoint, grid[5,0,1].cube.transform.right, 2 * tempAngle);
				grid[5,0,2].cube.transform.RotateAround(tempPoint, grid[5,0,2].cube.transform.right, 2 * tempAngle);
				grid[5,0,3].cube.transform.RotateAround(tempPoint, grid[5,0,3].cube.transform.right, 2 * tempAngle);
				grid[5,1,0].cube.transform.RotateAround(tempPoint, grid[5,1,0].cube.transform.right, 2 * tempAngle);
				grid[5,1,1].cube.transform.RotateAround(tempPoint, grid[5,1,1].cube.transform.right, 2 * tempAngle);
				grid[5,2,2].cube.transform.RotateAround(tempPoint, grid[5,2,2].cube.transform.right, 2 * tempAngle);
				grid[5,2,3].cube.transform.RotateAround(tempPoint, grid[5,2,3].cube.transform.right, tempAngle);
				grid[5,3,3].cube.transform.RotateAround(tempPoint, grid[5,3,3].cube.transform.right, tempAngle);


				tempPoint = grid[0,2,5].cube.transform.position;
				
				tempAngle = Mathf.Sin((finishTimer - 60) / 10.0f);
				
				grid[0,0,6].cube.transform.RotateAround(tempPoint, grid[0,0,6].cube.transform.right, tempAngle);
				grid[0,0,7].cube.transform.RotateAround(tempPoint, grid[0,0,6].cube.transform.right, tempAngle);
				grid[0,1,5].cube.transform.RotateAround(tempPoint, grid[0,1,5].cube.transform.right, tempAngle);
				grid[0,2,5].cube.transform.RotateAround(tempPoint, grid[0,2,5].cube.transform.right, tempAngle);
				grid[1,0,6].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, tempAngle);
				
				tempPoint = grid[5,2,5].cube.transform.position;

				grid[4,0,6].cube.transform.RotateAround(tempPoint, grid[4,0,6].cube.transform.right, tempAngle);
				grid[5,0,6].cube.transform.RotateAround(tempPoint, grid[5,0,6].cube.transform.right, tempAngle);
				grid[5,0,7].cube.transform.RotateAround(tempPoint, grid[5,0,7].cube.transform.right, tempAngle);
				grid[5,1,5].cube.transform.RotateAround(tempPoint, grid[5,1,5].cube.transform.right, tempAngle);
				grid[5,2,5].cube.transform.RotateAround(tempPoint, grid[5,2,5].cube.transform.right, tempAngle);

				tempAngle = 2* Mathf.Sin((finishTimer - 60) / 10.0f);
				tempPoint = grid[0,0,6].cube.transform.position * 0.5f + grid[0,1,5].cube.transform.position * 0.5f;
				
				grid[0,0,6].cube.transform.RotateAround(tempPoint, grid[0,0,6].cube.transform.right, tempAngle);
				grid[0,0,7].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, tempAngle);
				grid[1,0,6].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, tempAngle);

				tempPoint = grid[5,0,6].cube.transform.position * 0.5f + grid[5,1,5].cube.transform.position * 0.5f;

				grid[4,0,6].cube.transform.RotateAround(tempPoint, grid[4,0,6].cube.transform.right, tempAngle);
				grid[5,0,6].cube.transform.RotateAround(tempPoint, grid[5,0,6].cube.transform.right, tempAngle);
				grid[5,0,7].cube.transform.RotateAround(tempPoint, grid[5,0,7].cube.transform.right, tempAngle);
			}

			if((finishTimer >= 60) && (finishTimer <= 180))
			{
				cubesParent.transform.Translate(0, 0.2f * Mathf.Sin((finishTimer - 60) / 20.0f), 0);
			}

			if((finishTimer >= 120) && (finishTimer <= 180))
			{
				tempPoint = grid[0,2,5].cube.transform.position;
				
				tempAngle = 0.5f * Mathf.Sin((finishTimer - 60) / 10.0f);
				
				grid[0,0,6].cube.transform.RotateAround(tempPoint, grid[0,0,6].cube.transform.right, tempAngle);
				grid[0,0,7].cube.transform.RotateAround(tempPoint, grid[0,0,6].cube.transform.right, tempAngle);
				grid[0,1,5].cube.transform.RotateAround(tempPoint, grid[0,1,5].cube.transform.right, tempAngle);
				grid[0,2,5].cube.transform.RotateAround(tempPoint, grid[0,2,5].cube.transform.right, tempAngle);
				grid[1,0,6].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, tempAngle);
				
				tempPoint = grid[5,2,5].cube.transform.position;
				
				grid[4,0,6].cube.transform.RotateAround(tempPoint, grid[4,0,6].cube.transform.right, tempAngle);
				grid[5,0,6].cube.transform.RotateAround(tempPoint, grid[5,0,6].cube.transform.right, tempAngle);
				grid[5,0,7].cube.transform.RotateAround(tempPoint, grid[5,0,7].cube.transform.right, tempAngle);
				grid[5,1,5].cube.transform.RotateAround(tempPoint, grid[5,1,5].cube.transform.right, tempAngle);
				grid[5,2,5].cube.transform.RotateAround(tempPoint, grid[5,2,5].cube.transform.right, tempAngle);
				
				tempAngle = Mathf.Sin((finishTimer - 60) / 10.0f);
				tempPoint = grid[0,0,6].cube.transform.position * 0.5f + grid[0,1,5].cube.transform.position * 0.5f;
				
				grid[0,0,6].cube.transform.RotateAround(tempPoint, grid[0,0,6].cube.transform.right, tempAngle);
				grid[0,0,7].cube.transform.RotateAround(tempPoint, grid[0,0,7].cube.transform.right, tempAngle);
				grid[1,0,6].cube.transform.RotateAround(tempPoint, grid[1,0,6].cube.transform.right, tempAngle);
				
				tempPoint = grid[5,0,6].cube.transform.position * 0.5f + grid[5,1,5].cube.transform.position * 0.5f;
				
				grid[4,0,6].cube.transform.RotateAround(tempPoint, grid[4,0,6].cube.transform.right, tempAngle);
				grid[5,0,6].cube.transform.RotateAround(tempPoint, grid[5,0,6].cube.transform.right, tempAngle);
				grid[5,0,7].cube.transform.RotateAround(tempPoint, grid[5,0,7].cube.transform.right, tempAngle);
			}

			tempPoint = grid[2,2,4].cube.transform.position * 0.5f + grid[3,3,4].cube.transform.position * 0.5f;
			tempAngle = 0.05f * Mathf.Sin(finishTimer / 30.0f - Mathf.PI / 2);

			for (int x= 1; x< 5; x++)
			{
				for (int y= 2; y< 5; y++)
				{
					for (int z= 5; z< 8; z++)
					{
						if(grid[x,y,z].cube == null)
							continue;

						Vector3 tempV = grid[x,y,z].cube.transform.up * 0.2f + grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.forward * 0.2f;
						grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);

						if(z > 5)
						{
							tempV = grid[x,y,z].cube.transform.up * 0.2f + grid[x,y,z].cube.transform.right + grid[x,y,z].cube.transform.forward * 0.2f;
							grid[x,y,z].cube.transform.RotateAround(tempPoint, tempV, tempAngle);
						}
					}
				}
			}
			
			return;
		}

		if(tempLevel == 40)
		{
			float tempAngle = - 0.01f * Mathf.Sin(finishTimer / 50.0f - Mathf.PI / 2);
			
			grid[1,0,5].cube.transform.Translate(0,0,tempAngle);
			grid[1,1,5].cube.transform.Translate(0,0,tempAngle / 2.0f);

			grid[1,0,1].cube.transform.Translate(0,0,-tempAngle);
			grid[1,1,1].cube.transform.Translate(0,0,-tempAngle / 2.0f);

			grid[3,0,4].cube.transform.Translate(0,0,-tempAngle);
			grid[3,1,4].cube.transform.Translate(0,0,-tempAngle / 2.0f);

			grid[3,0,1].cube.transform.Translate(0,0,tempAngle);
			grid[3,1,1].cube.transform.Translate(0,0,tempAngle / 2.0f);


			Vector3 tempPoint = grid[1,4,5].cube.transform.position * 0.5f + grid[2,4,5].cube.transform.position * 0.5f;
			tempAngle = - 0.3f * Mathf.Sin(finishTimer / 60.0f - Mathf.PI / 2);
			
			grid[0,3,5].cube.transform.RotateAround(tempPoint, grid[0,3,5].cube.transform.up, -tempAngle);
			grid[0,4,5].cube.transform.RotateAround(tempPoint, grid[0,4,5].cube.transform.up, -tempAngle);
			grid[1,3,5].cube.transform.RotateAround(tempPoint, grid[1,3,5].cube.transform.up, -tempAngle);
			grid[1,4,5].cube.transform.RotateAround(tempPoint, grid[1,4,5].cube.transform.up, -tempAngle);
			grid[1,5,5].cube.transform.RotateAround(tempPoint, grid[1,5,5].cube.transform.up, -tempAngle);

			tempPoint = grid[0,4,5].cube.transform.position * 0.5f + grid[1,4,5].cube.transform.position * 0.5f;
			
			grid[0,3,5].cube.transform.RotateAround(tempPoint, grid[0,3,5].cube.transform.up, -0.5f * tempAngle);
			grid[0,4,5].cube.transform.RotateAround(tempPoint, grid[0,4,5].cube.transform.up, -0.5f * tempAngle);

			tempPoint = grid[2,4,5].cube.transform.position * 0.5f + grid[3,4,5].cube.transform.position * 0.5f;
			
			grid[3,3,5].cube.transform.RotateAround(tempPoint, grid[3,3,5].cube.transform.up, tempAngle);
			grid[3,4,5].cube.transform.RotateAround(tempPoint, grid[3,4,5].cube.transform.up, tempAngle);
			grid[3,5,5].cube.transform.RotateAround(tempPoint, grid[3,5,5].cube.transform.up, tempAngle);
			grid[4,3,5].cube.transform.RotateAround(tempPoint, grid[4,3,5].cube.transform.up, tempAngle);
			grid[4,4,5].cube.transform.RotateAround(tempPoint, grid[4,4,5].cube.transform.up, tempAngle);

			tempPoint = grid[3,4,5].cube.transform.position * 0.5f + grid[4,4,5].cube.transform.position * 0.5f;

			grid[4,3,5].cube.transform.RotateAround(tempPoint, grid[4,3,5].cube.transform.up, 0.5f * tempAngle);
			grid[4,4,5].cube.transform.RotateAround(tempPoint, grid[4,4,5].cube.transform.up, 0.5f * tempAngle);

			tempPoint = grid[2,2,0].cube.transform.position;
			tempAngle = Mathf.Sin(finishTimer / 40.0f - Mathf.PI / 2);
			
			grid[2,1,0].cube.transform.RotateAround(tempPoint, grid[2,1,0].cube.transform.forward, tempAngle);
			grid[2,2,0].cube.transform.RotateAround(tempPoint, grid[2,2,0].cube.transform.forward, tempAngle);


			tempPoint = grid[2,3,5].cube.transform.position * 0.5f + grid[2,3,6].cube.transform.position * 0.5f;
			tempAngle = 0.2f * Mathf.Sin(finishTimer / 40.0f - Mathf.PI / 2);
			
			grid[2,0,8].cube.transform.RotateAround(tempPoint, grid[2,0,8].cube.transform.right, tempAngle);
			grid[2,1,8].cube.transform.RotateAround(tempPoint, grid[2,1,8].cube.transform.right, tempAngle);
			grid[2,2,6].cube.transform.RotateAround(tempPoint, grid[2,2,6].cube.transform.right, tempAngle);
			grid[2,2,8].cube.transform.RotateAround(tempPoint, grid[2,2,8].cube.transform.right, tempAngle);
			grid[2,3,6].cube.transform.RotateAround(tempPoint, grid[2,3,6].cube.transform.right, tempAngle);
			grid[2,3,7].cube.transform.RotateAround(tempPoint, grid[2,3,6].cube.transform.right, tempAngle);
			grid[2,3,8].cube.transform.RotateAround(tempPoint, grid[2,3,8].cube.transform.right, tempAngle);
			grid[2,4,6].cube.transform.RotateAround(tempPoint, grid[2,4,6].cube.transform.right, tempAngle);
			grid[2,4,7].cube.transform.RotateAround(tempPoint, grid[2,4,7].cube.transform.right, tempAngle);

			tempPoint = grid[2,3,6].cube.transform.position * 0.5f + grid[2,4,7].cube.transform.position * 0.5f;
			tempAngle = 0.1f * Mathf.Sin(finishTimer / 40.0f - Mathf.PI / 2);
			
			grid[2,0,8].cube.transform.RotateAround(tempPoint, grid[2,0,8].cube.transform.right, tempAngle);
			grid[2,1,8].cube.transform.RotateAround(tempPoint, grid[2,1,8].cube.transform.right, tempAngle);
			grid[2,2,8].cube.transform.RotateAround(tempPoint, grid[2,2,8].cube.transform.right, tempAngle);
			grid[2,3,7].cube.transform.RotateAround(tempPoint, grid[2,3,6].cube.transform.right, tempAngle);
			grid[2,3,8].cube.transform.RotateAround(tempPoint, grid[2,3,8].cube.transform.right, tempAngle);
			grid[2,4,7].cube.transform.RotateAround(tempPoint, grid[2,4,7].cube.transform.right, tempAngle);

			tempPoint = grid[2,3,8].cube.transform.position * 0.5f + grid[2,4,7].cube.transform.position * 0.5f;
			tempAngle = 0.3f * Mathf.Sin(finishTimer / 50.0f - Mathf.PI / 2);
			
			grid[2,0,8].cube.transform.RotateAround(tempPoint, grid[2,0,8].cube.transform.right, tempAngle);
			grid[2,1,8].cube.transform.RotateAround(tempPoint, grid[2,1,8].cube.transform.right, tempAngle);
			grid[2,2,8].cube.transform.RotateAround(tempPoint, grid[2,2,8].cube.transform.right, tempAngle);
			grid[2,3,8].cube.transform.RotateAround(tempPoint, grid[2,3,8].cube.transform.right, tempAngle);

			cubesParent.transform.rotation = Quaternion.AngleAxis(2.0f * Mathf.Sin(finishTimer / 60.0f), new Vector3(0.5f,0.1f,1.0f));
			
			return;
		}

		if(tempLevel == 41)
		{
			
			
			return;
		}

		if(tempLevel == 42)
		{
			
			
			return;
		}

		if(tempLevel == 43)
		{
			
			
			return;
		}

		if(tempLevel == 44)
		{
			
			
			return;
		}

		if(tempLevel == 45)
		{
			
			
			return;
		}

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
		if(buttonsEdit[6].activeSelf == true)
		{
			//Destroy(creativeCubes[i].cube);
			//creativeCubes.Remove(creativeCubes[i]);
			foreach(Cube cube in creativeCubes)
			{
				Destroy(cube.cube);
			}
			creativeCubes.Clear();

			customStageCount++;
			saveLevel (customStageCount);
			animations.Clear();
			currentAnimation = null;

			pauseExit ();
		}
		else
		{
			convertEditToLevel ();

			buttonsEdit[0].SetActive(false);
			buttonsEdit[1].SetActive(false);
			buttonsEdit[2].SetActive(false);

			buttonsEdit[5].SetActive(true);
			buttonsEdit[6].SetActive(true);

			currentAnimation = new levelAnimation();

			breaking = true;
			creativeColoring = false;
		}

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

	void buttonEditArrow()
	{
		isRotation = false;

		buttonsEdit [5].guiTexture.color = new Color (0.8f, 0.8f, 0.8f, 1);
		buttonsEdit [6].guiTexture.color = new Color (0, 0, 0, 1);
	}

	void buttonEditRotate()
	{
		isRotation = true;

		buttonsEdit [5].guiTexture.color = new Color (0, 0, 0, 1);
		buttonsEdit [6].guiTexture.color = new Color (0.8f, 0.8f, 0.8f, 1);
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

				file.WriteLine(colorToHex(cube.cube.renderer.material.color));

				/*
				file.WriteLine(cube.cube.renderer.material.color.r);
				file.WriteLine(cube.cube.renderer.material.color.g);
				file.WriteLine(cube.cube.renderer.material.color.b);
				*/
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

		buttonsEdit [5].SetActive (false);
		buttonsEdit [6].SetActive (false);
		
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
							//Destroy(creativeCubes[i].cube);
							//creativeCubes.Remove(creativeCubes[i]);
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

	string colorToHex(Color32 color)
	{
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}
	
	Color hexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		
		return new Color (r / 255.0f, g / 255.0f, b / 255.0f);
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
					
				file.WriteLine(colorToHex(cube.color));

				/*
				file.WriteLine(cube.cube.renderer.material.color.r);
				file.WriteLine(cube.cube.renderer.material.color.g);
				file.WriteLine(cube.cube.renderer.material.color.b);
				*/
			}
				
			file.Close ();	

			file = new System.IO.StreamWriter(Application.persistentDataPath + "/custom" + level + "animation.txt");
			
			file.WriteLine (animations.Count);

			foreach(levelAnimation animation in animations)
			{
				file.WriteLine(animation.cubes.Count);

				foreach(Cube cube in animation.cubes)
				{
					for (int x=0; x<levelArray.GetLength(0); x++)
					{
						for (int y=0; y<levelArray.GetLength(1); y++)
						{
							for (int z=0; z<levelArray.GetLength(2); z++)
							{
								if(grid[x,y,z] == null)
									continue;

								if(grid[x,y,z].Equals(cube))
								{
									file.WriteLine(x);
									file.WriteLine(y);
									file.WriteLine(z);

									break;
								}
							}
						}
					}
				}

				file.WriteLine(animation.origin.x);
				file.WriteLine(animation.origin.y);
				file.WriteLine(animation.origin.z);

				file.WriteLine(animation.translation.x);
				file.WriteLine(animation.translation.y);
				file.WriteLine(animation.translation.z);

				file.WriteLine(animation.angleRight / animation.cubes.Count);
				file.WriteLine(animation.angleForward / animation.cubes.Count);
				file.WriteLine(animation.angleUp / animation.cubes.Count);
			}
			
			file.Close ();

			//TEST
			/*
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
			*/

		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}

	void saveProgress()
	{
		try
		{
			System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath + "/progress.txt");

			//file.Write (progress);
			for(int i = 0; i < 45; i++)
			{
				file.WriteLine(scores[i].ToString());
			}

			file.Close();
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}

	void loadProgress()
	{
		try
		{
			System.IO.StreamReader file = new System.IO.StreamReader (Application.persistentDataPath + "/progress.txt");

			if (file == null)
			{
				saveProgress ();
				return;
			}

			//progress = ushort.Parse (file.ReadToEnd ());
			progress = 0;

			for(int i = 0; i < 45; i++)
			{
				scores[i] = ushort.Parse(file.ReadLine());

				if(scores[i] > 0)
				{
					progress++;
				}
			}

			file.Close ();
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			saveProgress();
		}

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

public class levelAnimation
{
	public List<Cube> cubes = new List<Cube>();
	public Vector3 origin;
	public float angleRight;
	public float angleForward;
	public float angleUp;
	public Vector3 translation;
}
