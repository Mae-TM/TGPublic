using System;
using System.Collections;
using UnityEngine;

public class AbilityAnimator : MonoBehaviour
{
	public float timeStep;

	public bool loop;

	public bool pingPong;

	public Sprite[] sprites;

	private bool animateMask;

	private Action[] events;

	private Action endEvent;

	private SpriteRenderer renderer;

	private SpriteMask mask;

	private static AbilityAnimator Make(Sprite[] sprites, float scale, float timeStep)
	{
		GameObject gameObject = new GameObject(sprites[0].name);
		gameObject.transform.localScale = Vector3.one * scale;
		AbilityAnimator abilityAnimator = gameObject.AddComponent<AbilityAnimator>();
		abilityAnimator.renderer = gameObject.AddComponent<SpriteRenderer>();
		abilityAnimator.timeStep = timeStep;
		abilityAnimator.sprites = sprites;
		abilityAnimator.events = new Action[sprites.Length];
		return abilityAnimator;
	}

	public static AbilityAnimator Make(Sprite[] sprites, Vector3 pos, float scale = 1f, float timeStep = 0.05f)
	{
		AbilityAnimator abilityAnimator = Make(sprites, scale, timeStep);
		abilityAnimator.transform.localPosition = pos;
		abilityAnimator.gameObject.AddComponent<BillboardSprite>();
		return abilityAnimator;
	}

	public static AbilityAnimator Make(Sprite[] sprites, Transform parent, float scale = 1f, bool front = true, float timeStep = 0.05f, bool top = false)
	{
		AbilityAnimator abilityAnimator = Make(sprites, scale, timeStep);
		BillboardSprite componentInChildren = parent.GetComponentInChildren<BillboardSprite>();
		if (componentInChildren != null)
		{
			abilityAnimator.transform.SetParent(componentInChildren.transform, worldPositionStays: false);
			abilityAnimator.renderer.sortingOrder = (front ? 32767 : (-32768));
		}
		else
		{
			abilityAnimator.gameObject.AddComponent<BillboardSprite>();
			abilityAnimator.transform.SetParent(parent, worldPositionStays: false);
		}
		Player component;
		if (top)
		{
			Bounds bounds = parent.GetComponentInChildren<Renderer>().bounds;
			abilityAnimator.transform.position = bounds.center + new Vector3(0f, bounds.extents.y + 3f / 32f, 0f - bounds.extents.z);
		}
		else if (parent.TryGetComponent<Player>(out component))
		{
			abilityAnimator.transform.position = component.GetPosition();
		}
		return abilityAnimator;
	}

	public void SetEvent(int index, Action action)
	{
		events[index] = action;
	}

	public void SetEndEvent(Action action)
	{
		endEvent = action;
	}

	public void SetSprites(Sprite[] sprites)
	{
		this.sprites = sprites;
		events = new Action[sprites.Length];
	}

	public void AnimateMask(Sprite background)
	{
		mask = base.gameObject.AddComponent<SpriteMask>();
		renderer.sprite = background;
		renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
		animateMask = true;
	}

	private void SetIndex(int index)
	{
		if (animateMask)
		{
			mask.sprite = sprites[index];
		}
		else
		{
			renderer.sprite = sprites[index];
		}
		events[index]?.Invoke();
	}

	private IEnumerator Start()
	{
		WaitForSeconds delay = new WaitForSeconds(timeStep);
		do
		{
			int j = 0;
			while (j < sprites.Length)
			{
				SetIndex(j);
				yield return delay;
				int num = j + 1;
				j = num;
			}
			endEvent?.Invoke();
			if (pingPong)
			{
				j = sprites.Length - 1;
				while (j >= 0)
				{
					SetIndex(j);
					yield return delay;
					int num = j - 1;
					j = num;
				}
			}
		}
		while (loop);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
