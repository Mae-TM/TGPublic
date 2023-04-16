using UnityEngine;

public class OpenAction : InteractableAction
{
	public MonoBehaviour actionTarget;

	public Mesh altMesh;

	public Quaternion altRot;

	public Vector3 altScale;

	public Sprite altSprite;

	public string altDesc;

	private void Start()
	{
	}

	public override void Execute()
	{
		if ((bool)actionTarget)
		{
			if ((bool)altMesh)
			{
				Mesh mesh = actionTarget.GetComponent<MeshFilter>().mesh;
				Vector3 localScale = actionTarget.transform.localScale;
				Quaternion localRotation = actionTarget.transform.localRotation;
				Sprite sprite = base.sprite;
				string text = desc;
				actionTarget.GetComponent<MeshFilter>().mesh = altMesh;
				actionTarget.transform.localScale = altScale;
				actionTarget.transform.localRotation = altRot;
				base.sprite = altSprite;
				desc = altDesc;
				altMesh = mesh;
				altScale = localScale;
				altRot = localRotation;
				altSprite = sprite;
				altDesc = text;
			}
			else
			{
				MonoBehaviour.print("altMesh not set!");
			}
		}
		else
		{
			MonoBehaviour.print("actionTarget not set!");
		}
	}
}
