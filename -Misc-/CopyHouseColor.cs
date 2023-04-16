using UnityEngine;

public class CopyHouseColor : MonoBehaviour
{
	private void OnTransformParentChanged()
	{
		if (!(base.transform.root == null) && base.transform.root.TryGetComponent<House>(out var component))
		{
			SpriteRenderer component2 = GetComponent<SpriteRenderer>();
			Material material = component2.material;
			ImageEffects.SetShiftColor(material, component.ownerColor);
			component2.sharedMaterial = material;
		}
	}
}
