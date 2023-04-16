using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltipped : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public string tooltip;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(tooltip))
		{
			Tooltip.Show(tooltip);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Tooltip.Hide();
	}

	protected virtual void OnDisable()
	{
		Tooltip.Hide();
	}
}
