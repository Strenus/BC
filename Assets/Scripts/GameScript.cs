using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	public Cube[, ,] grid;

	public bool[, ,] array3Da = new bool[2, 2, 3] { { { false, true, false }, { true, false, true } }, 
		{ { false, true, false }, { true, false, true } } };

	List<GameObject> fragments = new List<GameObject>();

	List<GameObject> buttonsMenu = new List<GameObject>();
	List<GameObject> buttonsIngame = new List<GameObject>();

	ushort ticks = 0;
	bool breaking = true;
	bool pause = true;

	void Start () 
	{
		DontDestroyOnLoad(this.gameObject);

		buttonsMenu.AddRange(GameObject.FindGameObjectsWithTag("button"));

		buttonsIngame.Add(GameObject.Find("buttonHammer"));
		buttonsIngame.Add(GameObject.Find("buttonBrush"));

		foreach(GameObject go in buttonsIngame)
		{
			go.SetActive(false);
		}

		//SpawnCubes ();

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
					completed();
				}
			}
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
		grid = new Cube[array3Da.GetLength (0), array3Da.GetLength (1), array3Da.GetLength (2)];
		
		for (int x=0; x<array3Da.GetLength(0); x++)
		{
			for (int y=0; y<array3Da.GetLength(1); y++)
			{
				for (int z=0; z<array3Da.GetLength(2); z++)
				{
					
					grid[x,y,z] = new Cube();
					grid[x,y,z].cube.transform.position = new Vector3(x - array3Da.GetLength(0)/2.0f + 0.5f,y - array3Da.GetLength(1)/2.0f + 0.5f,z - array3Da.GetLength(2)/2.0f + 0.5f);
					
					sbyte tempCount = 0;
					
					
					for(int i=0;i<array3Da.GetLength(2);i++)
					{
						if(array3Da[x,y,i] == true)
							tempCount++;
					}
					grid[x,y,z].setSide (0, tempCount);
					grid[x,y,z].setSide (2, tempCount);
					
					
					tempCount = 0;
					for(int i=0;i<array3Da.GetLength(0);i++)
					{
						if(array3Da[i,y,z] == true)
							tempCount++;
					}
					grid[x,y,z].setSide (1, tempCount);
					grid[x,y,z].setSide (3, tempCount);
					
					
					tempCount = 0;
					for(int i=0;i<array3Da.GetLength(1);i++)
					{
						if(array3Da[x,i,z] == true)
							tempCount++;
					}
					grid[x,y,z].setSide (4, tempCount);
					grid[x,y,z].setSide (5, tempCount);
					
					
				}
			}
		}
	}

	void touchCube (GameObject cube)
	{
		if (breaking)
		{
			if(cube.renderer.materials[0].name == "cubeBase (Instance)")
				breakCube (cube);
		}
		else
		{
			brushCube(cube);
		}

		checkCompletion ();
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

	void brushCube (GameObject cube)
	{
		if(cube.renderer.materials[0].name == "cubeBase (Instance)")
		{
			Material[] mats = new Material[cube.renderer.materials.Length];

			mats[0] = Resources.Load("Materials/cubeBlue", typeof(Material)) as Material;

			for (int i = 1; i < mats.Length; i++) 
			{
				mats[i] = cube.renderer.materials[i];
			}

			cube.renderer.materials = mats;

		}
		else
		{
			Material[] mats = new Material[cube.renderer.materials.Length];
			
			mats[0] = Resources.Load("Materials/cubeBase", typeof(Material)) as Material;
			
			for (int i = 1; i < mats.Length; i++) 
			{
				mats[i] = cube.renderer.materials[i];
			}
			
			cube.renderer.materials = mats;
		}
	}

	bool checkCompletion()
	{
		for (int x=0; x<array3Da.GetLength(0); x++)
		{
			for (int y=0; y<array3Da.GetLength(1); y++)
			{
				for (int z=0; z<array3Da.GetLength(2); z++)
				{
					if(((grid[x,y,z].cube != null) && (!array3Da[x,y,z])) || ((grid[x,y,z].cube == null) && (array3Da[x,y,z])))
					{
						return false;
					}
				}
			}
		}

		return true;
	}

	void completed()
	{
		foreach (Cube cube in grid) 
		{
			if(cube != null)
				Destroy(cube.cube);
		}
		grid = null;
		pause = true;

		foreach(GameObject button in buttonsMenu)
		{
			button.SetActive(true);
		}

		foreach(GameObject go in buttonsIngame)
		{
			go.SetActive(false);
		}
	}

	void buttonStart()
	{
		foreach(GameObject button in buttonsMenu)
		{
			button.SetActive(false);
		}

		foreach(GameObject go in buttonsIngame)
		{
			go.SetActive(true);
		}

		pause = false;
		SpawnCubes ();
	}

	void buttonHammer()
	{
		breaking = true;
	}

	void buttonBrush()
	{
		breaking = false;
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

	public void setSide(byte side, sbyte num)
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

		Material[] mats = new Material[7];

		for (int i = 0; i < 7; i++) 
		{
			mats[i] = cube.renderer.materials[i];
		}

		if (num == -1)
		{
			mats[side + 1] = new Material(Shader.Find ("Diffuse"));
		}
		if (num == 0)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube0", typeof(Material)) as Material) as Material;
		}
		if (num == 1)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube1", typeof(Material)) as Material) as Material;
		}
		if (num == 2)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube2", typeof(Material)) as Material) as Material;
		}
		if (num == 3)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube3", typeof(Material)) as Material) as Material;
		}
		if (num == 4)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube4", typeof(Material)) as Material) as Material;
		}
		if (num == 5)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube5", typeof(Material)) as Material) as Material;
		}
		if (num == 6)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube6", typeof(Material)) as Material) as Material;
		}
		if (num == 7)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube7", typeof(Material)) as Material) as Material;
		}
		if (num == 8)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube8", typeof(Material)) as Material) as Material;
		}
		if (num == 9)
		{
			mats[side + 1] = GameObject.Instantiate(Resources.Load("Materials/cube9", typeof(Material)) as Material) as Material;
		}

		cube.renderer.materials = mats;
		cube.renderer.materials[side + 1].mainTextureOffset = sideOffset;
	}
}
