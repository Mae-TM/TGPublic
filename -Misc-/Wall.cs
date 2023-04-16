using UnityEngine;

public class Wall : MonoBehaviour
{
	public static Transform cam;

	private static Transform target;

	public static bool transparent;

	public static Material alphaMaterial;

	private MeshRenderer renderer;

	private bool horizontal;

	private Vector3 center;

	private void Awake()
	{
		renderer = GetComponent<MeshRenderer>();
	}

	private void Start()
	{
		Vector3 extents = renderer.bounds.extents;
		horizontal = extents.x > extents.z;
		center = renderer.bounds.center;
	}

	public void SetVisible(Material normalMaterial)
	{
		renderer.sharedMaterial = normalMaterial;
		renderer.gameObject.layer = 0;
	}

	public void UpdateVisibility(Material normalMaterial)
	{
		Vector3 position = target.position;
		Vector3 forward = cam.forward;
		if ((horizontal ? ((position.z - center.z) * forward.z) : ((position.x - center.x) * forward.x)) > 0f)
		{
			if (transparent)
			{
				renderer.sharedMaterial = alphaMaterial;
				renderer.gameObject.layer = 2;
			}
			else
			{
				renderer.enabled = false;
			}
		}
		else if (renderer.enabled)
		{
			renderer.sharedMaterial = normalMaterial;
			renderer.gameObject.layer = 0;
		}
		else
		{
			renderer.enabled = true;
		}
	}

	public static void SetCam(Camera camera, bool newTransparent)
	{
		cam = camera.transform;
		target = camera.GetComponent<MSPAOrthoController>().Target;
		transparent = newTransparent;
	}
}
