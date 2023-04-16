using System;
using UnityEngine;

public class AnimateFurnitureMaterial : MonoBehaviour
{
	private readonly int oceanOffsetID = Shader.PropertyToID("_OceanOffset");

	private float oceanOffset;

	public float animSpeed;

	private Material animatedMaterial;

	private void Start()
	{
		Material[] sharedMaterials = base.gameObject.GetComponent<MeshRenderer>().sharedMaterials;
		int num = Array.FindIndex(sharedMaterials, (Material m) => m.shader.name == "Noise/SeaShader");
		if (num != -1)
		{
			animatedMaterial = sharedMaterials[num];
		}
	}

	private void Update()
	{
		if (animSpeed != 0f)
		{
			oceanOffset += animSpeed * Time.deltaTime;
			animatedMaterial.SetVector(oceanOffsetID, Vector3.up * oceanOffset);
		}
	}
}
