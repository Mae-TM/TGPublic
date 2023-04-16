using UnityEngine;

public static class AssetBundleExtensions
{
	public delegate void FallBackFunctionType();

	public static AssetBundle Load(string name)
	{
		TryLoad(name, out var bundle);
		return bundle;
	}

	public static bool TryLoad(string name, out AssetBundle bundle)
	{
		if (!StreamingAssets.TryGetFile("AssetBundles/" + name, out var path))
		{
			bundle = null;
			return false;
		}
		bundle = AssetBundle.LoadFromFile(path);
		return true;
	}

	public static AssetBundleCreateRequest LoadAsync(string name)
	{
		StreamingAssets.TryGetFile("AssetBundles/" + name, out var path);
		return AssetBundle.LoadFromFileAsync(path);
	}

	public static T GetAssetWithFallback<T>(this AssetBundle assetBundle, string name, string fallbackName = "0", FallBackFunctionType fallbackDelegate = null) where T : Object
	{
		T val = assetBundle.LoadAsset<T>(name);
		if ((Object)val != (Object)null)
		{
			return val;
		}
		val = assetBundle.LoadAsset<T>(fallbackName);
		if ((Object)val == (Object)null)
		{
			Debug.LogError("Found neither " + name + " nor the fallback " + fallbackName);
		}
		fallbackDelegate?.Invoke();
		return val;
	}
}
