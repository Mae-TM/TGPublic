using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class RegionChild : NetworkBehaviour
{
	public delegate void OnRegionChangeHandler(WorldRegion oldRegion, WorldRegion newRegion);

	public delegate void OnAreaChangeHandler(WorldArea oldArea, WorldArea newArea);

	private static Camera camera;

	private static RegionChild focused;

	[SyncVar(hook = "OnAreaChanged")]
	private WorldArea area;

	private Vector3 pos;

	private NetworkBehaviourSyncVar ___areaNetId;

	public WorldArea Area
	{
		get
		{
			return Networkarea;
		}
		[Server]
		set
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RegionChild::set_Area(WorldArea)' called when server was not active");
			}
			else if (!(Networkarea == value))
			{
				WorldArea networkarea = Networkarea;
				Networkarea = value;
				if (base.isServerOnly)
				{
					OnAreaChanged(networkarea, value);
				}
			}
		}
	}

	public WorldRegion Region { get; private set; }

	public WorldArea Networkarea
	{
		get
		{
			return GetSyncVarNetworkBehaviour(___areaNetId, ref area);
		}
		[param: In]
		set
		{
			if (!SyncVarNetworkBehaviourEqual(value, ___areaNetId))
			{
				WorldArea networkarea = Networkarea;
				SetSyncVarNetworkBehaviour(value, ref area, 1uL, ref ___areaNetId);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnAreaChanged(networkarea, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public event OnRegionChangeHandler onRegionChanged;

	public static event OnRegionChangeHandler onFocusRegionChanged;

	public event OnAreaChangeHandler onAreaChanged;

	private void OnAreaChanged(WorldArea oldArea, WorldArea newArea)
	{
		SetRegion();
		this.onAreaChanged?.Invoke(oldArea, newArea);
	}

	private void Update()
	{
		if (base.transform.hasChanged && (object)Area != null)
		{
			Vector3 localPosition = base.transform.localPosition;
			if (pos != localPosition)
			{
				SetRegion(localPosition);
			}
			else
			{
				base.transform.hasChanged = false;
			}
		}
	}

	public void SetRegion()
	{
		SetRegion(base.transform.localPosition);
	}

	public void SetRegion(Vector3 position)
	{
		SetRegion(Area?.GetRegion(position));
		pos = position;
	}

	public void SetRegion(WorldRegion newRegion)
	{
		if ((object)newRegion == null)
		{
			base.transform.SetParent(null, worldPositionStays: false);
			base.gameObject.SetActive(value: false);
			Region = null;
			return;
		}
		if ((object)Region == null)
		{
			base.gameObject.SetActive(value: true);
		}
		if (newRegion != Region)
		{
			if (this == focused)
			{
				WorldManager.EnsureActive(newRegion.transform);
				RegionChild.onFocusRegionChanged?.Invoke(Region, newRegion);
			}
			base.transform.SetParent(newRegion.transform, worldPositionStays: false);
			this.onRegionChanged?.Invoke(Region, newRegion);
			if (!newRegion.IsSameGroup(Region))
			{
				if (this == focused)
				{
					if ((bool)Region)
					{
						Region.SetVisible(value: false);
					}
					newRegion.SetVisible(value: true);
				}
				else
				{
					Visibility.Copy(base.gameObject, newRegion.gameObject);
				}
			}
			Region = newRegion;
		}
		base.transform.hasChanged = false;
	}

	public void Focus()
	{
		if (!focused)
		{
			focused = this;
			if (!camera)
			{
				camera = Camera.main;
			}
			camera.enabled = true;
			if ((bool)Region)
			{
				Region.SetVisible(value: true);
			}
		}
	}

	public void Unfocus()
	{
		if (!(focused != this))
		{
			focused = null;
			camera.enabled = false;
			if ((bool)Region)
			{
				Region.SetVisible(value: false);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(Networkarea);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteNetworkBehaviour(Networkarea);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			NetworkBehaviourSyncVar __areaNetId = ___areaNetId;
			WorldArea networkarea = Networkarea;
			___areaNetId = reader.ReadNetworkBehaviourSyncVar();
			if (!SyncVarEqual(__areaNetId, ref ___areaNetId))
			{
				OnAreaChanged(networkarea, Networkarea);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			NetworkBehaviourSyncVar __areaNetId2 = ___areaNetId;
			WorldArea networkarea2 = Networkarea;
			___areaNetId = reader.ReadNetworkBehaviourSyncVar();
			if (!SyncVarEqual(__areaNetId2, ref ___areaNetId))
			{
				OnAreaChanged(networkarea2, Networkarea);
			}
		}
	}
}
