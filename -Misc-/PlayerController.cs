using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : NetworkBehaviour
{
	public bool reactToInput = true;

	private Animator spriteAnimator;

	private PlayerMovement moveCharacter;

	private MSPAOrthoController mspaCam;

	private Camera cam;

	[SyncVar(hook = "OnMovingChanged")]
	private bool moving;

	[SyncVar(hook = "OnGroundedChanged")]
	private bool grounded;

	public bool IsMoving => KeyboardControl.PlayerControls.Move.phase == InputActionPhase.Started;

	private int CameraFacing => Mathf.RoundToInt(mspaCam.cameraAngle / 90f);

	public bool Networkmoving
	{
		get
		{
			return moving;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref moving))
			{
				bool prev = moving;
				SetSyncVar(value, ref moving, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnMovingChanged(prev, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public bool Networkgrounded
	{
		get
		{
			return grounded;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref grounded))
			{
				bool prev = grounded;
				SetSyncVar(value, ref grounded, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnGroundedChanged(prev, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	private void Awake()
	{
		cam = MSPAOrthoController.main;
		mspaCam = cam.GetComponent<MSPAOrthoController>();
		spriteAnimator = base.transform.Find("Sprite Holder").GetComponent<Animator>();
	}

	public override void OnStartLocalPlayer()
	{
		Debug.Log("PlayerController.OnStartLocalPlayer()");
		moveCharacter = GetComponent<PlayerMovement>();
		KeyboardControl.PlayerControls.Jump.performed += Jump;
	}

	public void Update()
	{
		if (base.isLocalPlayer)
		{
			Vector3 move = MoveInput();
			moveCharacter.MoveChar(move.x, move.z);
			AnimationBools(move);
		}
	}

	private void AnimationBools(Vector3 move)
	{
		bool flag = moveCharacter.IsGrounded();
		if (grounded != flag)
		{
			OnGroundedChanged(grounded, flag);
			CmdSetGrounded(flag);
			Networkgrounded = flag;
		}
		bool flag2 = move.magnitude > 0.5f;
		if (moving != flag2)
		{
			OnMovingChanged(moving, flag2);
			CmdSetMoving(flag2);
			Networkmoving = flag2;
		}
	}

	private Vector3 MoveInput()
	{
		if (!reactToInput || KeyboardControl.IsKeyboardBlocked())
		{
			return Vector3.zero;
		}
		Vector2 vector = KeyboardControl.PlayerControls.Move.ReadValue<Vector2>();
		float x = vector.x;
		float y = vector.y;
		if (x != 0f)
		{
			SetRightFacing(x > 0f);
		}
		if (y != 0f)
		{
			SetFrontFacing(y < 0f);
		}
		Vector3 vector2;
		if (cam != null)
		{
			Vector3 normalized = Vector3.Project(base.transform.InverseTransformDirection(cam.transform.forward), new Vector3(1f, 0f, 1f)).normalized;
			vector2 = y * normalized + x * Vector3.Project(base.transform.InverseTransformDirection(cam.transform.right), new Vector3(1f, 0f, -1f)).normalized;
		}
		else
		{
			vector2 = y * Vector3.forward + x * Vector3.right;
		}
		return Vector3.ClampMagnitude(vector2, 1f);
	}

	private void Jump(InputAction.CallbackContext context)
	{
		if (reactToInput && !KeyboardControl.IsKeyboardBlocked())
		{
			moveCharacter.Jump();
		}
	}

	public void FaceMouse()
	{
		Vector3 vector = cam.WorldToScreenPoint(base.transform.position);
		SetRightFacing(vector.x < Input.mousePosition.x);
		SetFrontFacing(vector.y > Input.mousePosition.y);
	}

	private void SetFrontFacing(bool to)
	{
		CmdSetFacing((byte)(((!to) ? 2 : 0) - CameraFacing));
	}

	private void SetRightFacing(bool to)
	{
		CmdSetFacing((byte)((to ? 1 : 3) - CameraFacing));
	}

	[Command]
	private void CmdSetFacing(byte to)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		NetworkWriterExtensions.WriteByte(writer, to);
		SendCommandInternal(typeof(PlayerController), "CmdSetFacing", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcSetFacing(byte to)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		NetworkWriterExtensions.WriteByte(writer, to);
		SendRPCInternal(typeof(PlayerController), "RpcSetFacing", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSetGrounded(bool to)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(to);
		SendCommandInternal(typeof(PlayerController), "CmdSetGrounded", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSetMoving(bool to)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(to);
		SendCommandInternal(typeof(PlayerController), "CmdSetMoving", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnGroundedChanged(bool prev, bool to)
	{
		spriteAnimator.SetBool("Grounded", to);
	}

	private void OnMovingChanged(bool prev, bool to)
	{
		spriteAnimator.SetFloat("Speed", to ? 1f : 0f);
	}

	private void MirrorProcessed()
	{
	}

	private void UserCode_CmdSetFacing(byte to)
	{
		RpcSetFacing(to);
	}

	protected static void InvokeUserCode_CmdSetFacing(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetFacing called on client.");
		}
		else
		{
			((PlayerController)obj).UserCode_CmdSetFacing(NetworkReaderExtensions.ReadByte(reader));
		}
	}

	private void UserCode_RpcSetFacing(byte to)
	{
		int num = to + CameraFacing;
		if ((num & 1) == 0)
		{
			AnimatorStateInfo currentAnimatorStateInfo = spriteAnimator.GetCurrentAnimatorStateInfo(1);
			bool value = (num & 2) == 0 || !currentAnimatorStateInfo.IsTag("4Directional");
			spriteAnimator.SetBool("FrontFacing", value);
		}
		else
		{
			Vector3 localScale = base.transform.localScale;
			localScale.x = Mathf.Abs(localScale.x) * (float)(((num & 2) == 0) ? 1 : (-1));
			base.transform.localScale = localScale;
		}
	}

	protected static void InvokeUserCode_RpcSetFacing(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetFacing called on server.");
		}
		else
		{
			((PlayerController)obj).UserCode_RpcSetFacing(NetworkReaderExtensions.ReadByte(reader));
		}
	}

	private void UserCode_CmdSetGrounded(bool to)
	{
		Networkgrounded = to;
	}

	protected static void InvokeUserCode_CmdSetGrounded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetGrounded called on client.");
		}
		else
		{
			((PlayerController)obj).UserCode_CmdSetGrounded(reader.ReadBool());
		}
	}

	private void UserCode_CmdSetMoving(bool to)
	{
		Networkmoving = to;
	}

	protected static void InvokeUserCode_CmdSetMoving(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetMoving called on client.");
		}
		else
		{
			((PlayerController)obj).UserCode_CmdSetMoving(reader.ReadBool());
		}
	}

	static PlayerController()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(PlayerController), "CmdSetFacing", InvokeUserCode_CmdSetFacing, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(PlayerController), "CmdSetGrounded", InvokeUserCode_CmdSetGrounded, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(PlayerController), "CmdSetMoving", InvokeUserCode_CmdSetMoving, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(PlayerController), "RpcSetFacing", InvokeUserCode_RpcSetFacing);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(moving);
			writer.WriteBool(grounded);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(moving);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(grounded);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = moving;
			Networkmoving = reader.ReadBool();
			if (!SyncVarEqual(flag, ref moving))
			{
				OnMovingChanged(flag, moving);
			}
			bool flag2 = grounded;
			Networkgrounded = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref grounded))
			{
				OnGroundedChanged(flag2, grounded);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag3 = moving;
			Networkmoving = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref moving))
			{
				OnMovingChanged(flag3, moving);
			}
		}
		if ((num & 2L) != 0L)
		{
			bool flag4 = grounded;
			Networkgrounded = reader.ReadBool();
			if (!SyncVarEqual(flag4, ref grounded))
			{
				OnGroundedChanged(flag4, grounded);
			}
		}
	}
}
