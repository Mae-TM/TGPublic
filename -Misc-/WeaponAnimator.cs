using System.Linq;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer[] renderers;

	private CapsuleCollider[] colliders;

	private ParticleManager[] particleManagers;

	private Sprite[][] sprites;

	private int[] indices;

	private void Awake()
	{
		colliders = renderers.Select((SpriteRenderer renderer) => renderer.GetComponent<CapsuleCollider>()).ToArray();
		particleManagers = renderers.Select((SpriteRenderer renderer) => renderer.transform.parent.GetComponent<ParticleManager>()).ToArray();
		sprites = new Sprite[renderers.Length][];
		indices = new int[renderers.Length];
	}

	public void SetWeapon(NormalItem weapon, Material material)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			sprites[i] = weapon.equipSprites[(!weapon.IsRanged()) ? (i % weapon.equipSprites.Length) : 0];
			indices[i] = 0;
			Sprite sprite = sprites[i][0];
			SpriteRenderer spriteRenderer = renderers[i];
			Bounds bounds = sprite.bounds;
			Vector2 vector2 = (spriteRenderer.size = new Vector3(bounds.size.x, weapon.size));
			spriteRenderer.sprite = sprite;
			spriteRenderer.sharedMaterial = material;
			CapsuleCollider obj = colliders[i];
			Vector3 localScale = spriteRenderer.transform.localScale;
			obj.radius = vector2.x / localScale.x / 2f;
			obj.height = vector2.y / localScale.y;
			obj.center = bounds.center;
			particleManagers[i].ClearParticles();
			foreach (Sprite[] tagParticle in weapon.GetTagParticles())
			{
				particleManagers[i].AddParticles(tagParticle);
			}
			particleManagers[i].SetBounds(bounds.center, new Vector3(bounds.size.x, bounds.size.y, 0.2f));
		}
	}

	public void NextFrame()
	{
		for (int i = 0; i < sprites.Length; i++)
		{
			if (sprites[i].Length > 1)
			{
				indices[i] = (indices[i] + 1) % sprites[i].Length;
				renderers[i].sprite = sprites[i][indices[i]];
			}
		}
	}
}
