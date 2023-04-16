using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
	private float standardWallThickness = 0.34f;

	private Vector3[] wallDirections = new Vector3[4]
	{
		Vector3.left,
		Vector3.right,
		Vector3.forward,
		Vector3.back
	};

	private Quaternion[] wallRots = new Quaternion[4]
	{
		Quaternion.Euler(0f, 0f, 90f),
		Quaternion.Euler(0f, 0f, -90f),
		Quaternion.Euler(90f, 0f, 0f),
		Quaternion.Euler(-90f, 0f, 0f)
	};

	private int[] moveIndex = new int[4] { 0, 0, 2, 2 };

	private bool[] expand = new bool[4] { true, true, false, false };

	private int[,] wallDimensions = new int[4, 2]
	{
		{ 2, 1 },
		{ 2, 1 },
		{ 1, 0 },
		{ 1, 0 }
	};

	private void Start()
	{
	}

	private void GenerateFloor(float[] dimensions, GameObject room)
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = room.transform;
		obj.name = "floor";
		obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
		obj.transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	private void GenerateFloor(float[] dimensions, float wallThickness, GameObject room)
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = room.transform;
		obj.name = "floor";
		obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
		obj.transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	public GameObject GenerateRoom(float[] dimensions)
	{
		GameObject gameObject = new GameObject("Room");
		GenerateFloor(dimensions, gameObject);
		for (int i = 0; i < 4; i++)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "wall" + (i + 1);
			if (expand[i])
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[i, 0]] + 2f * standardWallThickness, standardWallThickness, dimensions[wallDimensions[i, 1]]);
			}
			else
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[i, 0]], standardWallThickness, dimensions[wallDimensions[i, 1]]);
			}
			gameObject2.transform.localPosition = wallDirections[i] * ((dimensions[moveIndex[i]] + standardWallThickness) / 2f);
			gameObject2.transform.Translate(new Vector3(0f, dimensions[1] / 2f, 0f));
			gameObject2.transform.localRotation = wallRots[i];
			gameObject2.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = gameObject.transform;
		obj.name = "ceiling";
		obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
		obj.transform.localPosition = new Vector3(0f, dimensions[1] + standardWallThickness / 2f, 0f);
		gameObject.AddComponent<OldRoom>();
		return gameObject;
	}

	public GameObject GenerateRoom(float[] dimensions, int[] whichWalls)
	{
		GameObject gameObject = new GameObject("Room");
		GenerateFloor(dimensions, gameObject);
		for (int i = 0; i < whichWalls.Length; i++)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "wall" + (i + 1);
			if (expand[whichWalls[i]])
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]] + 2f * standardWallThickness, standardWallThickness, dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			else
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]], standardWallThickness, dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			gameObject2.transform.localPosition = wallDirections[whichWalls[i]] * ((dimensions[moveIndex[whichWalls[i]]] + standardWallThickness) / 2f);
			gameObject2.transform.Translate(new Vector3(0f, dimensions[whichWalls[i]] / 2f, 0f));
			gameObject2.transform.localRotation = wallRots[whichWalls[i]];
			gameObject2.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = gameObject.transform;
		obj.name = "ceiling";
		obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
		obj.transform.localPosition = new Vector3(0f, dimensions[1] + standardWallThickness / 2f, 0f);
		gameObject.AddComponent<OldRoom>();
		return gameObject;
	}

	public GameObject GenerateRoom(float[] dimensions, float[] wallThicknesses)
	{
		GameObject gameObject = new GameObject("Room");
		GenerateFloor(dimensions, gameObject);
		for (int i = 0; i < 4; i++)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "wall" + (i + 1);
			if (expand[i])
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[i, 0]] + 2f * wallThicknesses[i], wallThicknesses[i], dimensions[wallDimensions[i, 1]]);
			}
			else
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[i, 0]], wallThicknesses[i], dimensions[wallDimensions[i, 1]]);
			}
			gameObject2.transform.localPosition = wallDirections[i] * ((dimensions[moveIndex[i]] + wallThicknesses[i]) / 2f);
			gameObject2.transform.Translate(new Vector3(0f, dimensions[i] / 2f, 0f));
			gameObject2.transform.localRotation = wallRots[i];
			gameObject2.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = gameObject.transform;
		obj.name = "ceiling";
		obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
		obj.transform.localPosition = new Vector3(0f, dimensions[1] + standardWallThickness / 2f, 0f);
		gameObject.AddComponent<OldRoom>();
		return gameObject;
	}

	public GameObject GenerateRoom(float[] dimensions, int[] whichWalls, float[] wallThicknesses)
	{
		GameObject gameObject = new GameObject("Room");
		GenerateFloor(dimensions, gameObject);
		for (int i = 0; i < whichWalls.Length; i++)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "wall" + (i + 1);
			if (expand[whichWalls[i]])
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]] + 2f * wallThicknesses[i], wallThicknesses[i], dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			else
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]], wallThicknesses[i], dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			gameObject2.transform.localPosition = wallDirections[whichWalls[i]] * ((dimensions[moveIndex[whichWalls[i]]] + wallThicknesses[i]) / 2f);
			gameObject2.transform.Translate(new Vector3(0f, dimensions[whichWalls[i]] / 2f, 0f));
			gameObject2.transform.localRotation = wallRots[whichWalls[i]];
			gameObject2.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = gameObject.transform;
		obj.name = "ceiling";
		obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
		obj.transform.localPosition = new Vector3(0f, dimensions[1] + standardWallThickness / 2f, 0f);
		gameObject.AddComponent<OldRoom>();
		return gameObject;
	}

	public GameObject GenerateRoom(float[] dimensions, int[] whichWalls, float wallThickness)
	{
		GameObject gameObject = new GameObject("Room");
		GenerateFloor(dimensions, wallThickness, gameObject);
		for (int i = 0; i < whichWalls.Length; i++)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "wall" + (i + 1);
			if (expand[whichWalls[i]])
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]] + 2f * wallThickness, wallThickness, dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			else
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]], wallThickness, dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			gameObject2.transform.localPosition = wallDirections[whichWalls[i]] * ((dimensions[moveIndex[whichWalls[i]]] + wallThickness) / 2f);
			gameObject2.transform.Translate(new Vector3(0f, dimensions[whichWalls[i]] / 2f, 0f));
			gameObject2.transform.localRotation = wallRots[whichWalls[i]];
			gameObject2.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = gameObject.transform;
		obj.name = "ceiling";
		obj.transform.localScale = new Vector3(dimensions[0] + 2f * wallThickness, wallThickness, dimensions[2] + 2f * wallThickness);
		obj.transform.localPosition = new Vector3(0f, dimensions[1] + wallThickness / 2f, 0f);
		gameObject.AddComponent<OldRoom>();
		return gameObject;
	}

	public GameObject GenerateRoom(float[] dimensions, bool generateFloor, bool generateCeiling)
	{
		GameObject gameObject = new GameObject("Room");
		if (generateFloor)
		{
			GenerateFloor(dimensions, gameObject);
		}
		for (int i = 0; i < 4; i++)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "wall" + (i + 1);
			if (expand[i])
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[i, 0]] + 2f * standardWallThickness, standardWallThickness, dimensions[wallDimensions[i, 1]]);
			}
			else
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[i, 0]], standardWallThickness, dimensions[wallDimensions[i, 1]]);
			}
			gameObject2.transform.localPosition = wallDirections[i] * ((dimensions[moveIndex[i]] + standardWallThickness) / 2f);
			gameObject2.transform.Translate(new Vector3(0f, dimensions[i] / 2f, 0f));
			gameObject2.transform.localRotation = wallRots[i];
			gameObject2.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
		if (generateCeiling)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj.transform.parent = gameObject.transform;
			obj.name = "ceiling";
			obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
			obj.transform.localPosition = new Vector3(0f, dimensions[1] + standardWallThickness / 2f, 0f);
		}
		gameObject.AddComponent<OldRoom>();
		return gameObject;
	}

	public GameObject GenerateRoom(float[] dimensions, int[] whichWalls, bool generateFloor, bool generateCeiling)
	{
		GameObject gameObject = new GameObject("Room");
		if (generateFloor)
		{
			GenerateFloor(dimensions, gameObject);
		}
		for (int i = 0; i < whichWalls.Length; i++)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "wall" + (i + 1);
			if (expand[whichWalls[i]])
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]] + 2f * standardWallThickness, standardWallThickness, dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			else
			{
				gameObject2.transform.localScale = new Vector3(dimensions[wallDimensions[whichWalls[i], 0]], standardWallThickness, dimensions[wallDimensions[whichWalls[i], 1]]);
			}
			gameObject2.transform.localPosition = wallDirections[whichWalls[i]] * ((dimensions[moveIndex[whichWalls[i]]] + standardWallThickness) / 2f);
			gameObject2.transform.Translate(new Vector3(0f, dimensions[whichWalls[i]] / 2f, 0f));
			gameObject2.transform.localRotation = wallRots[whichWalls[i]];
			gameObject2.transform.Rotate(new Vector3(0f, 90f, 0f));
		}
		if (generateCeiling)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj.transform.parent = gameObject.transform;
			obj.name = "ceiling";
			obj.transform.localScale = new Vector3(dimensions[0] + 2f * standardWallThickness, standardWallThickness, dimensions[2] + 2f * standardWallThickness);
			obj.transform.localPosition = new Vector3(0f, dimensions[1] + standardWallThickness / 2f, 0f);
		}
		gameObject.AddComponent<OldRoom>();
		return gameObject;
	}
}
