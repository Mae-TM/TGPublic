using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Animal : MonoBehaviour
{
	[SerializeField]
	private Sprite[] idle;

	[SerializeField]
	private Sprite[] jump;

	[SerializeField]
	private Sprite[] walk;

	private Sprite normal;

	[SerializeField]
	private float jumpX = 250f;

	[SerializeField]
	private float jumpY = 250f;

	[SerializeField]
	private float speed = 5f;

	[SerializeField]
	private float idleDuration = 0.5f;

	[SerializeField]
	private float waitDuration = 2.2f;

	private SpriteRenderer renderer;

	private Rigidbody body;

	private static void LoadSprites(IReadOnlyList<Sprite> source, IList<Sprite> target)
	{
		for (int i = 0; i < target.Count; i++)
		{
			int spriteIndex = CustomCharacter.GetSpriteIndex(target[i].name);
			if (spriteIndex != -1)
			{
				target[i] = source[spriteIndex];
			}
		}
	}

	private void Start()
	{
		body = GetComponent<Rigidbody>();
		renderer = base.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
		normal = renderer.sprite;
		SessionRandom.Seed((int)GetComponent<NetworkIdentity>().netId);
		PBColor color = new PBColor(Random.Range(0f, 1f), 1f, 1f);
		renderer.material = ImageEffects.SetShiftColor(renderer.material, color);
		if (!TryGetComponent<ItemObject>(out var component) || !(component.Item is NormalItem normalItem))
		{
			return;
		}
		Sprite[][] equipSprites = normalItem.equipSprites;
		if (equipSprites != null && equipSprites.Length != 0)
		{
			Sprite[] obj = equipSprites[0];
			if (obj != null && obj.Length != 0)
			{
				normal = equipSprites[0][0];
				renderer.sprite = normal;
				LoadSprites(equipSprites[0], idle);
				LoadSprites(equipSprites[0], jump);
				LoadSprites(equipSprites[0], walk);
			}
		}
	}

	private void OnEnable()
	{
		StartCoroutine(UpdateAnimation());
	}

	private IEnumerator UpdateAnimation()
	{
		while (true)
		{
			float seconds = Random.Range(waitDuration / 2.2f, waitDuration * 2.2f);
			yield return new WaitForSeconds(seconds);
			IEnumerator animation = ((jump.Length != 0 && Random.Range(0, 4) == 0) ? Jump() : ((walk.Length != 0 && Random.Range(0, 3) == 0) ? Walk() : ((idle.Length == 0) ? null : Animate(idle, idleDuration / (float)idle.Length))));
			if (animation != null)
			{
				while (animation.MoveNext())
				{
					yield return animation.Current;
				}
			}
			renderer.sprite = normal;
		}
	}

	private Vector3 GetMoveDirection()
	{
		Vector3 zero = Vector3.zero;
		switch (Random.Range(0, 4))
		{
		case 0:
			zero.x = 1f;
			break;
		case 1:
			zero.x = -1f;
			break;
		case 2:
			zero.z = 1f;
			break;
		case 3:
			zero.z = -1f;
			break;
		}
		Vector3 localScale = base.transform.localScale;
		if (Vector3.Dot(MSPAOrthoController.main.transform.right, zero) > 0f)
		{
			localScale.x = Mathf.Abs(localScale.x);
		}
		else
		{
			localScale.x = 0f - Mathf.Abs(localScale.x);
		}
		base.transform.localScale = localScale;
		return zero;
	}

	private IEnumerator Jump()
	{
		Vector3 force = jumpY * Vector3.up + jumpX * GetMoveDirection();
		body.AddForce(force);
		return PlayUntilGrounded(jump);
	}

	private IEnumerator Walk()
	{
		IEnumerator animation2 = Animate(walk, 0.1f, loop: true);
		Vector3 direction = GetMoveDirection();
		body.AddForce(jumpY * Vector3.up);
		while (animation2.MoveNext())
		{
			float num = Vector3.Dot(body.velocity, direction);
			body.AddForce(Mathf.Clamp(speed - num, 0f - speed, speed) * direction, ForceMode.VelocityChange);
			if (jumpY != 0f)
			{
				body.AddForce(-Physics.gravity * 0.1f, ForceMode.VelocityChange);
			}
			yield return animation2.Current;
			if (Random.Range(0, 10) == 0)
			{
				break;
			}
		}
		animation2 = PlayUntilGrounded(new Sprite[1] { walk[0] });
		while (animation2.MoveNext())
		{
			yield return animation2.Current;
		}
	}

	private IEnumerator PlayUntilGrounded(Sprite[] sprites, float frameDuration = 0.1f)
	{
		yield return new WaitForFixedUpdate();
		if (Mathf.Abs(body.velocity.y) > 1E-05f)
		{
			Coroutine coroutine = StartCoroutine(Animate(sprites, frameDuration, loop: true));
			yield return new WaitUntil(() => Mathf.Abs(body.velocity.y) < 1E-05f);
			StopCoroutine(coroutine);
		}
		body.velocity = Vector3.zero;
	}

	private IEnumerator Animate(Sprite[] sprites, float frameDuration = 0.1f, bool loop = false)
	{
		do
		{
			WaitForSeconds wait = new WaitForSeconds(frameDuration);
			foreach (Sprite sprite in sprites)
			{
				renderer.sprite = sprite;
				yield return wait;
			}
		}
		while (loop);
	}
}
