using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChumList : MonoBehaviour
{
	private readonly List<Chum> chums = new List<Chum>();

	public GameObject ChumPrefab;

	private const int CHUM_VERTICAL_SPACING = 30;

	private List<NetworkPlayer> players = new List<NetworkPlayer>();

	public static ChumList Instance;

	private bool update;

	private void Start()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one chumlist in the scene!");
			return;
		}
		Instance = this;
		GetChumList();
		UpdateChums();
	}

	public void ScheduleChumUpdate()
	{
		update = true;
	}

	private void Update()
	{
		if (update)
		{
			update = false;
			GetChumList();
			UpdateChums();
		}
	}

	public void UpdateChums()
	{
		while (base.transform.childCount > 0)
		{
			Transform child = base.transform.GetChild(0);
			child.SetParent(null, worldPositionStays: false);
			Object.Destroy(child.gameObject);
		}
		for (int i = 0; i < chums.Count; i++)
		{
			GameObject obj = Object.Instantiate(ChumPrefab, base.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = new Vector3(0f, 192.55f);
			obj.transform.Translate(new Vector3(0f, -(30 * i)));
			obj.transform.Find("Name").GetComponent<Text>().text = chums[i].username;
		}
	}

	public void GetChumList()
	{
		chums.Clear();
		foreach (Player player in NetcodeManager.Instance.GetPlayers())
		{
			if (player != Player.player)
			{
				chums.Add(new Chum(player.sync.np.name));
			}
		}
	}
}
