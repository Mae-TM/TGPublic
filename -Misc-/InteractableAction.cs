using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Interactable))]
public abstract class InteractableAction : MonoBehaviour, IInteractableAction
{
	[SerializeField]
	protected Sprite sprite;

	[SerializeField]
	protected string desc;

	public string Desc => desc;

	protected virtual Material Material => null;

	protected virtual Color Color => Color.white;

	public abstract void Execute();

	public virtual void ApplyToImage(Image image)
	{
		image.sprite = sprite;
		image.material = Material;
		image.color = Color;
	}

	[SpecialName]
	bool IInteractableAction.get_enabled()
	{
		return base.enabled;
	}
}
