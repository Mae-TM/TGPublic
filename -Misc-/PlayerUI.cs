using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	private static readonly int blinkHash = Animator.StringToHash("Blinking");

	public HealthVial healthVial;

	[SerializeField]
	private Text healthRegenText;

	[SerializeField]
	private RectTransform VimBar;

	private Text vimText;

	private Text vimRegenText;

	private Image vimBar;

	[SerializeField]
	private RectTransform XPBar;

	[SerializeField]
	private Text gristCount;

	[SerializeField]
	private GameObject gristCollectPopup;

	private void Awake()
	{
		vimRegenText = VimBar.GetChild(2).GetComponent<Text>();
		vimText = VimBar.GetChild(1).GetComponent<Text>();
		vimBar = VimBar.GetChild(0).GetComponent<Image>();
	}

	public void UpdateExperience(float experience, float maxExperience)
	{
		bool flag = experience >= maxExperience;
		XPBar.GetChild(0).GetComponent<Image>().fillAmount = experience / maxExperience;
		XPBar.GetChild(1).GetComponent<Animator>().SetBool(blinkHash, flag);
		XPBar.GetChild(2).GetComponent<Text>().text = (flag ? "LEVEL UP" : $"{experience}/{maxExperience}");
	}

	public void SetLevel(uint level)
	{
		XPBar.GetChild(1).GetChild(0).GetComponent<Text>()
			.text = level.ToString();
	}

	public void SetVim(float vim, float vimMax)
	{
		vimText.text = $"{Mathf.Floor(vim)}/{vimMax}";
		vimBar.fillAmount = vim / vimMax;
	}

	public void SetVimRegen(float value)
	{
		vimRegenText.text = "+" + value;
	}

	public void SetHealthRegen(float value)
	{
		healthRegenText.text = "+" + value;
	}

	public void SetGrist(int value)
	{
		gristCount.text = Sylladex.MetricFormat(value);
	}

	public void ShowGristCollect(int index, int diff, Vector3 position)
	{
		if (diff > 0 && gristCollectPopup != null)
		{
			gristCollectPopup.SetActive(value: true);
			gristCollectPopup.GetComponent<Animator>().Play(0);
			gristCollectPopup.transform.position = MSPAOrthoController.main.WorldToScreenPoint(position) + new Vector3(0f, 160f, 0f);
			Transform child = gristCollectPopup.transform.GetChild(0);
			child.GetComponent<Text>().text = "+" + Sylladex.MetricFormat(diff);
			child.GetChild(0).GetComponent<GristImage>().SetGrist(index);
		}
	}
}
