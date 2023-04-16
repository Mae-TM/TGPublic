using UnityEngine;

public class OldRoom : MonoBehaviour
{
	public string roomName;

	public int id;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider entity)
	{
		if ((bool)entity)
		{
			_ = (bool)entity.gameObject.GetComponent<Player>();
		}
	}
}
