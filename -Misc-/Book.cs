using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Book : InteractableAction
{
	public string file;

	public string[] vars;

	public Font font;

	public int fontSize;

	public TextAnchor alignment;

	public Sprite[] image = new Sprite[0];

	public Sprite cover;

	private string[] pageText;

	private uint pageindex;

	private static Sprite background;

	private static GameObject bookView;

	private static Text bookText;

	private static Image bookImage;

	public void Awake()
	{
		if (pageText == null)
		{
			string text;
			using (StreamReader streamReader = File.OpenText(Application.streamingAssetsPath + "/Books/" + file + ".txt"))
			{
				text = streamReader.ReadToEnd();
			}
			for (int i = 0; i < vars.Length; i++)
			{
				text = text.Replace("<" + i + "/>", vars[i]);
			}
			pageText = text.Split(new string[1] { "<newpage/>" }, StringSplitOptions.None);
			desc = "Read";
			sprite = Resources.Load<Sprite>("Read");
		}
	}

	public override void Execute()
	{
		if (pageText == null)
		{
			Awake();
		}
		if (bookView == null || bookView.Equals(null))
		{
			bookView = Player.Ui.Find("Book").gameObject;
			Transform obj = bookView.transform.Find("Content");
			bookImage = obj.GetChild(0).GetComponent<Image>();
			bookText = obj.GetChild(1).GetComponent<Text>();
			background = bookView.GetComponent<Image>().sprite;
		}
		bookView.SetActive(value: true);
		SetPage();
	}

	private void NextPage()
	{
		pageindex++;
		SetPage();
	}

	private void PrevPage()
	{
		pageindex--;
		SetPage();
	}

	private void SetPage()
	{
		Button[] componentsInChildren = bookView.GetComponentsInChildren<Button>(includeInactive: true);
		if (pageindex != 0)
		{
			componentsInChildren[0].gameObject.SetActive(value: true);
			componentsInChildren[0].onClick.RemoveAllListeners();
			componentsInChildren[0].onClick.AddListener(PrevPage);
		}
		else
		{
			componentsInChildren[0].gameObject.SetActive(value: false);
		}
		int num = (int)((cover == null) ? pageindex : (pageindex - 1));
		if (pageindex == 0 && cover != null)
		{
			bookView.GetComponent<Image>().sprite = cover;
			bookText.text = "";
		}
		else
		{
			bookView.GetComponent<Image>().sprite = background;
			bookText.text = pageText[num];
			bookText.font = font;
			bookText.fontSize = fontSize;
			bookText.alignment = alignment;
			if (num < image.Length)
			{
				bookImage.sprite = image[num];
			}
			else
			{
				bookImage.sprite = null;
			}
			bookImage.preserveAspect = true;
		}
		if (num + 1 < pageText.Length)
		{
			componentsInChildren[1].gameObject.SetActive(value: true);
			componentsInChildren[1].onClick.RemoveAllListeners();
			componentsInChildren[1].onClick.AddListener(NextPage);
		}
		else
		{
			componentsInChildren[1].gameObject.SetActive(value: false);
		}
	}
}
