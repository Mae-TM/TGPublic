using UnityEngine;

public class CopyHouseMat : MonoBehaviour
{
	public Renderer renderer;

	public int index;

	private void OnValidate()
	{
		if (!renderer)
		{
			renderer = GetComponentInChildren<Renderer>();
		}
	}

	private void OnTransformParentChanged()
	{
		Room componentInParent = GetComponentInParent<Room>();
		if ((bool)componentInParent)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			sharedMaterials[index] = componentInParent.GetComponent<MeshRenderer>().sharedMaterial;
			renderer.sharedMaterials = sharedMaterials;
		}
	}
}
