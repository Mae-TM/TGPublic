using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using TheGenesisLib.Models;
using UnityEngine;

public class DatabaseManager : AbstractSingletonManager<DatabaseManager>
{
	public List<LDBItem> DefaultItems { get; private set; }

	public List<LDBRecipe> DefaultRecipes { get; private set; }

	public Dictionary<string, List<LDBItem>> OverlayItems { get; private set; }

	public Dictionary<string, List<LDBRecipe>> OverlayRecipes { get; private set; }

	public List<LDBItem> AllItems => DefaultItems.Concat(OverlayItems.SelectMany((KeyValuePair<string, List<LDBItem>> x) => x.Value)).ToList();

	public List<LDBRecipe> AllRecipes => DefaultRecipes.Concat(OverlayRecipes.SelectMany((KeyValuePair<string, List<LDBRecipe>> x) => x.Value)).ToList();

	public DatabaseManager()
	{
		if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath, "ItemMods")))
		{
			Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "ItemMods"));
		}
		LoadDefaultDatabase();
		foreach (FileInfo directoryContent in StreamingAssets.GetDirectoryContents("ItemMods", "*.ldb"))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(directoryContent.Name);
			try
			{
				LoadOverlayDatabase(fileNameWithoutExtension);
			}
			catch (Exception)
			{
				Debug.LogError("Failed to load overlay database: " + fileNameWithoutExtension);
			}
		}
	}

	public void LoadDefaultDatabase()
	{
		LiteDatabase liteDatabase = new LiteDatabase(Path.Combine(Application.streamingAssetsPath, "items.ldb"));
		DefaultItems = liteDatabase.GetCollection<LDBItem>().FindAll().ToList();
		DefaultRecipes = liteDatabase.GetCollection<LDBRecipe>().FindAll().ToList();
		OverlayItems = new Dictionary<string, List<LDBItem>>();
		OverlayRecipes = new Dictionary<string, List<LDBRecipe>>();
		liteDatabase.Dispose();
	}

	private string GetOverlayPath(string name)
	{
		StreamingAssets.TryGetFile("ItemMods/" + name + ".ldb", out var path);
		return path;
	}

	public void LoadOverlayDatabase(string overlayName)
	{
		LiteDatabase liteDatabase = new LiteDatabase(GetOverlayPath(overlayName));
		List<LDBItem> value = liteDatabase.GetCollection<LDBItem>().FindAll().ToList();
		List<LDBRecipe> value2 = liteDatabase.GetCollection<LDBRecipe>().FindAll().ToList();
		if (OverlayItems.ContainsKey(overlayName))
		{
			OverlayItems[overlayName] = value;
		}
		else
		{
			OverlayItems.Add(overlayName, value);
		}
		if (OverlayRecipes.ContainsKey(overlayName))
		{
			OverlayRecipes[overlayName] = value2;
		}
		else
		{
			OverlayRecipes.Add(overlayName, value2);
		}
		liteDatabase.Dispose();
	}

	public void UnloadOverlayDatabase(string overlayName)
	{
		if (OverlayItems.ContainsKey(overlayName))
		{
			OverlayItems.Remove(overlayName);
		}
		if (OverlayRecipes.ContainsKey(overlayName))
		{
			OverlayRecipes.Remove(overlayName);
		}
	}

	public void UnloadAllOverlayDatabases()
	{
		OverlayItems.Clear();
		OverlayRecipes.Clear();
	}

	public bool DoesItemExist(string code)
	{
		return AllItems.Any((LDBItem x) => x.Code == code);
	}

	public void CreateItem(string overlayName, LDBItem item)
	{
		LiteDatabase liteDatabase = new LiteDatabase(GetOverlayPath(overlayName));
		ILiteCollection<LDBItem> collection = liteDatabase.GetCollection<LDBItem>();
		collection.EnsureIndex((LDBItem k) => k.Tags);
		collection.EnsureIndex((LDBItem k) => k.Grist);
		collection.EnsureIndex((LDBItem k) => k.Strifekind);
		collection.Insert(item);
		liteDatabase.Dispose();
	}

	public LDBItem GetItem(string code)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			return null;
		}
		LDBItem lDBItem = AllItems.FirstOrDefault((LDBItem x) => x.Code == code);
		if (lDBItem != null)
		{
			return lDBItem;
		}
		Debug.LogWarning("Could not find item with code " + code + "!");
		return new LDBItem
		{
			Name = "Internet Error",
			Prefab = "GenericObject",
			Icon = "GenericObject",
			Grist = 1,
			Speed = 1,
			Custom = false,
			Code = "00000000",
			Strifekind = "Hammer",
			Weaponsprite = "Hammer",
			Description = "This is not the item you're looking for.\nYou might have dun goofed when dealing with custom items."
		};
	}

	public LDBItem GetRecipeResult(NormalItem a, NormalItem b, string method)
	{
		LDBItem lDBItem = null;
		if (a.itemType != Item.ItemType.Custom)
		{
			if (b.itemType != Item.ItemType.Custom)
			{
				lDBItem = GetRecipe(a.captchaCode, b.captchaCode, method);
			}
			if (lDBItem == null)
			{
				lDBItem = GetKindRecipe(a.captchaCode, b, method);
			}
			if (lDBItem == null)
			{
				lDBItem = GetTagRecipe(a.captchaCode, b, method);
			}
		}
		if (b.itemType != Item.ItemType.Custom)
		{
			if (lDBItem == null)
			{
				GetKindRecipe(b.captchaCode, a, method);
			}
			if (lDBItem == null)
			{
				GetTagRecipe(b.captchaCode, a, method);
			}
		}
		return lDBItem;
	}

	private LDBItem GetKindRecipe(string a, NormalItem b, string method)
	{
		if (b.IsWeapon())
		{
			return GetRecipe(a, $"kind:{b.weaponKind[0]}", method);
		}
		if (b.IsArmor())
		{
			return GetRecipe(a, $"kind:{b.armor}", method);
		}
		return null;
	}

	public string GetTagString(IEnumerable<NormalItem.Tag> tags)
	{
		return string.Join(",", tags);
	}

	private LDBItem GetTagRecipe(string a, NormalItem b, string method)
	{
		if (b.GetTagCount() == 0)
		{
			return null;
		}
		return GetRecipe(a, "tag:" + GetTagString(b.GetTags()), method);
	}

	public void CreateRecipe(string overlayName, string a, string b, LDBRecipe.Methods method, string result)
	{
		LDBItem lDBItem = AllItems.FirstOrDefault((LDBItem x) => x.Code == result);
		if (lDBItem != null)
		{
			LDBRecipe entity = new LDBRecipe
			{
				ItemA = a,
				ItemB = b,
				Method = method,
				Result = lDBItem
			};
			LiteDatabase liteDatabase = new LiteDatabase(GetOverlayPath(overlayName));
			ILiteCollection<LDBRecipe> collection = liteDatabase.GetCollection<LDBRecipe>();
			collection.EnsureIndex((LDBRecipe k) => k.Method);
			collection.EnsureIndex((LDBRecipe k) => k.ItemA);
			collection.EnsureIndex((LDBRecipe k) => k.ItemB);
			collection.Insert(entity);
			liteDatabase.Dispose();
		}
	}

	private ILiteCollection<LDBItem> GetIndexedItemCollection(LiteDatabase db)
	{
		ILiteCollection<LDBItem> collection = db.GetCollection<LDBItem>();
		collection.EnsureIndex((LDBItem k) => k.Tags);
		collection.EnsureIndex((LDBItem k) => k.Grist);
		collection.EnsureIndex((LDBItem k) => k.Strifekind);
		return collection;
	}

	private ILiteCollection<LDBRecipe> GetIndexedRecipeCollection(LiteDatabase db)
	{
		ILiteCollection<LDBRecipe> collection = db.GetCollection<LDBRecipe>();
		collection.EnsureIndex((LDBRecipe k) => k.Method);
		collection.EnsureIndex((LDBRecipe k) => k.ItemA);
		collection.EnsureIndex((LDBRecipe k) => k.ItemB);
		return collection;
	}

	private LDBItem GetRecipeFromDatabase(LiteDatabase db, string a, string b, string method)
	{
		ILiteCollection<LDBItem> indexedItemCollection = GetIndexedItemCollection(db);
		ILiteCollection<LDBRecipe> indexedRecipeCollection = GetIndexedRecipeCollection(db);
		if (a == b)
		{
			return null;
		}
		LDBRecipe.Methods methods = ((!(method == "&&")) ? LDBRecipe.Methods.OR : LDBRecipe.Methods.AND);
		LDBItem lDBItem = indexedItemCollection.FindById(a);
		LDBItem lDBItem2 = indexedItemCollection.FindById(b);
		List<BsonValue> list = new List<BsonValue> { a };
		List<BsonValue> list2 = new List<BsonValue> { b };
		if (lDBItem != null)
		{
			list.AddRange(lDBItem.Aliases.Select((string al) => new BsonValue(al)));
		}
		if (lDBItem2 != null)
		{
			list2.AddRange(lDBItem2.Aliases.Select((string bl) => new BsonValue(bl)));
		}
		return indexedRecipeCollection.FindOne(Query.And(Query.EQ("Method", methods.ToString()), Query.Or(Query.And(Query.In("ItemA", list), Query.In("ItemB", list2)), Query.And(Query.In("ItemA", list2), Query.In("ItemB", list)))))?.Result;
	}

	private LDBItem GetRecipe(string a, string b, string method)
	{
		LiteDatabase liteDatabase = new LiteDatabase(Path.Combine(Application.streamingAssetsPath, "items.ldb"));
		LDBItem recipeFromDatabase = GetRecipeFromDatabase(liteDatabase, a, b, method);
		liteDatabase.Dispose();
		if (recipeFromDatabase != null)
		{
			return recipeFromDatabase;
		}
		foreach (string key in OverlayRecipes.Keys)
		{
			liteDatabase = new LiteDatabase(GetOverlayPath(key));
			recipeFromDatabase = GetRecipeFromDatabase(liteDatabase, a, b, method);
			liteDatabase.Dispose();
			if (recipeFromDatabase != null)
			{
				return recipeFromDatabase;
			}
		}
		return null;
	}

	public string GetPrototyping(string code)
	{
		return AllItems.FirstOrDefault((LDBItem x) => x.Code == code)?.Prototyping;
	}

	private LDBItem FindItemInDatabase(LiteDatabase db, Enum kind, NormalItem.Tag tag, int power, int speed)
	{
		return (from k in GetIndexedItemCollection(db).Find((LDBItem i) => i.Tags.Contains(tag.ToString()) && i.Strifekind == kind.ToString() && i.Spawn)
			orderby Math.Abs(k.Grist - power), Math.Abs(k.Speed - speed)
			select k).ThenBy((LDBItem i) => UnityEngine.Random.value).FirstOrDefault();
	}

	public LDBItem FindItemInDatabase(LiteDatabase db, Enum kind, IEnumerable<NormalItem.Tag> tags, int power, int speed)
	{
		ILiteCollection<LDBItem> indexedItemCollection = GetIndexedItemCollection(db);
		List<string> strTags = tags.Select((NormalItem.Tag t) => "\"" + t.ToString() + "\"").ToList();
		return (from i in indexedItemCollection.Find(Query.And(Query.EQ("Strifekind", kind.ToString()), Query.EQ("Spawn", true), Query.GT("COUNT($.Tags)", 0), Query.EQ("COUNT(UNION($.Tags, [" + string.Join(",", strTags) + "]))", strTags.Count)))
			orderby strTags.Except(i.Tags).Count(), Math.Abs(i.Grist - power), Math.Abs(i.Speed - speed), UnityEngine.Random.value
			select i).FirstOrDefault();
	}

	public LDBItem FindItem(Enum kind, NormalItem.Tag tag, int power, int speed)
	{
		LiteDatabase liteDatabase = new LiteDatabase(Path.Combine(Application.streamingAssetsPath, "items.ldb"));
		LDBItem lDBItem = FindItemInDatabase(liteDatabase, kind, tag, power, speed);
		liteDatabase.Dispose();
		if (lDBItem != null)
		{
			return lDBItem;
		}
		foreach (string key in OverlayItems.Keys)
		{
			liteDatabase = new LiteDatabase(GetOverlayPath(key));
			lDBItem = FindItemInDatabase(liteDatabase, kind, tag, power, speed);
			liteDatabase.Dispose();
			if (lDBItem != null)
			{
				return lDBItem;
			}
		}
		return null;
	}

	public LDBItem FindItem(Enum kind, IEnumerable<NormalItem.Tag> tags, int power, int speed)
	{
		LiteDatabase liteDatabase = new LiteDatabase(Path.Combine(Application.streamingAssetsPath, "items.ldb"));
		LDBItem lDBItem = FindItemInDatabase(liteDatabase, kind, tags, power, speed);
		liteDatabase.Dispose();
		if (lDBItem != null)
		{
			return lDBItem;
		}
		foreach (string key in OverlayItems.Keys)
		{
			liteDatabase = new LiteDatabase(GetOverlayPath(key));
			lDBItem = FindItemInDatabase(liteDatabase, kind, tags, power, speed);
			liteDatabase.Dispose();
			if (lDBItem != null)
			{
				return lDBItem;
			}
		}
		return null;
	}

	public IEnumerable<LDBItem> GenerateDungeonItemsFromDatabase(LiteDatabase db, int amount, int value, int maxCost, IEnumerable<NormalItem.Tag> exclude)
	{
		ILiteCollection<LDBItem> indexedItemCollection = GetIndexedItemCollection(db);
		string text = string.Join(",", exclude.Select((NormalItem.Tag tag) => $"\"{tag}\""));
		return indexedItemCollection.Find(Query.And(Query.Between("Grist", 1, maxCost), Query.EQ("Spawn", true), Query.Not("COUNT(EXCEPT($.Tags, [" + text + "]))", 0))).ToArray();
	}

	public IEnumerable<LDBItem> GenerateDungeonItems(int amount, int value, int maxCost, IEnumerable<NormalItem.Tag> exclude)
	{
		LiteDatabase liteDatabase = new LiteDatabase(Path.Combine(Application.streamingAssetsPath, "items.ldb"));
		IEnumerable<LDBItem> first = GenerateDungeonItemsFromDatabase(liteDatabase, amount, value, maxCost, exclude);
		liteDatabase.Dispose();
		IEnumerable<LDBItem> enumerable = new List<LDBItem>();
		foreach (string key in OverlayItems.Keys)
		{
			liteDatabase = new LiteDatabase(GetOverlayPath(key));
			IEnumerable<LDBItem> second = GenerateDungeonItemsFromDatabase(liteDatabase, amount, value, maxCost, exclude);
			liteDatabase.Dispose();
			enumerable = enumerable.Concat(second);
		}
		List<LDBItem> combined = first.Concat(enumerable).ToList();
		combined.Sort((LDBItem a, LDBItem b) => a.Grist.CompareTo(b.Grist));
		maxCost = combined[combined.Count - 1].Grist;
		int[] index = new int[maxCost + 1];
		int i = 0;
		int j = 0;
		for (; i <= maxCost; i++)
		{
			for (; j < combined.Count && combined[j].Grist <= i; j++)
			{
			}
			index[i] = j;
		}
		while (amount > 0)
		{
			int targetGrist = UnityEngine.Random.Range(0, Math.Min(value * 2 / amount, maxCost));
			yield return combined[UnityEngine.Random.Range(index[targetGrist], index[targetGrist + 1])];
			value -= targetGrist;
			if (value > 0)
			{
				int num = amount - 1;
				amount = num;
				continue;
			}
			break;
		}
	}
}
