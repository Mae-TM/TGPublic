using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CharacterOptionsGridComponent : MonoBehaviour
{
	public class SymbolChangeEventArgs : EventArgs
	{
		public Texture2D texture;

		public int id;
	}

	public class SimpleWWWItemEventArgs : EventArgs
	{
		public string code;

		public ArmorKind kind;
	}

	public class SimpleStringEventArgs : EventArgs
	{
		public string data;
	}

	public Image gridCell;

	public Sprite[] symbolSprt;

	private HairHighlights hairHighLights;

	private readonly List<NormalItem>[] items = new List<NormalItem>[5];

	public event EventHandler<SymbolChangeEventArgs> SymbolChange;

	public event EventHandler<SimpleWWWItemEventArgs> ClothesChange;

	public event EventHandler<SimpleStringEventArgs> EyesChange;

	public event EventHandler<SimpleStringEventArgs> MouthChange;

	public event EventHandler<SimpleStringEventArgs> HairLowerChange;

	public event EventHandler<SimpleStringEventArgs> HairUpperChange;

	public event EventHandler<SimpleStringEventArgs> ShirtChange;

	public void SymbolChangeInternal(int n)
	{
		this.SymbolChange?.Invoke(this, new SymbolChangeEventArgs
		{
			id = n
		});
	}

	public void SymbolChangeInternal(Texture2D texture)
	{
		this.SymbolChange?.Invoke(this, new SymbolChangeEventArgs
		{
			texture = texture,
			id = -1
		});
	}

	public void Start()
	{
		symbolSprt = Resources.LoadAll<Sprite>("Player/symbol/symb");
		SetSymbolTab();
		gridCell.transform.parent.GetChild(1).SetSiblingIndex(gridCell.transform.parent.childCount - 1);
		for (int i = 0; i < items.Length; i++)
		{
			items[i] = new List<NormalItem>();
		}
		foreach (string item in StreamingAssets.ReadLines("charoptions.txt"))
		{
			NormalItem normalItem = item;
			if (normalItem != null && normalItem.IsArmor())
			{
				items[(int)normalItem.armor].Add(normalItem);
			}
		}
		List<NormalItem>[] array = items;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].TrimExcess();
		}
	}

	public string GetRandomClothing(ArmorKind kind)
	{
		List<NormalItem> list = items[(int)kind];
		return list[UnityEngine.Random.Range(0, list.Count)].captchaCode;
	}

	private int GetTab()
	{
		foreach (Transform item in gridCell.transform.parent.parent.parent.parent)
		{
			int siblingIndex = item.GetSiblingIndex();
			if (siblingIndex != 0 && !item.GetComponent<Button>().interactable)
			{
				return siblingIndex - 1;
			}
		}
		return -1;
	}

	public void SetHairHighLights(HairHighlights to)
	{
		hairHighLights = to;
		int tab = GetTab();
		if (tab == 1 || tab == 2)
		{
			SwitchTab(tab);
		}
	}

	public void SwitchTab(int tab)
	{
		foreach (Transform item in gridCell.transform.parent.parent.parent.parent)
		{
			int siblingIndex = item.GetSiblingIndex();
			if (siblingIndex != 0)
			{
				item.GetComponent<Button>().interactable = siblingIndex != tab + 1;
			}
		}
		Transform transform2 = null;
		foreach (Transform item2 in gridCell.transform.parent)
		{
			if (item2.name == "Custom")
			{
				transform2 = item2;
			}
			else if (item2 != gridCell.transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		GridLayoutGroup component = gridCell.transform.parent.GetComponent<GridLayoutGroup>();
		component.padding.left = 6;
		component.padding.right = 6;
		component.padding.top = 6;
		component.padding.bottom = 6;
		gridCell.gameObject.SetActive(value: true);
		switch (tab)
		{
		case 0:
			SetClothingTab(ArmorKind.Hat);
			transform2.gameObject.SetActive(value: false);
			break;
		case 1:
		{
			transform2.gameObject.SetActive(value: false);
			component.cellSize = new Vector2(128f, 128f);
			component.spacing = new Vector2(0f, -16f);
			component.padding.right = -16;
			component.padding.top = -12;
			Sprite sprite2 = Resources.Load<Sprite>("Player/Head");
			AssetBundle hairUpperBundle = ItemDownloader.GetHairUpperBundle(hairHighLights);
			foreach (string option in ItemDownloader.GetOptions(hairUpperBundle))
			{
				Image image = UnityEngine.Object.Instantiate(gridCell, gridCell.transform.parent);
				image.name = Path.GetFileNameWithoutExtension(option);
				image.sprite = hairUpperBundle.LoadAsset<Sprite>(option);
				image.GetComponent<Button>().onClick.AddListener(delegate
				{
					this.HairUpperChange(this, new SimpleStringEventArgs
					{
						data = option
					});
				});
				GameObject gameObject = new GameObject("Image");
				gameObject.transform.SetParent(image.transform, worldPositionStays: false);
				gameObject.AddComponent<Image>().sprite = sprite2;
				RectTransform obj5 = gameObject.transform as RectTransform;
				float b = Mathf.Max(image.sprite.rect.width, image.sprite.rect.height);
				b = Mathf.Max(200f, b);
				obj5.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 19200f / b);
				obj5.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 19200f / b);
			}
			gridCell.gameObject.SetActive(value: false);
			break;
		}
		case 2:
			transform2.gameObject.SetActive(value: false);
			SetHeadTab(ItemDownloader.GetHairLowerBundle(hairHighLights), this.HairLowerChange);
			break;
		case 3:
			SetClothingTab(ArmorKind.Face);
			transform2.gameObject.SetActive(value: false);
			break;
		case 4:
			transform2.gameObject.SetActive(value: false);
			SetHeadTab(ItemDownloader.Instance.eyesBundle, this.EyesChange);
			break;
		case 5:
			transform2.gameObject.SetActive(value: false);
			SetHeadTab(ItemDownloader.Instance.mouthBundle, this.MouthChange);
			break;
		case 6:
			SetClothingTab(ArmorKind.Shirt);
			transform2.gameObject.SetActive(value: false);
			break;
		case 7:
			component.cellSize = new Vector2(32f, 32f);
			component.spacing = new Vector2(2f, 2f);
			SetSymbolTab();
			transform2.SetSiblingIndex(gridCell.transform.parent.childCount - 1);
			transform2.gameObject.SetActive(value: true);
			break;
		case 8:
			transform2.gameObject.SetActive(value: false);
			component.cellSize = new Vector2(96f, 96f);
			component.spacing = new Vector2(0f, -32f);
			component.padding.bottom -= 32;
			foreach (string option2 in ItemDownloader.GetOptions(ItemDownloader.Instance.shirtBundle))
			{
				Sprite sprite = ItemDownloader.Instance.shirtBundle.LoadAsset<Sprite>(option2);
				string name = Path.GetFileNameWithoutExtension(option2);
				Image image = UnityEngine.Object.Instantiate(gridCell, gridCell.transform.parent);
				image.name = name;
				image.GetComponent<Image>().enabled = false;
				image.GetComponent<Button>().onClick.AddListener(delegate
				{
					this.ShirtChange(this, new SimpleStringEventArgs
					{
						data = name
					});
				});
				Sprite[] sleeves = ItemDownloader.GetSleeves(name);
				GameObject gameObject;
				if (sleeves.Length != 0)
				{
					gameObject = new GameObject("LSleeve");
					gameObject.transform.SetParent(image.transform, worldPositionStays: false);
					gameObject.AddComponent<Image>().sprite = sleeves[6];
					RectTransform obj = gameObject.transform as RectTransform;
					obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 96f * sleeves[6].rect.width / 200f);
					obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 96f * sleeves[6].rect.height / 200f);
					obj.anchoredPosition = new Vector2(58f, 28f) * 96f / 200f;
				}
				gameObject = new GameObject("Shirt");
				gameObject.transform.SetParent(image.transform, worldPositionStays: false);
				if (name != "aanone")
				{
					gameObject.AddComponent<Image>().sprite = sprite;
					RectTransform obj2 = gameObject.transform as RectTransform;
					obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 96f);
					obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 96f);
				}
				else
				{
					gameObject.AddComponent<Image>().sprite = gridCell.sprite;
					RectTransform obj3 = gameObject.transform as RectTransform;
					obj3.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48f);
					obj3.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 48f);
				}
				if (sleeves.Length != 0)
				{
					gameObject = new GameObject("RSleeve");
					gameObject.transform.SetParent(image.transform, worldPositionStays: false);
					gameObject.AddComponent<Image>().sprite = sleeves[5];
					RectTransform obj4 = gameObject.transform as RectTransform;
					obj4.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 96f * sleeves[5].rect.width / 200f);
					obj4.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 96f * sleeves[5].rect.height / 200f);
					obj4.anchoredPosition = new Vector2(-69f, 28f) * 96f / 200f;
				}
			}
			gridCell.gameObject.SetActive(value: false);
			break;
		case 9:
			SetClothingTab(ArmorKind.Pants);
			transform2.gameObject.SetActive(value: false);
			break;
		case 10:
			SetClothingTab(ArmorKind.Shoes);
			transform2.gameObject.SetActive(value: false);
			break;
		}
	}

	private void SetClothingTab(ArmorKind kind)
	{
		GridLayoutGroup component = gridCell.transform.parent.GetComponent<GridLayoutGroup>();
		component.cellSize = new Vector2(63f, 63f);
		component.spacing = new Vector2(4f, 4f);
		Button component2 = gridCell.GetComponent<Button>();
		component2.onClick.RemoveAllListeners();
		component2.onClick.AddListener(delegate
		{
			this.ClothesChange?.Invoke(this, new SimpleWWWItemEventArgs
			{
				kind = kind
			});
		});
		foreach (NormalItem item in items[(int)kind])
		{
			Image image = UnityEngine.Object.Instantiate(gridCell, gridCell.transform.parent);
			image.sprite = item.sprite;
			image.name = item.GetItemName();
			image.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.ClothesChange?.Invoke(this, new SimpleWWWItemEventArgs
				{
					code = item.captchaCode,
					kind = kind
				});
			});
		}
	}

	private void SetHeadTab(AssetBundle bundle, EventHandler<SimpleStringEventArgs> action)
	{
		GridLayoutGroup component = gridCell.transform.parent.GetComponent<GridLayoutGroup>();
		component.cellSize = new Vector2(96f, 96f);
		component.spacing = new Vector2(-6f, -20f);
		component.padding.left = -4;
		component.padding.top = -16;
		Sprite sprite = gridCell.sprite;
		gridCell.sprite = Resources.Load<Sprite>("Player/Head");
		foreach (string option in ItemDownloader.GetOptions(bundle))
		{
			Image image = UnityEngine.Object.Instantiate(gridCell, gridCell.transform.parent);
			image.name = Path.GetFileNameWithoutExtension(option);
			image.GetComponent<Button>().onClick.AddListener(delegate
			{
				action(this, new SimpleStringEventArgs
				{
					data = option
				});
			});
			GameObject obj = new GameObject("Image");
			obj.transform.SetParent(image.transform, worldPositionStays: false);
			image = obj.AddComponent<Image>();
			image.sprite = bundle.LoadAsset<Sprite>(option);
			image.raycastTarget = false;
			RectTransform obj2 = obj.transform as RectTransform;
			obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 128f * image.sprite.rect.width / 200f);
			obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 128f * image.sprite.rect.height / 200f);
		}
		gridCell.sprite = sprite;
		gridCell.gameObject.SetActive(value: false);
	}

	private void SetSymbolTab()
	{
		Button component = gridCell.GetComponent<Button>();
		component.onClick.RemoveAllListeners();
		component.onClick.AddListener(delegate
		{
			SymbolChangeInternal(0);
		});
		for (int i = 1; i < symbolSprt.Length; i++)
		{
			Image image = UnityEngine.Object.Instantiate(gridCell, gridCell.transform.parent);
			image.sprite = symbolSprt[i];
			image.name = symbolSprt[i].name;
			int index = i;
			image.GetComponent<Button>().onClick.AddListener(delegate
			{
				SymbolChangeInternal(index);
			});
		}
		foreach (FileInfo directoryContent in StreamingAssets.GetDirectoryContents("Symbols", "*.png", SearchOption.AllDirectories))
		{
			byte[] data = File.ReadAllBytes(directoryContent.FullName);
			Texture2D tex = new Texture2D(2, 2);
			if (tex.LoadImage(data))
			{
				if (tex.width > tex.height)
				{
					int num = Mathf.Min(32, tex.width);
					tex = ImageEffects.ResizeTexture(tex, num, num * tex.height / tex.width);
				}
				else
				{
					int num2 = Mathf.Min(32, tex.height);
					tex = ImageEffects.ResizeTexture(tex, num2 * tex.width / tex.height, num2);
				}
				Image image2 = UnityEngine.Object.Instantiate(gridCell, gridCell.transform.parent);
				image2.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
				image2.name = directoryContent.Name;
				image2.GetComponent<Button>().onClick.AddListener(delegate
				{
					SymbolChangeInternal(tex);
				});
			}
		}
	}
}
