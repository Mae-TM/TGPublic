using UnityEngine;
using UnityEngine.EventSystems;

public class AbstratusCard : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private RectTransform rectTransform;

	private Transform parent;

	private Vector2 origScale;

	private Vector2 targetScale;

	private float timeClicked;

	private void Start()
	{
		rectTransform = base.transform as RectTransform;
		targetScale = rectTransform.sizeDelta;
		origScale = targetScale;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (parent == null)
		{
			parent = rectTransform.parent;
			rectTransform.SetParent(parent.parent, worldPositionStays: true);
			targetScale = 2f * origScale;
			timeClicked = Time.time;
		}
	}

	private void Update()
	{
		if (parent != null && Input.GetMouseButtonUp(0) && timeClicked < Time.time)
		{
			rectTransform.SetParent(parent, worldPositionStays: true);
			targetScale = origScale;
			parent = null;
		}
		rectTransform.sizeDelta = targetScale;
		rectTransform.localPosition = Vector3.zero;
	}
}
