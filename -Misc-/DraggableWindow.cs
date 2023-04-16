using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public Transform target;

	private bool isMouseDown;

	private Vector3 startMousePosition;

	private Vector3 startPosition;

	public bool shouldReturn;

	public void OnPointerDown(PointerEventData dt)
	{
		isMouseDown = true;
		startPosition = target.position;
		startMousePosition = Input.mousePosition;
	}

	public void OnPointerUp(PointerEventData dt)
	{
		isMouseDown = false;
		if (shouldReturn)
		{
			target.position = startPosition;
		}
	}

	private void Update()
	{
		if (isMouseDown)
		{
			Vector3 vector = Input.mousePosition - startMousePosition;
			Vector3 position = startPosition + vector;
			target.position = position;
		}
	}
}
