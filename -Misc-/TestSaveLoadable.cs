using UnityEngine;

public class TestSaveLoadable : MonoBehaviour, ISaveLoadable
{
	public void PickFile(string file)
	{
		MonoBehaviour.print(file);
	}

	public void Cancel()
	{
	}

	private void Start()
	{
		SaveLoad obj = Resources.FindObjectsOfTypeAll<SaveLoad>()[0];
		obj.saveLoadable = this;
		obj.gameObject.SetActive(value: true);
	}

	private void Update()
	{
	}
}
