using UnityEngine;
using UnityEngine.EventSystems;

public class AutoClose : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public static uint activeCount;

	public bool destroy;

	private bool pointerEntered;

	private float openTime;

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerEntered = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerEntered = false;
	}

	private void OnEnable()
	{
		openTime = Time.time;
		activeCount++;
	}

	private void OnDisable()
	{
		if (activeCount != 0)
		{
			activeCount--;
		}
	}

	private void Update()
	{
		if (Time.time > openTime && Input.GetMouseButtonUp(0) && !pointerEntered)
		{
			if (destroy)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
