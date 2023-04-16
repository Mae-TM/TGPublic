using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuildInfo : MonoBehaviour
{
	public string DevName;

	public string DevComments;

	public Sprite DevSprite;

	public Color textColor;

	public Transform Showing;

	public Transform Hidden;

	public Transform Movethis;

	public float TimeToComplete;

	public Text Comment;

	public Text Name;

	public Image Dev;

	private bool moving;

	private bool isShowing;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		Comment.text = DevComments;
		Name.text = "::" + DevName + "::";
		Dev.sprite = DevSprite;
	}

	public void doThing()
	{
		if (!moving)
		{
			moving = true;
			isShowing = !isShowing;
			StartCoroutine(MoveIn());
		}
	}

	private IEnumerator MoveIn()
	{
		float timetoComplete = Time.time + TimeToComplete;
		Vector3 orgin = Hidden.position;
		float ydist = Showing.position.y - Hidden.position.y;
		while (Time.time < timetoComplete)
		{
			Vector3 position = Movethis.position;
			if (isShowing)
			{
				position.y = orgin.y + ydist * (1f - (timetoComplete - Time.time) / TimeToComplete);
			}
			else
			{
				position.y = orgin.y + ydist * ((timetoComplete - Time.time) / TimeToComplete);
			}
			Movethis.position = position;
			yield return null;
		}
		moving = false;
	}
}
