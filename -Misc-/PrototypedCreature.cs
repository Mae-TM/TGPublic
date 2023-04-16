using System;
using System.Linq;
using UnityEngine;

public class PrototypedCreature : MonoBehaviour
{
	[Serializable]
	private struct AnimatedPart
	{
		public string name;

		public int[] cusCharIndex;
	}

	[Serializable]
	private struct StaticPart
	{
		public string name;

		[SerializeField]
		private SpriteRenderer[] renderer;

		public void SetSprites(Sprite[] sprites)
		{
			for (int i = 0; i < Math.Min(renderer.Length, sprites.Length); i++)
			{
				renderer[i].sprite = sprites[i];
			}
		}
	}

	private const string PROTO_PATH = "assets/characters/kernelsprite/";

	[SerializeField]
	private AnimatedPart[] animatedParts;

	[SerializeField]
	private StaticPart[] staticParts;

	private int id;

	private void Start()
	{
		id = (int)GetComponent<Attackable>().netId;
		UnityEngine.Random.InitState(id);
		AssetBundle[] array = new AssetBundle[Math.Min(2, KernelSprite.GetProtoCount())];
		if (array.Length == 0)
		{
			return;
		}
		for (uint num = 0u; num < array.Length; num++)
		{
			array[num] = KernelSprite.GetRandomProto();
		}
		string[][] assetNames = GetAssetNames(array);
		CustomCharacter cusChar = GetComponent<CustomCharacter>();
		AnimatedPart[] array2 = animatedParts;
		for (int i = 0; i < array2.Length; i++)
		{
			AnimatedPart part = array2[i];
			ChooseProto(array, assetNames, part.name, delegate(Sprite[] sprites)
			{
				int[] cusCharIndex = part.cusCharIndex;
				foreach (int index in cusCharIndex)
				{
					cusChar.SetSpriteSheet(index, sprites);
				}
			});
		}
		StaticPart[] array3 = staticParts;
		for (int i = 0; i < array3.Length; i++)
		{
			StaticPart staticPart = array3[i];
			ChooseProto(array, assetNames, staticPart.name, ((StaticPart)staticPart).SetSprites);
		}
	}

	private bool ChooseProto(AssetBundle[] proto, string[][] assetNames, string bodyPart, Action<Sprite[]> callback)
	{
		return ChooseProto(proto, assetNames, bodyPart, delegate(AsyncOperation result)
		{
			UnityEngine.Object[] allAssets = ((AssetBundleRequest)result).allAssets;
			Sprite[] array = new Sprite[allAssets.Length];
			for (int i = 0; i < allAssets.Length; i++)
			{
				array[i] = (Sprite)allAssets[i];
			}
			callback(array);
		});
	}

	private bool ChooseProto(AssetBundle[] proto, string[][] assetNames, string bodyPart, Action<AsyncOperation> callback)
	{
		UnityEngine.Random.InitState(id);
		UnityEngine.Random.InitState(id ^ bodyPart[UnityEngine.Random.Range(0, bodyPart.Length)]);
		string text = "/" + base.name.ToLowerInvariant() + "/";
		bodyPart = bodyPart.ToLowerInvariant();
		string text2 = bodyPart + ".png";
		string[] array = ((!proto[0].Contains("assets/characters/kernelsprite/" + proto[0].name + text + text2)) ? assetNames[0].Where((string str) => str.StartsWith(bodyPart, StringComparison.Ordinal)).ToArray() : new string[1] { text2 });
		if (proto.Length > 1 && (array.Length == 0 || UnityEngine.Random.Range(0, 2) == 0))
		{
			text2 = "assets/characters/kernelsprite/" + proto[1].name + text + bodyPart + ".png";
			if (proto[1].Contains(text2))
			{
				proto[1].LoadAssetWithSubAssetsAsync<Sprite>(text2).completed += callback;
				return true;
			}
			string[] array2 = assetNames[1].Where((string str) => str.StartsWith(bodyPart, StringComparison.Ordinal)).ToArray();
			if (array2.Length != 0)
			{
				string text3 = "assets/characters/kernelsprite/" + proto[1].name + text + array2.ElementAt(UnityEngine.Random.Range(0, array2.Length));
				proto[1].LoadAssetWithSubAssetsAsync<Sprite>(text3).completed += callback;
				return true;
			}
		}
		if (array.Length == 0)
		{
			return false;
		}
		string text4 = "assets/characters/kernelsprite/" + proto[0].name + text + array.ElementAt(UnityEngine.Random.Range(0, array.Length));
		proto[0].LoadAssetWithSubAssetsAsync<Sprite>(text4).completed += callback;
		return true;
	}

	private string[][] GetAssetNames(AssetBundle[] proto)
	{
		string[][] array = new string[proto.Length][];
		string text = "/" + base.name.ToLowerInvariant() + "/";
		for (int i = 0; i < proto.Length; i++)
		{
			string start = "assets/characters/kernelsprite/" + proto[i].name + text;
			array[i] = (from str in proto[i].GetAllAssetNames()
				where str.StartsWith(start, StringComparison.Ordinal)
				select str.Substring(start.Length)).ToArray();
		}
		return array;
	}
}
