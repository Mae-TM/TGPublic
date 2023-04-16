using System;
using UnityEngine;

public class OceanAnimation : MonoBehaviour
{
	private Material oceanMaterial;

	private readonly int oceanOffsetID = Shader.PropertyToID("_OceanOffset");

	private float oceanOffset;

	private void Start()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
			int num = Array.FindIndex(sharedMaterials, (Material m) => m.shader.name == "Noise/SeaShader");
			if (num != -1)
			{
				oceanMaterial = sharedMaterials[num];
			}
		}
	}

	private void Update()
	{
		float num = (float)Math.Cos((DateTime.UtcNow - DateTime.MinValue).TotalSeconds * (double)UnityEngine.Random.Range(10, 50) / 150.0 * (Math.PI * 2.0)) * 5f;
		if (num < 0f)
		{
			num *= -1f;
		}
		oceanOffset += (num * 1E-05f + 0.0002f) * Time.deltaTime;
		oceanMaterial.SetVector(oceanOffsetID, Vector3.up * oceanOffset);
	}
}
