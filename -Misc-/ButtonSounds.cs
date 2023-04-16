using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSounds : MonoBehaviour
{
	private AudioSource source;

	private AudioClip mouseOver;

	private AudioClip mouseClick;

	private void Start()
	{
		source = base.gameObject.AddComponent<AudioSource>();
		EventTrigger eventTrigger = base.gameObject.AddComponent<EventTrigger>();
		mouseOver = Resources.Load<AudioClip>("Music/UI/tgp_mouseover");
		mouseClick = Resources.Load<AudioClip>("Music/UI/tgp_mouseclick");
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener(delegate
		{
			source.PlayOneShot(mouseOver, 2f);
		});
		eventTrigger.triggers.Add(entry);
		EventTrigger.Entry entry2 = new EventTrigger.Entry();
		entry2.eventID = EventTriggerType.PointerClick;
		entry2.callback.AddListener(delegate
		{
			source.PlayOneShot(mouseClick, 1f);
		});
		eventTrigger.triggers.Add(entry2);
	}
}
