using UnityEngine;
using UnityEngine.AI;

public class DoorAction2 : InteractableAction, AutoPickup
{
	[SerializeField]
	private Transform target;

	[SerializeField]
	private Transform target2;

	private static float lastUsed = 0f;

	private static Collider[] results = new Collider[8];

	private void Awake()
	{
		sprite = Resources.Load<Sprite>("DoorAction");
		desc = "Use Door";
		OffMeshLink offMeshLink = AutoOffMeshLink.AddOffMeshLink(base.gameObject);
		offMeshLink.startTransform = target;
		offMeshLink.endTransform = target2;
	}

	public override void Execute()
	{
		if (target != null && !target.Equals(null))
		{
			if (!IsBlocked())
			{
				Fader.instance.fadedToBlack = OnFadeFinished;
				Fader.instance.BeginFade(1);
			}
			else
			{
				SoundEffects.Instance.Nope();
			}
		}
		else
		{
			Debug.LogError("Warning: Door's target has not been set.");
		}
	}

	private void OnFadeFinished()
	{
		Fader.instance.BeginFade(-1);
		Teleport(Player.player);
		lastUsed = Time.unscaledTime;
	}

	private bool IsBlocked()
	{
		CapsuleCollider component = Player.player.GetComponent<CapsuleCollider>();
		if (IsBlocked(target.position, component))
		{
			return IsBlocked(target2.position, component);
		}
		return false;
	}

	private bool IsBlocked(Vector3 position, CapsuleCollider capsule)
	{
		Vector3 point = position + new Vector3(0f, 0.1f, 0f);
		Vector3 point2 = position + new Vector3(0f, capsule.height, 0f);
		while (true)
		{
			int num = Physics.OverlapCapsuleNonAlloc(point, point2, float.Epsilon, results);
			for (int i = 0; i < num; i++)
			{
				if ((bool)results[i].GetComponentInParent<Furniture>() && !results[i].transform.IsChildOf(base.transform))
				{
					return true;
				}
			}
			if (num < results.Length)
			{
				break;
			}
			results = new Collider[results.Length * 2];
		}
		return false;
	}

	public void Pickup(Player player)
	{
		if (Time.unscaledTime - lastUsed > Time.unscaledDeltaTime * 10f && !IsBlocked())
		{
			Teleport(player);
		}
		lastUsed = Time.unscaledTime;
	}

	private void Teleport(Player player)
	{
		player.LeaveStrife();
		Vector3 position = player.transform.position;
		float sqrMagnitude = (target.position - position).sqrMagnitude;
		float sqrMagnitude2 = (target2.position - position).sqrMagnitude;
		player.SetPosition(((sqrMagnitude > sqrMagnitude2) ? target : target2).position);
	}
}
