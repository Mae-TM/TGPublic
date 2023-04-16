using UnityEngine;

public class Probe : MonoBehaviour
{
	public GameObject end;

	public DisplacementMap3 displacementMap;

	public bool test;

	public bool iterate;

	private void Start()
	{
	}

	private void Update()
	{
		if (test)
		{
			Vector3 vector = base.transform.localPosition.normalized / 2f;
			base.transform.localPosition = vector * displacementMap.getHeight(vector);
			Vector3 vector2 = displacementMap.gradTest(vector);
			MonoBehaviour.print(vector2);
			vector += vector2;
			end.transform.localPosition = vector.normalized / 2f * displacementMap.getHeight(vector);
			test = false;
		}
		if (iterate)
		{
			base.transform.position = end.transform.position;
			iterate = false;
		}
	}
}
