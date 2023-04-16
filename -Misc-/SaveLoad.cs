using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoad : MonoBehaviour
{
	public string path;

	public RectTransform textArea;

	public Text text;

	public int boundary = 5;

	public RectTransform selected;

	public int textWidth = 150;

	public int doubleClickTime = 30;

	public InputField inputField;

	public Scrollbar scrollbar;

	public ISaveLoadable saveLoadable;

	public string fileType = "";

	public bool getDirectoryInsteadOfFile;

	public Text title;

	private string[] choices;

	private int width;

	private int height;

	private int textHeight;

	private int xMag;

	private int yMag;

	private int selection = -1;

	private int frameOfLastClick;

	private int scrollValue;

	private void setScrollValue(int i)
	{
		scrollbar.value = i / (scrollbar.numberOfSteps - 1);
	}

	private void updateSelection(bool moveScrollbar)
	{
		if (selection == -1)
		{
			selected.gameObject.SetActive(value: false);
			return;
		}
		if (selection < scrollValue * height || selection >= (scrollValue + width) * height)
		{
			if (!moveScrollbar)
			{
				selected.gameObject.SetActive(value: false);
				return;
			}
			if (selection < scrollValue * height)
			{
				setScrollValue(selection / height);
			}
			else
			{
				setScrollValue(selection / height + width - 1);
			}
		}
		int num = selection / height - scrollValue;
		int num2 = selection % height;
		selected.transform.localPosition = new Vector3(num * textWidth - xMag, yMag - num2 * textHeight);
		selected.gameObject.SetActive(value: true);
		inputField.text = choices[selection];
	}

	public void updateScrollbar()
	{
		scrollValue = Mathf.Max(0, Mathf.RoundToInt(scrollbar.value * (float)(scrollbar.numberOfSteps - 1)));
		Text[] componentsInChildren = textArea.GetComponentsInChildren<Text>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
		xMag = (int)((textArea.rect.width - (float)(2 * boundary) - (float)textWidth) / 2f);
		yMag = (int)((textArea.rect.height - (float)(2 * boundary) - (float)textHeight) / 2f);
		float num = -xMag;
		float num2 = yMag;
		int num3 = Mathf.Min((scrollValue + width) * height, choices.Length);
		for (int j = scrollValue * height; j < num3; j++)
		{
			Text obj = Object.Instantiate(text, textArea);
			obj.transform.localPosition = new Vector3(num, num2);
			obj.text = choices[j];
			obj.gameObject.SetActive(value: true);
			obj.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth - 2 * boundary);
			num2 -= (float)textHeight;
			if (num2 < (float)(-yMag))
			{
				num2 = yMag;
				num += (float)textWidth;
				if (num > (float)xMag)
				{
					break;
				}
			}
		}
		updateSelection(moveScrollbar: false);
	}

	private void updateDirectory()
	{
		selection = -1;
		selected.gameObject.SetActive(value: false);
		inputField.text = "";
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		IEnumerable<string> enumerable = (from d in directoryInfo.EnumerateDirectories()
			select d.Name + Path.DirectorySeparatorChar).Prepend(".." + Path.DirectorySeparatorChar);
		if (!getDirectoryInsteadOfFile)
		{
			enumerable = enumerable.Concat(from f in directoryInfo.EnumerateFiles(fileType)
				select f.Name);
		}
		choices = enumerable.ToArray();
		scrollbar.numberOfSteps = 2 - width + (choices.Length - 1) / height;
		if (scrollbar.numberOfSteps <= 1)
		{
			scrollbar.gameObject.SetActive(value: false);
			updateScrollbar();
			return;
		}
		scrollbar.size = 1f / (float)scrollbar.numberOfSteps;
		if (scrollbar.value == 0f)
		{
			updateScrollbar();
		}
		else
		{
			scrollbar.value = 0f;
		}
		scrollbar.gameObject.SetActive(value: true);
	}

	private void ok(bool isButton)
	{
		Debug.Log($"isButton: {isButton}, getDirectoryInsteadOfFile: {getDirectoryInsteadOfFile}, directory {path + inputField.text} exists: {Directory.Exists(path + inputField.text)}");
		if (inputField.text == ".." + Path.DirectorySeparatorChar)
		{
			path = Directory.GetParent(path).Parent.FullName + Path.DirectorySeparatorChar;
			updateDirectory();
		}
		else if ((!isButton || !getDirectoryInsteadOfFile) && Directory.Exists(path + inputField.text))
		{
			path += inputField.text;
			updateDirectory();
		}
		else
		{
			base.gameObject.SetActive(value: false);
			saveLoadable.PickFile(path + inputField.text);
		}
	}

	public void ok()
	{
		ok(isButton: true);
	}

	public void cancel()
	{
		base.gameObject.SetActive(value: false);
		saveLoadable.Cancel();
	}

	public void run(ISaveLoadable saveLoadable, string title, string path, string fileType, bool getDirectoryInsteadOfFile)
	{
		this.saveLoadable = saveLoadable;
		this.title.text = title;
		if (path != null)
		{
			if (path == "")
			{
				this.path = "." + Path.DirectorySeparatorChar;
			}
			else
			{
				this.path = path;
			}
		}
		this.fileType = fileType;
		this.getDirectoryInsteadOfFile = getDirectoryInsteadOfFile;
		base.gameObject.SetActive(value: true);
	}

	private void OnEnable()
	{
		textHeight = (int)((RectTransform)text.transform).rect.height;
		width = Mathf.FloorToInt((textArea.rect.width - (float)(2 * boundary)) / (float)textWidth);
		height = Mathf.FloorToInt((textArea.rect.height - (float)(2 * boundary)) / (float)textHeight);
		selected.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
		selected.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
		updateDirectory();
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			Vector3 point = textArea.InverseTransformPoint(Input.mousePosition);
			if (textArea.rect.Contains(point))
			{
				int num = Mathf.RoundToInt((point.x + (float)xMag) / (float)textWidth);
				if (num < 0 || num >= width)
				{
					return;
				}
				int num2 = Mathf.RoundToInt(((float)yMag - point.y) / (float)textHeight);
				if (num2 < 0 || num2 >= height)
				{
					return;
				}
				int num3 = (num + scrollValue) * height + num2;
				if (num3 >= choices.Length)
				{
					return;
				}
				if (num3 == selection)
				{
					if (Time.frameCount - frameOfLastClick < doubleClickTime)
					{
						ok(isButton: false);
					}
					frameOfLastClick = Time.frameCount;
					return;
				}
				selection = num3;
				updateSelection(moveScrollbar: false);
				frameOfLastClick = Time.frameCount;
			}
		}
		if (!inputField.isFocused)
		{
			if (Input.GetKeyDown(KeyCode.UpArrow) && selection > 0)
			{
				selection--;
				updateSelection(moveScrollbar: true);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow) && selection < choices.Length - 1)
			{
				selection++;
				updateSelection(moveScrollbar: true);
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (selection > height)
				{
					selection -= height;
				}
				else
				{
					selection = 0;
				}
				updateSelection(moveScrollbar: true);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				if (selection + height < choices.Length - 1)
				{
					selection += height;
				}
				else
				{
					selection = choices.Length - 1;
				}
				updateSelection(moveScrollbar: true);
			}
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			ok();
		}
		else if (Input.GetKeyDown(KeyCode.Escape))
		{
			cancel();
		}
	}
}
