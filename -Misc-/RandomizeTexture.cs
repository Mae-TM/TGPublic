using UnityEngine;

public class RandomizeTexture : MonoBehaviour
{
	public string assetBundle;

	private AssetBundle specificBundle;

	private static Material[] materialList;

	private void Awake()
	{
		MeshRenderer[] componentsInChildren;
		if (materialList == null)
		{
			if (!AssetBundleExtensions.TryLoad("textures/" + assetBundle, out specificBundle))
			{
				return;
			}
			Texture2D[] array = specificBundle.LoadAllAssets<Texture2D>();
			Material source = new Material(Shader.Find("Toon/Lit"));
			componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				source = new Material(componentsInChildren[i].sharedMaterials[0]);
			}
			materialList = new Material[array.Length];
			int num = 0;
			Texture2D[] array2 = array;
			foreach (Texture2D mainTexture in array2)
			{
				Material material = new Material(source);
				material.mainTexture = mainTexture;
				materialList[num] = material;
				num++;
			}
			specificBundle.Unload(unloadAllLoadedObjects: false);
		}
		Material material2 = materialList[Random.Range(0, materialList.Length)];
		componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer obj in componentsInChildren)
		{
			Material[] sharedMaterials = obj.sharedMaterials;
			sharedMaterials[0] = material2;
			obj.sharedMaterials = sharedMaterials;
		}
	}

	private void Update()
	{
	}
}
