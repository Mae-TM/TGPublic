using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MediumMap : MonoBehaviour
{
	private enum Mode
	{
		System,
		Planet,
		ShiftingToPlanet,
		ShiftingFromPlanet
	}

	[SerializeField]
	private Text planetText;

	public Camera cam;

	public int movementFrames = 10;

	public int clickTime = 10;

	public GameObject transportalizer;

	public PlayerMapMarker mapMarker;

	public int transportalizersPerPlanet = 10;

	public bool transportalizerMode;

	private Vector2 prevPosition;

	private Mode mode;

	private List<GameObject> planets;

	private Transform currentPlanet;

	private int frame;

	private Vector3 startPosition;

	private Vector3 endPosition;

	private Quaternion startOrientation;

	private Quaternion endOrientation;

	private int clickStartTime;

	private bool mouseHeld;

	public static Vector3 TransformPosition(Transform transform, Transform planet)
	{
		return planet.TransformPoint(2f * transform.root.InverseTransformPoint(transform.position));
	}

	public void AddPlanet(int id, string name, Material material)
	{
		if (planets == null)
		{
			InitPlanets();
		}
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		material = new Material(material);
		material.SetInt("_Displace", 1);
		gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
		gameObject.name = name;
		gameObject.transform.parent = base.transform;
		planets.Add(gameObject);
		for (int i = 1; i < planets.Count; i++)
		{
			planets[i].transform.localPosition = 3f * new Vector3(Mathf.Cos((float)(i - 1) * 2f * (float)Math.PI / (float)(planets.Count - 1)), Mathf.Sin((float)(i - 1) * 2f * (float)Math.PI / (float)(planets.Count - 1)), 0f);
		}
	}

	public GameObject AddTransportalizer(Transform source)
	{
		Transform transform = base.transform.Find(source.root.name);
		if (transform == null)
		{
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(transportalizer, TransformPosition(source, transform), transform.rotation * source.rotation, transform);
		MapTransportalizer mapTransportalizer = obj.AddComponent<MapTransportalizer>();
		mapTransportalizer.map = this;
		mapTransportalizer.original = source;
		obj.SetActive(value: true);
		return obj;
	}

	public GameObject AddStaticObject(GameObject realObject, string planet, bool big = true)
	{
		Transform transform = base.transform.Find(planet);
		if (transform == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(realObject, TransformPosition(realObject.transform, transform), transform.rotation * realObject.transform.rotation, transform);
		gameObject.transform.localScale = realObject.transform.lossyScale / realObject.transform.root.localScale.x * (big ? 25f : 10f);
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(componentsInChildren[num]);
		}
		return gameObject;
	}

	private void OnEnable()
	{
		if (cam == null)
		{
			cam = MSPAOrthoController.main;
		}
		if (mode == Mode.System || mode == Mode.ShiftingFromPlanet)
		{
			cam.transform.position = base.transform.position + cam.transform.lossyScale.z * new Vector3(0f, 0f, -7f);
			if (planetText != null)
			{
				planetText.text = "";
			}
		}
		else
		{
			cam.transform.position = currentPlanet.transform.position + cam.transform.lossyScale.z * new Vector3(0f, 0f, -1.5f);
			if (planetText != null)
			{
				planetText.text = currentPlanet.name;
			}
		}
		cam.transform.rotation = Quaternion.identity;
	}

	private void OnDisable()
	{
		transportalizerMode = false;
	}

	private int getPlanetClicked()
	{
		RaycastHit[] array = Physics.RaycastAll(cam.ScreenPointToRay(Input.mousePosition));
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < planets.Count; j++)
			{
				if (array[i].collider.gameObject == planets[j])
				{
					return j;
				}
			}
		}
		return -1;
	}

	private bool isCurrentPlanetClicked()
	{
		RaycastHit[] array = Physics.RaycastAll(cam.ScreenPointToRay(Input.mousePosition));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject.transform == currentPlanet)
			{
				return true;
			}
		}
		return false;
	}

	private Vector2 toUsefulCoordinates(Vector3 pixelCoordinates)
	{
		return new Vector2(pixelCoordinates.x - (float)(Screen.width / 2), pixelCoordinates.y - (float)(Screen.height / 2)) / Screen.height;
	}

	private void rotatePlanet(Vector2 prevPosition, Vector2 newPosition)
	{
		if (!(prevPosition == newPosition))
		{
			Vector2 vector = (newPosition - prevPosition) * cam.fieldOfView * Screen.width / Screen.height;
			currentPlanet.localRotation = Quaternion.Euler(vector.y, 0f - vector.x, 0f) * currentPlanet.localRotation;
		}
	}

	private void rotatePlanet2(Vector2 prevPosition, Vector2 newPosition)
	{
		if (!(prevPosition == newPosition))
		{
			Vector2 vector = (newPosition - prevPosition) * 180f / (float)Math.PI;
			float num = prevPosition.x * vector.y - prevPosition.y * vector.x;
			currentPlanet.localRotation = Quaternion.Euler(0f, 0f, num / prevPosition.sqrMagnitude) * currentPlanet.localRotation;
		}
	}

	private void Start()
	{
		if (planets == null)
		{
			InitPlanets();
		}
	}

	private void InitPlanets()
	{
		planets = new List<GameObject>();
		foreach (Transform item in base.transform)
		{
			if (!(item.gameObject == transportalizer) && !(item == cam.transform) && !(item.GetComponent<PlayerMapMarker>() != null))
			{
				planets.Add(item.gameObject);
				for (int i = 0; i < transportalizersPerPlanet; i++)
				{
					GameObject obj = UnityEngine.Object.Instantiate(transportalizer, item);
					Vector3 vector = UnityEngine.Random.onUnitSphere / 2f;
					obj.transform.localPosition = vector;
					obj.transform.localRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, vector), vector);
					obj.SetActive(value: true);
				}
			}
		}
	}

	private void Update()
	{
		switch (mode)
		{
		case Mode.Planet:
			if (Input.GetMouseButton(0))
			{
				if (mouseHeld)
				{
					Vector2 newPosition = toUsefulCoordinates(Input.mousePosition);
					rotatePlanet(prevPosition, newPosition);
					prevPosition = newPosition;
				}
				else
				{
					prevPosition = toUsefulCoordinates(Input.mousePosition);
					clickStartTime = Time.frameCount;
					mouseHeld = true;
				}
			}
			if (Input.GetMouseButton(1))
			{
				if (mouseHeld)
				{
					Vector2 newPosition2 = toUsefulCoordinates(Input.mousePosition);
					rotatePlanet2(prevPosition, newPosition2);
					prevPosition = newPosition2;
				}
				else
				{
					prevPosition = toUsefulCoordinates(Input.mousePosition);
					clickStartTime = Time.frameCount;
					mouseHeld = true;
				}
			}
			if (Input.GetMouseButtonUp(0))
			{
				mouseHeld = false;
				if (Time.frameCount - clickStartTime < clickTime && !isCurrentPlanetClicked())
				{
					startPosition = cam.transform.position - currentPlanet.position;
					endPosition = base.transform.position + base.transform.lossyScale.z * new Vector3(0f, 0f, -7f) - currentPlanet.position;
					startOrientation = currentPlanet.localRotation;
					endOrientation = Quaternion.identity;
					mode = Mode.ShiftingFromPlanet;
					if (planetText != null)
					{
						planetText.text = "";
					}
				}
			}
			if (Input.GetMouseButtonUp(1))
			{
				mouseHeld = false;
			}
			break;
		case Mode.System:
		{
			if (!Input.GetMouseButtonUp(0))
			{
				break;
			}
			int planetClicked = getPlanetClicked();
			if (planetClicked != -1)
			{
				currentPlanet = planets[planetClicked].transform;
				startPosition = cam.transform.position - currentPlanet.position;
				endPosition = Vector3.Scale(new Vector3(0f, 0f, -1.5f), base.transform.lossyScale);
				mode = Mode.ShiftingToPlanet;
				if (planetText != null)
				{
					planetText.text = currentPlanet.name;
				}
			}
			break;
		}
		case Mode.ShiftingToPlanet:
			if (frame < movementFrames)
			{
				frame++;
				float num2 = (float)frame * 1f / (float)movementFrames;
				float t2 = 3f * num2 * num2 - 2f * num2 * num2 * num2;
				cam.transform.position = Vector3.Slerp(startPosition, endPosition, t2) + currentPlanet.position;
			}
			else
			{
				cam.transform.position = endPosition + currentPlanet.position;
				frame = 0;
				mode = Mode.Planet;
			}
			break;
		case Mode.ShiftingFromPlanet:
			if (frame < movementFrames)
			{
				frame++;
				float num = (float)frame * 1f / (float)movementFrames;
				float t = 3f * num * num - 2f * num * num * num;
				cam.transform.position = Vector3.Slerp(startPosition, endPosition, t) + currentPlanet.position;
				currentPlanet.localRotation = Quaternion.Slerp(startOrientation, endOrientation, t);
			}
			else
			{
				cam.transform.position = endPosition + currentPlanet.position;
				cam.transform.localRotation = endOrientation;
				frame = 0;
				mode = Mode.System;
			}
			break;
		}
	}

	public void SetPlanet(string planet)
	{
		currentPlanet = base.transform.Find(planet);
		if (planetText != null)
		{
			planetText.text = planet;
		}
		cam.transform.position = Vector3.Scale(new Vector3(0f, 0f, -1.5f), base.transform.lossyScale) + currentPlanet.position;
		frame = 0;
		mode = Mode.Planet;
	}
}
