using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using TheGenesisLib.Models;
using UnityEngine;

internal class ItemDownloader : MonoBehaviour
{
	public static ItemDownloader Instance;

	private AssetBundle itemBundle;

	private AssetBundle prefabBundle;

	private AssetBundle weaponBundle;

	private AssetBundle specialBundle;

	private AssetBundle sleevesBundle;

	private AssetBundle glovesBundle;

	private AssetBundle bodyBundle;

	private AssetBundle[] hairUpperBundle;

	private AssetBundle[] hairLowerBundle;

	private AssetBundle[] armorBundle;

	public AssetBundle eyesBundle;

	public AssetBundle mouthBundle;

	public AssetBundle shirtBundle;

	[SerializeField]
	private Sprite[] armorKind;

	private void Awake()
	{
		Instance = this;
		itemBundle = AssetBundleExtensions.Load("items");
		prefabBundle = AssetBundleExtensions.Load("itemprefab");
		weaponBundle = AssetBundleExtensions.Load("weapons");
		specialBundle = AssetBundleExtensions.Load("specialitems");
		hairUpperBundle = Enumerable.Range(0, 4).Select(delegate(int i)
		{
			HairHighlights hairHighlights2 = (HairHighlights)i;
			string text2 = hairHighlights2.ToString().ToLowerInvariant();
			return AssetBundleExtensions.Load("hair/upper/" + text2);
		}).ToArray();
		hairLowerBundle = Enumerable.Range(0, 5).Select(delegate(int i)
		{
			HairHighlights hairHighlights = (HairHighlights)i;
			string text = hairHighlights.ToString().ToLowerInvariant();
			return AssetBundleExtensions.Load("hair/lower/" + text);
		}).ToArray();
		eyesBundle = AssetBundleExtensions.Load("eyes");
		mouthBundle = AssetBundleExtensions.Load("mouth");
		shirtBundle = AssetBundleExtensions.Load("shirt");
		sleevesBundle = AssetBundleExtensions.Load("sleeves");
		glovesBundle = AssetBundleExtensions.Load("gloves");
		bodyBundle = AssetBundleExtensions.Load("body");
		armorBundle = new AssetBundle[5]
		{
			AssetBundleExtensions.Load("hat"),
			AssetBundleExtensions.Load("glasses"),
			AssetBundleExtensions.Load("coat"),
			AssetBundleExtensions.Load("pants"),
			AssetBundleExtensions.Load("shoes")
		};
		GameObject[] array = prefabBundle.LoadAllAssets<GameObject>();
		for (int j = 0; j < array.Length; j++)
		{
			NetworkClient.RegisterPrefab(array[j]);
		}
	}

	public LDBItem GetItem(string code)
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.GetItem(code);
	}

	public LDBItem[] GetItems(string[] code)
	{
		LDBItem[] array = new LDBItem[code.Length];
		for (uint num = 0u; num < code.Length; num++)
		{
			if (!string.IsNullOrEmpty(code[num]))
			{
				array[num] = GetItem(code[num]);
			}
		}
		return array;
	}

	public IEnumerable<LDBItem> GenerateDungeonItems(int amount, int value, int maxCost, IEnumerable<NormalItem.Tag> exclude)
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.GenerateDungeonItems(amount, value, maxCost, exclude);
	}

	private void OnDestroy()
	{
		GameObject[] array = prefabBundle.LoadAllAssets<GameObject>();
		for (int i = 0; i < array.Length; i++)
		{
			NetworkClient.UnregisterPrefab(array[i]);
		}
		itemBundle.Unload(unloadAllLoadedObjects: true);
		prefabBundle.Unload(unloadAllLoadedObjects: true);
		weaponBundle.Unload(unloadAllLoadedObjects: true);
		specialBundle.Unload(unloadAllLoadedObjects: true);
		AssetBundle[] array2 = hairUpperBundle;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Unload(unloadAllLoadedObjects: false);
		}
		array2 = hairLowerBundle;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Unload(unloadAllLoadedObjects: false);
		}
		eyesBundle.Unload(unloadAllLoadedObjects: true);
		mouthBundle.Unload(unloadAllLoadedObjects: true);
		shirtBundle.Unload(unloadAllLoadedObjects: true);
		sleevesBundle.Unload(unloadAllLoadedObjects: true);
		glovesBundle.Unload(unloadAllLoadedObjects: true);
		bodyBundle.Unload(unloadAllLoadedObjects: true);
		array2 = armorBundle;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Unload(unloadAllLoadedObjects: false);
		}
	}

	public IEnumerator<AsyncOperation> Downloaditem(string code, Action<NormalItem> done = null)
	{
		done?.Invoke(new NormalItem(GetItem(code)));
		yield break;
	}

	public static string GetRecipeResult(NormalItem a, NormalItem b, string method)
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.GetRecipeResult(a, b, method)?.Code;
	}

	public IEnumerator<AsyncOperation> GetRecipeAsync(NormalItem a, NormalItem b, string method, Func<string, IEnumerator<AsyncOperation>> onComplete = null)
	{
		if (onComplete != null)
		{
			IEnumerator<AsyncOperation> enumerator = onComplete(GetRecipeResult(a, b, method));
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}
	}

	public static Sprite GetSprite(string name)
	{
		return Instance.itemBundle.LoadAsset<Sprite>(name.Replace("/", string.Empty));
	}

	public static GameObject GetPrefab(string name)
	{
		return Instance.prefabBundle.LoadAsset<GameObject>(Path.GetFileName(name));
	}

	public static Sprite[] GetWeapon(string name)
	{
		return Instance.weaponBundle.LoadAssetWithSubAssets<Sprite>(name);
	}

	public static Sprite[] GetSpecialItem(string name)
	{
		return Instance.specialBundle.LoadAssetWithSubAssets<Sprite>(name);
	}

	public static Sprite[] GetArmor(string name, ArmorKind armorKind)
	{
		return Instance.armorBundle[(int)armorKind].LoadAssetWithSubAssets<Sprite>(name);
	}

	public static Sprite GetFirstArmor(string name, ArmorKind armorKind)
	{
		return Instance.armorBundle[(int)armorKind].LoadAsset<Sprite>(name);
	}

	public static Sprite[] GetSleeves(string name)
	{
		return Instance.sleevesBundle.LoadAssetWithSubAssets<Sprite>(name);
	}

	public static Sprite[] GetGloves(string name)
	{
		return Instance.glovesBundle.LoadAssetWithSubAssets<Sprite>(name);
	}

	public static Sprite[] GetBody(bool isRobot, string name)
	{
		if (!isRobot)
		{
			return null;
		}
		Sprite[] array = Instance.bodyBundle.LoadAssetWithSubAssets<Sprite>("Robot" + name);
		if (array.Length != 0)
		{
			return array;
		}
		return null;
	}

	public static AssetBundle GetHairUpperBundle(HairHighlights highLights)
	{
		int num = (int)((highLights != HairHighlights.Streaks) ? highLights : HairHighlights.Pale);
		return Instance.hairUpperBundle[num];
	}

	public static AssetBundle GetHairLowerBundle(HairHighlights highLights)
	{
		return Instance.hairLowerBundle[(int)highLights];
	}

	public static IEnumerable<string> GetOptions(AssetBundle bundle)
	{
		return bundle.GetAllAssetNames().Select(Path.GetFileNameWithoutExtension);
	}

	public static Sprite GetWeaponKind(WeaponKind kind)
	{
		return Resources.Load<Sprite>($"WeaponKind/{kind}");
	}

	public static Sprite GetArmorKind(ArmorKind kind)
	{
		if (kind != ArmorKind.None)
		{
			return Instance.armorKind[(int)kind];
		}
		return null;
	}
}
