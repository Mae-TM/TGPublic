using System;
using System.Collections;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PlayerSync : NetworkBehaviour
{
	private Transform spriteTransform;

	private CustomCharacter cuschar;

	private SpriteRenderer eyes;

	public bool local;

	[NonSerialized]
	public NetworkPlayer np;

	private Player player;

	private MSPAOrthoController mainCam;

	private Animator anim;

	public Material GetMaterial(bool hueShift = true)
	{
		if (!hueShift)
		{
			return spriteTransform.Find("Head").GetComponent<SpriteRenderer>().sharedMaterial;
		}
		return spriteTransform.Find("Body").Find("Symbol").GetComponent<SpriteRenderer>()
			.sharedMaterial;
	}

	private IEnumerator FixColour()
	{
		yield return null;
		ImageEffects.SetShiftColor(GetMaterial(), np.character.color);
	}

	public void ApplyLooks()
	{
		spriteTransform = base.transform.Find("Sprite Holder");
		anim = spriteTransform.GetComponent<Animator>();
		cuschar = GetComponent<CustomCharacter>();
		eyes = spriteTransform.Find("Head").Find("Eyes").GetComponent<SpriteRenderer>();
		SpriteRenderer component = spriteTransform.Find("Head").Find("Hair top").GetComponent<SpriteRenderer>();
		Material material = ImageEffects.SetShiftColor(local ? component.sharedMaterial : component.material, np.character.color);
		material.SetFloat("_Black", Mathf.Round(1f - np.character.whiteHair));
		SpriteRenderer component2 = spriteTransform.Find("Body").Find("Symbol").GetComponent<SpriteRenderer>();
		component2.sprite = np.character.GetSymbol();
		Material material3 = (component2.material = ImageEffects.SetShiftColor(local ? component2.sharedMaterial : component2.material, np.character.color));
		if (local)
		{
			StartCoroutine(FixColour());
		}
		cuschar.SetSpriteSheet(0, ItemDownloader.Instance.eyesBundle.LoadAssetWithSubAssets<Sprite>(np.character.eyes));
		Sprite[] to = ItemDownloader.GetHairLowerBundle(np.character.hairHighlights).LoadAssetWithSubAssets<Sprite>(np.character.hairbottom);
		cuschar.SetSpriteSheet(10, to, material);
		Sprite[] array = ItemDownloader.Instance.mouthBundle.LoadAssetWithSubAssets<Sprite>(np.character.mouth);
		if (array == null || array.Length == 0)
		{
			array = ItemDownloader.Instance.mouthBundle.LoadAssetWithSubAssets<Sprite>("aanone");
		}
		if (CharacterLook.IsHueShiftable(array[0]))
		{
			cuschar.SetSpriteSheet(1, array, material3);
		}
		else
		{
			cuschar.SetSpriteSheet(1, array);
		}
		Sprite[] array2 = ItemDownloader.Instance.shirtBundle.LoadAssetWithSubAssets<Sprite>(np.character.shirt);
		if (CharacterLook.IsHueShiftable(array2[0]))
		{
			cuschar.SetSpriteSheet(8, array2, material3);
		}
		else
		{
			cuschar.SetSpriteSheet(8, array2);
		}
		SetShirtSleeves();
		Sprite[] array3 = ItemDownloader.GetHairUpperBundle(np.character.hairHighlights).LoadAssetWithSubAssets<Sprite>(np.character.hairtop);
		cuschar.SetSpriteSheet(9, array3, material);
		component = spriteTransform.Find("Head Back").Find("Hair back").GetComponent<SpriteRenderer>();
		component.sprite = array3[1];
		component.material = material;
		Sprite[] body = ItemDownloader.GetBody(np.character.isRobot, "Arms");
		cuschar.SetSpriteSheet(11, body);
		cuschar.SetSpriteSheet(17, body);
		Sprite[] body2 = ItemDownloader.GetBody(np.character.isRobot, "Hands");
		cuschar.SetSpriteSheet(13, body2);
		cuschar.SetSpriteSheet(15, body2);
		cuschar.SetSpriteSheet(19, body2);
		Sprite[] body3 = ItemDownloader.GetBody(np.character.isRobot, "Body");
		GetComponent<CustomCharacterComplex>().Init(new Sprite[2][] { body3, body3 });
	}

	private void SetSleeves(Sprite[] sleeves, Material mat)
	{
		if (sleeves.Length == 0)
		{
			sleeves = null;
		}
		cuschar.SetSpriteSheet(12, sleeves, mat);
		cuschar.SetSpriteSheet(18, sleeves, mat);
	}

	private void SetGloves(Sprite[] gloves, Material mat)
	{
		if (gloves.Length == 0)
		{
			gloves = null;
		}
		cuschar.SetSpriteSheet(14, gloves, mat);
		cuschar.SetSpriteSheet(16, gloves, mat);
		cuschar.SetSpriteSheet(20, gloves, mat);
	}

	private void SetShirtSleeves()
	{
		Material sharedMaterial = spriteTransform.Find("Body").Find("Shirt").GetComponent<SpriteRenderer>()
			.sharedMaterial;
		SetSleeves(ItemDownloader.GetSleeves(np.character.shirt), sharedMaterial);
		SetGloves(ItemDownloader.GetGloves(np.character.shirt), sharedMaterial);
	}

	public void ChangeArmor(NormalItem item)
	{
		if (item.equipSprites == null || item.equipSprites.All((Sprite[] list) => list == null || list.Length == 0))
		{
			DisableArmor(item.armor);
			return;
		}
		Material material = GetMaterial(CharacterLook.IsHueShiftable(item));
		switch (item.armor)
		{
		case ArmorKind.Hat:
		{
			if (CharacterLook.IsHelmet(item))
			{
				spriteTransform.Find("Head").Find("Hair top").GetComponent<SpriteRenderer>()
					.enabled = false;
				spriteTransform.Find("Head Back").Find("Hair back").GetComponent<SpriteRenderer>()
					.enabled = false;
			}
			SpriteRenderer component5 = spriteTransform.Find("Head").Find("Hat").GetComponent<SpriteRenderer>();
			component5.sprite = item.equipSprites[0][0];
			component5.sharedMaterial = material;
			if (item.equipSprites[0].Length > 1)
			{
				SpriteRenderer component6 = spriteTransform.Find("Head Back").Find("Hat back").GetComponent<SpriteRenderer>();
				component6.sprite = item.equipSprites[0][1];
				component6.sharedMaterial = material;
				if (item.equipSprites[0].Length > 3)
				{
					SpriteRenderer component7 = spriteTransform.Find("Head").Find("Hat hindlayer").GetComponent<SpriteRenderer>();
					component7.sprite = item.equipSprites[0][3];
					component7.sharedMaterial = material;
				}
			}
			break;
		}
		case ArmorKind.Face:
		{
			SpriteRenderer component = spriteTransform.Find("Head").Find("Glasses").GetComponent<SpriteRenderer>();
			component.sprite = item.equipSprites[0][0];
			component.sharedMaterial = material;
			component.enabled = true;
			if (item.equipSprites[0].Length <= 1)
			{
				break;
			}
			SpriteRenderer component2 = spriteTransform.Find("Head Back").Find("Glasses back").GetComponent<SpriteRenderer>();
			component2.sprite = item.equipSprites[0][1];
			component2.sharedMaterial = material;
			component2.enabled = true;
			if (item.equipSprites[0].Length > 7)
			{
				SpriteRenderer component3 = spriteTransform.Find("Head").Find("Glasses hindlayer").GetComponent<SpriteRenderer>();
				component3.sprite = item.equipSprites[0][7];
				component3.sharedMaterial = material;
				component3.enabled = true;
				if (item.equipSprites[0].Length > 8)
				{
					SpriteRenderer component4 = spriteTransform.Find("Head Back").Find("Glasses hindlayer").GetComponent<SpriteRenderer>();
					component4.sprite = item.equipSprites[0][8];
					component4.sharedMaterial = material;
					component4.enabled = true;
				}
			}
			break;
		}
		case ArmorKind.Shirt:
			cuschar.SetSpriteSheet(6, item.equipSprites[0], material);
			cuschar.SetSpriteSheet(7, item.equipSprites[1], material);
			SetSleeves(item.equipSprites[2], material);
			SetGloves(item.equipSprites[3], material);
			break;
		case ArmorKind.Pants:
			cuschar.SetSpriteSheet(2, item.equipSprites[0], material);
			cuschar.SetSpriteSheet(3, item.equipSprites[1], material);
			break;
		case ArmorKind.Shoes:
			cuschar.SetSpriteSheet(4, item.equipSprites[0], material);
			cuschar.SetSpriteSheet(5, item.equipSprites[0], material);
			break;
		}
	}

	public void DisableArmor(ArmorKind kind)
	{
		switch (kind)
		{
		case ArmorKind.Hat:
			spriteTransform.Find("Head").Find("Hat").GetComponent<SpriteRenderer>()
				.sprite = null;
			spriteTransform.Find("Head Back").Find("Hat back").GetComponent<SpriteRenderer>()
				.sprite = null;
			spriteTransform.Find("Head").Find("Hat hindlayer").GetComponent<SpriteRenderer>()
				.sprite = null;
			spriteTransform.Find("Head").Find("Hair top").GetComponent<SpriteRenderer>()
				.enabled = true;
			spriteTransform.Find("Head Back").Find("Hair back").GetComponent<SpriteRenderer>()
				.enabled = true;
			break;
		case ArmorKind.Face:
		{
			SpriteRenderer component = spriteTransform.Find("Head").Find("Glasses").GetComponent<SpriteRenderer>();
			component.sprite = null;
			component.enabled = false;
			SpriteRenderer component2 = spriteTransform.Find("Head Back").Find("Glasses back").GetComponent<SpriteRenderer>();
			component2.sprite = null;
			component2.enabled = false;
			SpriteRenderer component3 = spriteTransform.Find("Head").Find("Glasses hindlayer").GetComponent<SpriteRenderer>();
			component3.sprite = null;
			component3.enabled = false;
			break;
		}
		case ArmorKind.Shirt:
		{
			SetShirtSleeves();
			Sprite[] armor = ItemDownloader.GetArmor("aaNone", ArmorKind.Shirt);
			cuschar.SetSpriteSheet(6, armor);
			cuschar.SetSpriteSheet(7, armor);
			break;
		}
		case ArmorKind.Pants:
		{
			Sprite[] armor = ItemDownloader.GetArmor("aaNone", ArmorKind.Pants);
			cuschar.SetSpriteSheet(2, armor);
			cuschar.SetSpriteSheet(3, armor);
			break;
		}
		case ArmorKind.Shoes:
		{
			Sprite[] armor = ItemDownloader.GetArmor("aaNone", ArmorKind.Shoes);
			cuschar.SetSpriteSheet(4, armor);
			cuschar.SetSpriteSheet(5, armor);
			break;
		}
		default:
			Debug.LogWarning($"Tried to disable armour {kind}?");
			break;
		}
	}

	private void OnDestroy()
	{
		if (!local)
		{
			GlobalChat.ScheduleRemovePlayer(np);
		}
		FlatMap.RemoveMarker(base.transform);
	}

	private void Start()
	{
		player = GetComponent<Player>();
		mainCam = MSPAOrthoController.main.GetComponent<MSPAOrthoController>();
		if (local)
		{
			np.name = MultiplayerSettings.playerName;
			if (Player.loadedPlayerData == null)
			{
				np.character = ChangeSpritePart.LoadCharacterStatic();
				ApplyLooks();
			}
		}
		else
		{
			ApplyLooks();
			GlobalChat.ScheduleAddPlayer(np);
		}
		FlatMap.AddMarker(np.character.GetSymbol(), base.transform, moving: true, colored: true);
	}

	public Vector3 GetForward(bool local = true)
	{
		Vector3 vector = 0.5f * Mathf.Sqrt(2f) * new Vector3(Mathf.Sign(base.transform.localScale.x), 0f, (!anim.GetBool("FrontFacing")) ? 1 : (-1));
		Vector3 direction = Vector3.ProjectOnPlane(mainCam.transform.forward, base.transform.up);
		direction = base.transform.InverseTransformDirection(direction);
		vector = Quaternion.FromToRotation(Vector3.forward, direction) * vector;
		if (!local)
		{
			vector = base.transform.TransformDirection(vector);
		}
		return vector;
	}

	public void Sleep(Vector3 where, string trigger)
	{
		player.SetPosition(where);
		CmdSetTrigger(trigger);
	}

	[Command]
	public void CmdSetTrigger(string trigger)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(trigger);
		SendCommandInternal(typeof(PlayerSync), "CmdSetTrigger", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcSetTrigger(int trigger)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(trigger);
		SendRPCInternal(typeof(PlayerSync), "RpcSetTrigger", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public int GetFaceState()
	{
		string text = eyes.sprite.name;
		int num = text[text.Length - 1] - 48;
		if (num > 1)
		{
			num--;
		}
		return num;
	}

	private void MirrorProcessed()
	{
	}

	public void UserCode_CmdSetTrigger(string trigger)
	{
		int trigger2 = Animator.StringToHash(trigger);
		if (!base.isClient)
		{
			anim.SetTrigger(trigger2);
		}
		RpcSetTrigger(trigger2);
	}

	protected static void InvokeUserCode_CmdSetTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTrigger called on client.");
		}
		else
		{
			((PlayerSync)obj).UserCode_CmdSetTrigger(reader.ReadString());
		}
	}

	private void UserCode_RpcSetTrigger(int trigger)
	{
		anim.SetTrigger(trigger);
	}

	protected static void InvokeUserCode_RpcSetTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetTrigger called on server.");
		}
		else
		{
			((PlayerSync)obj).UserCode_RpcSetTrigger(reader.ReadInt());
		}
	}

	static PlayerSync()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(PlayerSync), "CmdSetTrigger", InvokeUserCode_CmdSetTrigger, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(PlayerSync), "RpcSetTrigger", InvokeUserCode_RpcSetTrigger);
	}
}
