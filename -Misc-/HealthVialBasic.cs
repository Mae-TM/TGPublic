using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthVialBasic : HealthVial, IOverhead
{
	private static HealthVialBasic vialPrefab;

	[SerializeField]
	private GameObject healthVial;

	[SerializeField]
	private Transform shieldGauge;

	[SerializeField]
	private RectTransform shieldGel;

	[SerializeField]
	private Image barFront;

	[SerializeField]
	private RectTransform barBack;

	[SerializeField]
	private GameObject nameTag;

	private Material gelMaterial;

	private Coroutine healthBarCoroutine;

	public static HealthVial Make(GameObject gameObject)
	{
		if (vialPrefab == null)
		{
			vialPrefab = Resources.Load<HealthVialBasic>("Health vial");
		}
		Bounds bounds = gameObject.GetComponentInChildren<Renderer>().bounds;
		Vector3 position = bounds.center + new Vector3(0f, bounds.extents.y + 3f / 32f, 0f - bounds.extents.z);
		BillboardSprite componentInChildren = gameObject.GetComponentInChildren<BillboardSprite>();
		Transform transform = (((object)componentInChildren == null) ? gameObject.transform : componentInChildren.transform);
		HealthVialBasic healthVialBasic = Object.Instantiate(vialPrefab, position, Quaternion.identity, transform);
		Transform transform2 = healthVialBasic.transform;
		transform2.localScale = transform.InverseTransformVector(transform2.localScale);
		Visibility.Copy(healthVialBasic.gameObject, gameObject);
		return healthVialBasic;
	}

	private void Awake()
	{
		Image component = healthVial.transform.GetChild(1).GetComponent<Image>();
		component.material = new Material(component.material);
		gelMaterial = component.material;
	}

	public void SetColor(Color color, Color gelColor)
	{
		barFront.color = color;
		ImageEffects.SetShiftColor(gelMaterial, gelColor);
	}

	public void SetNameTag(GameObject to)
	{
		nameTag = to;
		nameTag.transform.SetParent(base.transform, worldPositionStays: false);
		nameTag.SetActive(!healthVial.activeInHierarchy);
	}

	public void ShowAbove(RectTransform rectTransform)
	{
		rectTransform.SetParent(base.transform, worldPositionStays: false);
		rectTransform.pivot = new Vector2(0.5f, 0f);
		rectTransform.anchorMin = new Vector2(0.5f, 1f);
		rectTransform.anchorMax = new Vector2(0.5f, 1f);
		rectTransform.anchoredPosition = Vector2.zero;
	}

	private void LateUpdate()
	{
		Transform transform = base.transform;
		Vector3 localScale = transform.localScale;
		localScale.x = Mathf.Sign(transform.lossyScale.x) * localScale.x;
		transform.localScale = localScale;
	}

	protected override void SetVialSize(float health, float shield, float max)
	{
		Transform obj = barFront.transform;
		float num = Mathf.Lerp(obj.localScale.x, Mathf.Clamp01(health / max), Time.deltaTime * 5f);
		obj.localScale = new Vector3(num, 1f, 1f);
		barBack.pivot = new Vector2(1.5f - num, 0.5f);
		shieldGauge.localScale = new Vector3(num + shield / max, 1f, 1f);
		shieldGel.pivot = new Vector2(0.5f + shield / max, 0.5f);
	}

	public override void Enable(float duration = float.PositiveInfinity)
	{
		if (healthVial.activeInHierarchy)
		{
			if (healthBarCoroutine == null)
			{
				return;
			}
			StopCoroutine(healthBarCoroutine);
			healthBarCoroutine = null;
		}
		else
		{
			healthVial.SetActive(value: true);
			if (nameTag != null)
			{
				nameTag.SetActive(value: false);
			}
		}
		if (!float.IsPositiveInfinity(duration))
		{
			healthBarCoroutine = StartCoroutine(Disable(duration));
		}
	}

	public override void Disable()
	{
		healthVial.SetActive(value: false);
		if (nameTag != null)
		{
			nameTag.SetActive(value: true);
		}
	}

	private IEnumerator Disable(float delay)
	{
		yield return new WaitForSeconds(delay);
		healthBarCoroutine = null;
		Disable();
	}
}
