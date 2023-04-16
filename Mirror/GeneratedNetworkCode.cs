using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quest.NET.Enums;
using UnityEngine;

namespace Mirror;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public static class GeneratedNetworkCode
{
	public static ReadyMessage _Read_Mirror_002EReadyMessage(NetworkReader reader)
	{
		return default(ReadyMessage);
	}

	public static void _Write_Mirror_002EReadyMessage(NetworkWriter writer, ReadyMessage value)
	{
	}

	public static NotReadyMessage _Read_Mirror_002ENotReadyMessage(NetworkReader reader)
	{
		return default(NotReadyMessage);
	}

	public static void _Write_Mirror_002ENotReadyMessage(NetworkWriter writer, NotReadyMessage value)
	{
	}

	public static AddPlayerMessage _Read_Mirror_002EAddPlayerMessage(NetworkReader reader)
	{
		return default(AddPlayerMessage);
	}

	public static void _Write_Mirror_002EAddPlayerMessage(NetworkWriter writer, AddPlayerMessage value)
	{
	}

	public static SceneMessage _Read_Mirror_002ESceneMessage(NetworkReader reader)
	{
		SceneMessage result = default(SceneMessage);
		result.sceneName = reader.ReadString();
		result.sceneOperation = _Read_Mirror_002ESceneOperation(reader);
		result.customHandling = reader.ReadBool();
		return result;
	}

	public static SceneOperation _Read_Mirror_002ESceneOperation(NetworkReader reader)
	{
		return (SceneOperation)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_Mirror_002ESceneMessage(NetworkWriter writer, SceneMessage value)
	{
		writer.WriteString(value.sceneName);
		_Write_Mirror_002ESceneOperation(writer, value.sceneOperation);
		writer.WriteBool(value.customHandling);
	}

	public static void _Write_Mirror_002ESceneOperation(NetworkWriter writer, SceneOperation value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static CommandMessage _Read_Mirror_002ECommandMessage(NetworkReader reader)
	{
		CommandMessage result = default(CommandMessage);
		result.netId = reader.ReadUInt();
		result.componentIndex = reader.ReadInt();
		result.functionHash = reader.ReadInt();
		result.payload = reader.ReadBytesAndSizeSegment();
		return result;
	}

	public static void _Write_Mirror_002ECommandMessage(NetworkWriter writer, CommandMessage value)
	{
		writer.WriteUInt(value.netId);
		writer.WriteInt(value.componentIndex);
		writer.WriteInt(value.functionHash);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static RpcMessage _Read_Mirror_002ERpcMessage(NetworkReader reader)
	{
		RpcMessage result = default(RpcMessage);
		result.netId = reader.ReadUInt();
		result.componentIndex = reader.ReadInt();
		result.functionHash = reader.ReadInt();
		result.payload = reader.ReadBytesAndSizeSegment();
		return result;
	}

	public static void _Write_Mirror_002ERpcMessage(NetworkWriter writer, RpcMessage value)
	{
		writer.WriteUInt(value.netId);
		writer.WriteInt(value.componentIndex);
		writer.WriteInt(value.functionHash);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static SpawnMessage _Read_Mirror_002ESpawnMessage(NetworkReader reader)
	{
		SpawnMessage result = default(SpawnMessage);
		result.netId = reader.ReadUInt();
		result.isLocalPlayer = reader.ReadBool();
		result.isOwner = reader.ReadBool();
		result.sceneId = reader.ReadULong();
		result.assetId = reader.ReadGuid();
		result.position = reader.ReadVector3();
		result.rotation = reader.ReadQuaternion();
		result.scale = reader.ReadVector3();
		result.payload = reader.ReadBytesAndSizeSegment();
		return result;
	}

	public static void _Write_Mirror_002ESpawnMessage(NetworkWriter writer, SpawnMessage value)
	{
		writer.WriteUInt(value.netId);
		writer.WriteBool(value.isLocalPlayer);
		writer.WriteBool(value.isOwner);
		writer.WriteULong(value.sceneId);
		writer.WriteGuid(value.assetId);
		writer.WriteVector3(value.position);
		writer.WriteQuaternion(value.rotation);
		writer.WriteVector3(value.scale);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static ObjectSpawnStartedMessage _Read_Mirror_002EObjectSpawnStartedMessage(NetworkReader reader)
	{
		return default(ObjectSpawnStartedMessage);
	}

	public static void _Write_Mirror_002EObjectSpawnStartedMessage(NetworkWriter writer, ObjectSpawnStartedMessage value)
	{
	}

	public static ObjectSpawnFinishedMessage _Read_Mirror_002EObjectSpawnFinishedMessage(NetworkReader reader)
	{
		return default(ObjectSpawnFinishedMessage);
	}

	public static void _Write_Mirror_002EObjectSpawnFinishedMessage(NetworkWriter writer, ObjectSpawnFinishedMessage value)
	{
	}

	public static ObjectDestroyMessage _Read_Mirror_002EObjectDestroyMessage(NetworkReader reader)
	{
		ObjectDestroyMessage result = default(ObjectDestroyMessage);
		result.netId = reader.ReadUInt();
		return result;
	}

	public static void _Write_Mirror_002EObjectDestroyMessage(NetworkWriter writer, ObjectDestroyMessage value)
	{
		writer.WriteUInt(value.netId);
	}

	public static ObjectHideMessage _Read_Mirror_002EObjectHideMessage(NetworkReader reader)
	{
		ObjectHideMessage result = default(ObjectHideMessage);
		result.netId = reader.ReadUInt();
		return result;
	}

	public static void _Write_Mirror_002EObjectHideMessage(NetworkWriter writer, ObjectHideMessage value)
	{
		writer.WriteUInt(value.netId);
	}

	public static EntityStateMessage _Read_Mirror_002EEntityStateMessage(NetworkReader reader)
	{
		EntityStateMessage result = default(EntityStateMessage);
		result.netId = reader.ReadUInt();
		result.payload = reader.ReadBytesAndSizeSegment();
		return result;
	}

	public static void _Write_Mirror_002EEntityStateMessage(NetworkWriter writer, EntityStateMessage value)
	{
		writer.WriteUInt(value.netId);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static NetworkPingMessage _Read_Mirror_002ENetworkPingMessage(NetworkReader reader)
	{
		NetworkPingMessage result = default(NetworkPingMessage);
		result.clientTime = reader.ReadDouble();
		return result;
	}

	public static void _Write_Mirror_002ENetworkPingMessage(NetworkWriter writer, NetworkPingMessage value)
	{
		writer.WriteDouble(value.clientTime);
	}

	public static NetworkPongMessage _Read_Mirror_002ENetworkPongMessage(NetworkReader reader)
	{
		NetworkPongMessage result = default(NetworkPongMessage);
		result.clientTime = reader.ReadDouble();
		result.serverTime = reader.ReadDouble();
		return result;
	}

	public static void _Write_Mirror_002ENetworkPongMessage(NetworkWriter writer, NetworkPongMessage value)
	{
		writer.WriteDouble(value.clientTime);
		writer.WriteDouble(value.serverTime);
	}

	public static PlayerData _Read_PlayerData(NetworkReader reader)
	{
		PlayerData result = default(PlayerData);
		result.name = reader.ReadString();
		result.character = _Read_CharacterSettings(reader);
		result.level = reader.ReadUInt();
		result.experience = reader.ReadFloat();
		result.grist = _Read_System_002EInt32_005B_005D(reader);
		result.role = _Read_Class(reader);
		result.aspect = _Read_Aspect(reader);
		result.kernelSprite = _Read_KernelSpriteData(reader);
		result.exile = _Read_ExileData(reader);
		result.sylladex = _Read_SylladexData(reader);
		result.armor = _Read_Item_005B_005D(reader);
		return result;
	}

	public static CharacterSettings _Read_CharacterSettings(NetworkReader reader)
	{
		CharacterSettings result = default(CharacterSettings);
		result.eyes = reader.ReadString();
		result.mouth = reader.ReadString();
		result.shirt = reader.ReadString();
		result.hairtop = reader.ReadString();
		result.hairbottom = reader.ReadString();
		result.symbol = reader.ReadInt();
		result.whiteHair = reader.ReadFloat();
		result.color = _Read_PBColor(reader);
		result.isRobot = reader.ReadBool();
		result.customSymbol = reader.ReadBytesAndSize();
		result.hairHighlights = _Read_HairHighlights(reader);
		return result;
	}

	public static PBColor _Read_PBColor(NetworkReader reader)
	{
		PBColor result = default(PBColor);
		result.h = reader.ReadFloat();
		result.s = reader.ReadFloat();
		result.v = reader.ReadFloat();
		return result;
	}

	public static HairHighlights _Read_HairHighlights(NetworkReader reader)
	{
		return (HairHighlights)reader.ReadInt();
	}

	public static int[] _Read_System_002EInt32_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<int>();
	}

	public static Class _Read_Class(NetworkReader reader)
	{
		return (Class)reader.ReadInt();
	}

	public static Aspect _Read_Aspect(NetworkReader reader)
	{
		return (Aspect)reader.ReadInt();
	}

	public static KernelSpriteData _Read_KernelSpriteData(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		KernelSpriteData kernelSpriteData = new KernelSpriteData();
		kernelSpriteData.hasEntered = reader.ReadBool();
		kernelSpriteData.prototypes = _Read_System_002EString_005B_005D(reader);
		return kernelSpriteData;
	}

	public static string[] _Read_System_002EString_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<string>();
	}

	public static ExileData _Read_ExileData(NetworkReader reader)
	{
		ExileData result = default(ExileData);
		result.action = _Read_Exile_002FAction(reader);
		result.isTalking = reader.ReadBool();
		result.type = reader.ReadUInt();
		return result;
	}

	public static Exile.Action _Read_Exile_002FAction(NetworkReader reader)
	{
		return (Exile.Action)reader.ReadInt();
	}

	public static SylladexData _Read_SylladexData(NetworkReader reader)
	{
		SylladexData result = default(SylladexData);
		result.modus = reader.ReadString();
		result.modusData = _Read_ModusData(reader);
		result.specibus = _Read_Specibus_002FData(reader);
		result.quests = _Read_QuestData_005B_005D(reader);
		result.characterName = reader.ReadString();
		return result;
	}

	public static ModusData _Read_ModusData(NetworkReader reader)
	{
		ModusData result = default(ModusData);
		result.capacity = reader.ReadInt();
		result.modusSpecificData = reader.ReadBytesAndSize();
		return result;
	}

	public static Specibus.Data _Read_Specibus_002FData(NetworkReader reader)
	{
		Specibus.Data result = default(Specibus.Data);
		result.size = reader.ReadInt();
		result.weapons = _Read_HouseData_002FItem_005B_005D(reader);
		return result;
	}

	public static HouseData.Item[] _Read_HouseData_002FItem_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<HouseData.Item>();
	}

	public static QuestData[] _Read_QuestData_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<QuestData>();
	}

	public static QuestData _Read_QuestData(NetworkReader reader)
	{
		QuestData result = default(QuestData);
		result.questId = reader.ReadString();
		result.status = _Read_Quest_002ENET_002EEnums_002EQuestStatus(reader);
		result.objectives = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EByte_005B_005D_003E(reader);
		return result;
	}

	public static QuestStatus _Read_Quest_002ENET_002EEnums_002EQuestStatus(NetworkReader reader)
	{
		return (QuestStatus)reader.ReadInt();
	}

	public static List<byte[]> _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EByte_005B_005D_003E(NetworkReader reader)
	{
		return reader.ReadList<byte[]>();
	}

	public static Item[] _Read_Item_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<Item>();
	}

	public static void _Write_PlayerData(NetworkWriter writer, PlayerData value)
	{
		writer.WriteString(value.name);
		_Write_CharacterSettings(writer, value.character);
		writer.WriteUInt(value.level);
		writer.WriteFloat(value.experience);
		_Write_System_002EInt32_005B_005D(writer, value.grist);
		_Write_Class(writer, value.role);
		_Write_Aspect(writer, value.aspect);
		_Write_KernelSpriteData(writer, value.kernelSprite);
		_Write_ExileData(writer, value.exile);
		_Write_SylladexData(writer, value.sylladex);
		_Write_Item_005B_005D(writer, value.armor);
	}

	public static void _Write_CharacterSettings(NetworkWriter writer, CharacterSettings value)
	{
		writer.WriteString(value.eyes);
		writer.WriteString(value.mouth);
		writer.WriteString(value.shirt);
		writer.WriteString(value.hairtop);
		writer.WriteString(value.hairbottom);
		writer.WriteInt(value.symbol);
		writer.WriteFloat(value.whiteHair);
		_Write_PBColor(writer, value.color);
		writer.WriteBool(value.isRobot);
		writer.WriteBytesAndSize(value.customSymbol);
		_Write_HairHighlights(writer, value.hairHighlights);
	}

	public static void _Write_PBColor(NetworkWriter writer, PBColor value)
	{
		writer.WriteFloat(value.h);
		writer.WriteFloat(value.s);
		writer.WriteFloat(value.v);
	}

	public static void _Write_HairHighlights(NetworkWriter writer, HairHighlights value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_System_002EInt32_005B_005D(NetworkWriter writer, int[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_Class(NetworkWriter writer, Class value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_Aspect(NetworkWriter writer, Aspect value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_KernelSpriteData(NetworkWriter writer, KernelSpriteData value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteBool(value.hasEntered);
		_Write_System_002EString_005B_005D(writer, value.prototypes);
	}

	public static void _Write_System_002EString_005B_005D(NetworkWriter writer, string[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_ExileData(NetworkWriter writer, ExileData value)
	{
		_Write_Exile_002FAction(writer, value.action);
		writer.WriteBool(value.isTalking);
		writer.WriteUInt(value.type);
	}

	public static void _Write_Exile_002FAction(NetworkWriter writer, Exile.Action value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_SylladexData(NetworkWriter writer, SylladexData value)
	{
		writer.WriteString(value.modus);
		_Write_ModusData(writer, value.modusData);
		_Write_Specibus_002FData(writer, value.specibus);
		_Write_QuestData_005B_005D(writer, value.quests);
		writer.WriteString(value.characterName);
	}

	public static void _Write_ModusData(NetworkWriter writer, ModusData value)
	{
		writer.WriteInt(value.capacity);
		writer.WriteBytesAndSize(value.modusSpecificData);
	}

	public static void _Write_Specibus_002FData(NetworkWriter writer, Specibus.Data value)
	{
		writer.WriteInt(value.size);
		_Write_HouseData_002FItem_005B_005D(writer, value.weapons);
	}

	public static void _Write_HouseData_002FItem_005B_005D(NetworkWriter writer, HouseData.Item[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_QuestData_005B_005D(NetworkWriter writer, QuestData[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_QuestData(NetworkWriter writer, QuestData value)
	{
		writer.WriteString(value.questId);
		_Write_Quest_002ENET_002EEnums_002EQuestStatus(writer, value.status);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EByte_005B_005D_003E(writer, value.objectives);
	}

	public static void _Write_Quest_002ENET_002EEnums_002EQuestStatus(NetworkWriter writer, QuestStatus value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EByte_005B_005D_003E(NetworkWriter writer, List<byte[]> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_Item_005B_005D(NetworkWriter writer, Item[] value)
	{
		writer.WriteArray(value);
	}

	public static Furniture.SpawnMessage _Read_Furniture_002FSpawnMessage(NetworkReader reader)
	{
		Furniture.SpawnMessage result = default(Furniture.SpawnMessage);
		result.furniture = reader.ReadString();
		result.building = reader.ReadNetworkBehaviour();
		return result;
	}

	public static void _Write_Furniture_002FSpawnMessage(NetworkWriter writer, Furniture.SpawnMessage value)
	{
		writer.WriteString(value.furniture);
		writer.WriteNetworkBehaviour(value.building);
	}

	public static AAPoly.ShortChains _Read_AAPoly_002FShortChains(NetworkReader reader)
	{
		AAPoly.ShortChains result = default(AAPoly.ShortChains);
		result.corners = _Read_System_002EInt32_005B_005D(reader);
		result.chainLengths = _Read_System_002EInt32_005B_005D(reader);
		return result;
	}

	public static void _Write_AAPoly_002FShortChains(NetworkWriter writer, AAPoly.ShortChains value)
	{
		_Write_System_002EInt32_005B_005D(writer, value.corners);
		_Write_System_002EInt32_005B_005D(writer, value.chainLengths);
	}

	public static HouseData _Read_HouseData(NetworkReader reader)
	{
		HouseData result = default(HouseData);
		result.version = reader.ReadUShort();
		result.stories = _Read_HouseData_002FStory_005B_005D(reader);
		result.spawnPosition = reader.ReadVector3Int();
		result.background = reader.ReadString();
		result.items = _Read_HouseData_002FDroppedItem_005B_005D(reader);
		result.attackables = _Read_HouseData_002FAttackable_005B_005D(reader);
		return result;
	}

	public static HouseData.Story[] _Read_HouseData_002FStory_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<HouseData.Story>();
	}

	public static HouseData.Story _Read_HouseData_002FStory(NetworkReader reader)
	{
		HouseData.Story result = default(HouseData.Story);
		result.rooms = _Read_AAPoly_005B_005D(reader);
		result.furniture = _Read_HouseData_002FFurniture_005B_005D(reader);
		result.brokenGround = _Read_UnityEngine_002ERectInt_005B_005D(reader);
		return result;
	}

	public static AAPoly[] _Read_AAPoly_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<AAPoly>();
	}

	public static HouseData.Furniture[] _Read_HouseData_002FFurniture_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<HouseData.Furniture>();
	}

	public static HouseData.Furniture _Read_HouseData_002FFurniture(NetworkReader reader)
	{
		HouseData.Furniture result = default(HouseData.Furniture);
		result.name = reader.ReadString();
		result.x = reader.ReadInt();
		result.z = reader.ReadInt();
		result.orientation = _Read_Orientation(reader);
		result.items = _Read_HouseData_002FItem_005B_005D(reader);
		return result;
	}

	public static Orientation _Read_Orientation(NetworkReader reader)
	{
		return (Orientation)NetworkReaderExtensions.ReadByte(reader);
	}

	public static RectInt[] _Read_UnityEngine_002ERectInt_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<RectInt>();
	}

	public static HouseData.DroppedItem[] _Read_HouseData_002FDroppedItem_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<HouseData.DroppedItem>();
	}

	public static HouseData.DroppedItem _Read_HouseData_002FDroppedItem(NetworkReader reader)
	{
		HouseData.DroppedItem result = default(HouseData.DroppedItem);
		result.item = reader.ReadItemData();
		result.pos = reader.ReadVector3();
		result.rot = reader.ReadVector3();
		return result;
	}

	public static HouseData.Attackable[] _Read_HouseData_002FAttackable_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<HouseData.Attackable>();
	}

	public static void _Write_HouseData(NetworkWriter writer, HouseData value)
	{
		writer.WriteUShort(value.version);
		_Write_HouseData_002FStory_005B_005D(writer, value.stories);
		writer.WriteVector3Int(value.spawnPosition);
		writer.WriteString(value.background);
		_Write_HouseData_002FDroppedItem_005B_005D(writer, value.items);
		_Write_HouseData_002FAttackable_005B_005D(writer, value.attackables);
	}

	public static void _Write_HouseData_002FStory_005B_005D(NetworkWriter writer, HouseData.Story[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_HouseData_002FStory(NetworkWriter writer, HouseData.Story value)
	{
		_Write_AAPoly_005B_005D(writer, value.rooms);
		_Write_HouseData_002FFurniture_005B_005D(writer, value.furniture);
		_Write_UnityEngine_002ERectInt_005B_005D(writer, value.brokenGround);
	}

	public static void _Write_AAPoly_005B_005D(NetworkWriter writer, AAPoly[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_HouseData_002FFurniture_005B_005D(NetworkWriter writer, HouseData.Furniture[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_HouseData_002FFurniture(NetworkWriter writer, HouseData.Furniture value)
	{
		writer.WriteString(value.name);
		writer.WriteInt(value.x);
		writer.WriteInt(value.z);
		_Write_Orientation(writer, value.orientation);
		_Write_HouseData_002FItem_005B_005D(writer, value.items);
	}

	public static void _Write_Orientation(NetworkWriter writer, Orientation value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static void _Write_UnityEngine_002ERectInt_005B_005D(NetworkWriter writer, RectInt[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_HouseData_002FDroppedItem_005B_005D(NetworkWriter writer, HouseData.DroppedItem[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_HouseData_002FDroppedItem(NetworkWriter writer, HouseData.DroppedItem value)
	{
		writer.WriteItemData(value.item);
		writer.WriteVector3(value.pos);
		writer.WriteVector3(value.rot);
	}

	public static void _Write_HouseData_002FAttackable_005B_005D(NetworkWriter writer, HouseData.Attackable[] value)
	{
		writer.WriteArray(value);
	}

	public static HouseData.NormalItem _Read_HouseData_002FNormalItem(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		HouseData.NormalItem normalItem = new HouseData.NormalItem();
		normalItem.code = reader.ReadString();
		normalItem.contents = _Read_HouseData_002FItem_005B_005D(reader);
		normalItem.isEntry = reader.ReadBool();
		return normalItem;
	}

	public static void _Write_HouseData_002FNormalItem(NetworkWriter writer, HouseData.NormalItem value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteString(value.code);
		_Write_HouseData_002FItem_005B_005D(writer, value.contents);
		writer.WriteBool(value.isEntry);
	}

	public static HouseData.Totem _Read_HouseData_002FTotem(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		HouseData.Totem totem = new HouseData.Totem();
		totem.result = reader.ReadItemData();
		totem.color = reader.ReadVector3();
		return totem;
	}

	public static void _Write_HouseData_002FTotem(NetworkWriter writer, HouseData.Totem value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteItemData(value.result);
		writer.WriteVector3(value.color);
	}

	public static HouseData.PunchCard _Read_HouseData_002FPunchCard(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		HouseData.PunchCard punchCard = new HouseData.PunchCard();
		punchCard.result = reader.ReadItemData();
		punchCard.original = reader.ReadItemData();
		return punchCard;
	}

	public static void _Write_HouseData_002FPunchCard(NetworkWriter writer, HouseData.PunchCard value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteItemData(value.result);
		writer.WriteItemData(value.original);
	}

	public static HouseData.AlchemyItem _Read_HouseData_002FAlchemyItem(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		HouseData.AlchemyItem alchemyItem = new HouseData.AlchemyItem();
		alchemyItem.code = reader.ReadString();
		alchemyItem.name = reader.ReadString();
		alchemyItem.power = reader.ReadFloat();
		alchemyItem.speed = reader.ReadFloat();
		alchemyItem.size = reader.ReadFloat();
		alchemyItem.animation = reader.ReadString();
		alchemyItem.weaponKind = _Read_WeaponKind(reader);
		alchemyItem.armor = _Read_ArmorKind(reader);
		alchemyItem.tags = _Read_NormalItem_002FTag_005B_005D(reader);
		alchemyItem.customTags = _Read_NormalItem_002FTag_005B_005D(reader);
		alchemyItem.equipSprite = reader.ReadString();
		alchemyItem.sprite = reader.ReadString();
		return alchemyItem;
	}

	public static WeaponKind _Read_WeaponKind(NetworkReader reader)
	{
		return (WeaponKind)reader.ReadInt();
	}

	public static ArmorKind _Read_ArmorKind(NetworkReader reader)
	{
		return (ArmorKind)reader.ReadInt();
	}

	public static NormalItem.Tag[] _Read_NormalItem_002FTag_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<NormalItem.Tag>();
	}

	public static NormalItem.Tag _Read_NormalItem_002FTag(NetworkReader reader)
	{
		return (NormalItem.Tag)reader.ReadInt();
	}

	public static void _Write_HouseData_002FAlchemyItem(NetworkWriter writer, HouseData.AlchemyItem value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteString(value.code);
		writer.WriteString(value.name);
		writer.WriteFloat(value.power);
		writer.WriteFloat(value.speed);
		writer.WriteFloat(value.size);
		writer.WriteString(value.animation);
		_Write_WeaponKind(writer, value.weaponKind);
		_Write_ArmorKind(writer, value.armor);
		_Write_NormalItem_002FTag_005B_005D(writer, value.tags);
		_Write_NormalItem_002FTag_005B_005D(writer, value.customTags);
		writer.WriteString(value.equipSprite);
		writer.WriteString(value.sprite);
	}

	public static void _Write_WeaponKind(NetworkWriter writer, WeaponKind value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_ArmorKind(NetworkWriter writer, ArmorKind value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_NormalItem_002FTag_005B_005D(NetworkWriter writer, NormalItem.Tag[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_NormalItem_002FTag(NetworkWriter writer, NormalItem.Tag value)
	{
		writer.WriteInt((int)value);
	}

	public static SyncedInteractableAction.Message _Read_SyncedInteractableAction_002FMessage(NetworkReader reader)
	{
		SyncedInteractableAction.Message result = default(SyncedInteractableAction.Message);
		result.identity = reader.ReadNetworkIdentity();
		return result;
	}

	public static void _Write_SyncedInteractableAction_002FMessage(NetworkWriter writer, SyncedInteractableAction.Message value)
	{
		writer.WriteNetworkIdentity(value.identity);
	}

	public static PlayerJoinMessage _Read_PlayerJoinMessage(NetworkReader reader)
	{
		PlayerJoinMessage result = default(PlayerJoinMessage);
		result.id = reader.ReadInt();
		result.house = _Read_HouseData(reader);
		result.data = _Read_PlayerData(reader);
		return result;
	}

	public static void _Write_PlayerJoinMessage(NetworkWriter writer, PlayerJoinMessage value)
	{
		writer.WriteInt(value.id);
		_Write_HouseData(writer, value.house);
		_Write_PlayerData(writer, value.data);
	}

	public static PesterchumMessage _Read_PesterchumMessage(NetworkReader reader)
	{
		PesterchumMessage result = default(PesterchumMessage);
		result.sender = reader.ReadString();
		result.receiver = reader.ReadString();
		result.message = reader.ReadString();
		result.color = reader.ReadString();
		return result;
	}

	public static void _Write_PesterchumMessage(NetworkWriter writer, PesterchumMessage value)
	{
		writer.WriteString(value.sender);
		writer.WriteString(value.receiver);
		writer.WriteString(value.message);
		writer.WriteString(value.color);
	}

	public static PesterchumStatusChange _Read_PesterchumStatusChange(NetworkReader reader)
	{
		PesterchumStatusChange result = default(PesterchumStatusChange);
		result.sender = reader.ReadString();
		result.receiver = reader.ReadString();
		result.status = reader.ReadBool();
		return result;
	}

	public static void _Write_PesterchumStatusChange(NetworkWriter writer, PesterchumStatusChange value)
	{
		writer.WriteString(value.sender);
		writer.WriteString(value.receiver);
		writer.WriteBool(value.status);
	}

	public static RandomSync _Read_RandomSync(NetworkReader reader)
	{
		RandomSync result = default(RandomSync);
		result.random = reader.ReadBytesAndSize();
		return result;
	}

	public static void _Write_RandomSync(NetworkWriter writer, RandomSync value)
	{
		writer.WriteBytesAndSize(value.random);
	}

	public static HostSaveRequest _Read_HostSaveRequest(NetworkReader reader)
	{
		return default(HostSaveRequest);
	}

	public static void _Write_HostSaveRequest(NetworkWriter writer, HostSaveRequest value)
	{
	}

	public static ClientSaveResponse _Read_ClientSaveResponse(NetworkReader reader)
	{
		ClientSaveResponse result = default(ClientSaveResponse);
		result.player = _Read_SessionPlayer(reader);
		return result;
	}

	public static SessionPlayer _Read_SessionPlayer(NetworkReader reader)
	{
		SessionPlayer result = default(SessionPlayer);
		result.name = reader.ReadString();
		result.character = _Read_CharacterSettings(reader);
		result.level = reader.ReadUInt();
		result.experience = reader.ReadFloat();
		result.grist = _Read_System_002EInt32_005B_005D(reader);
		result.role = _Read_Class(reader);
		result.aspect = _Read_Aspect(reader);
		result.kernelSprite = _Read_KernelSpriteData(reader);
		result.exile = _Read_ExileData(reader);
		result.sylladex = _Read_SylladexData(reader);
		result.armor = _Read_HouseData_002FItem_005B_005D(reader);
		result.currentArea = reader.ReadInt();
		result.position = reader.ReadVector3();
		return result;
	}

	public static void _Write_ClientSaveResponse(NetworkWriter writer, ClientSaveResponse value)
	{
		_Write_SessionPlayer(writer, value.player);
	}

	public static void _Write_SessionPlayer(NetworkWriter writer, SessionPlayer value)
	{
		writer.WriteString(value.name);
		_Write_CharacterSettings(writer, value.character);
		writer.WriteUInt(value.level);
		writer.WriteFloat(value.experience);
		_Write_System_002EInt32_005B_005D(writer, value.grist);
		_Write_Class(writer, value.role);
		_Write_Aspect(writer, value.aspect);
		_Write_KernelSpriteData(writer, value.kernelSprite);
		_Write_ExileData(writer, value.exile);
		_Write_SylladexData(writer, value.sylladex);
		_Write_HouseData_002FItem_005B_005D(writer, value.armor);
		writer.WriteInt(value.currentArea);
		writer.WriteVector3(value.position);
	}

	public static HostLoadRequest _Read_HostLoadRequest(NetworkReader reader)
	{
		HostLoadRequest result = default(HostLoadRequest);
		result.sylladex = _Read_SylladexData(reader);
		result.exile = _Read_ExileData(reader);
		return result;
	}

	public static void _Write_HostLoadRequest(NetworkWriter writer, HostLoadRequest value)
	{
		_Write_SylladexData(writer, value.sylladex);
		_Write_ExileData(writer, value.exile);
	}

	public static StatusEffect.Data _Read_StatusEffect_002FData(NetworkReader reader)
	{
		StatusEffect.Data result = default(StatusEffect.Data);
		result.type = reader.ReadString();
		result.protoData = reader.ReadBytesAndSize();
		result.endTime = reader.ReadFloat();
		return result;
	}

	public static void _Write_StatusEffect_002FData(NetworkWriter writer, StatusEffect.Data value)
	{
		writer.WriteString(value.type);
		writer.WriteBytesAndSize(value.protoData);
		writer.WriteFloat(value.endTime);
	}

	public static Item.SpawnMessage _Read_Item_002FSpawnMessage(NetworkReader reader)
	{
		Item.SpawnMessage result = default(Item.SpawnMessage);
		result.item = reader.ReadItem();
		result.area = reader.ReadNetworkBehaviour();
		result.position = reader.ReadVector3();
		result.asOwner = reader.ReadBool();
		return result;
	}

	public static void _Write_Item_002FSpawnMessage(NetworkWriter writer, Item.SpawnMessage value)
	{
		writer.WriteItem(value.item);
		writer.WriteNetworkBehaviour(value.area);
		writer.WriteVector3(value.position);
		writer.WriteBool(value.asOwner);
	}

	public static void _Write_BuildingChanges(NetworkWriter writer, BuildingChanges value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FChange_003E(writer, value.changes);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FRoomTransfer_003E(writer, value.transfers);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FChange_003E(NetworkWriter writer, List<BuildingChanges.Change> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_BuildingChanges_002FChange(NetworkWriter writer, BuildingChanges.Change value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteInt(value.story);
		writer.WriteInt(value.room);
		writer.WriteSides(value.changes);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FRoomTransfer_003E(NetworkWriter writer, List<BuildingChanges.RoomTransfer> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_BuildingChanges_002FRoomTransfer(NetworkWriter writer, BuildingChanges.RoomTransfer value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteInt(value.story);
		writer.WriteInt(value.from);
		writer.WriteInt(value.to);
		writer.WriteSides(value.changes);
	}

	public static BuildingChanges _Read_BuildingChanges(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		BuildingChanges buildingChanges = new BuildingChanges();
		buildingChanges.changes = _Read_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FChange_003E(reader);
		buildingChanges.transfers = _Read_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FRoomTransfer_003E(reader);
		return buildingChanges;
	}

	public static List<BuildingChanges.Change> _Read_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FChange_003E(NetworkReader reader)
	{
		return reader.ReadList<BuildingChanges.Change>();
	}

	public static BuildingChanges.Change _Read_BuildingChanges_002FChange(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		BuildingChanges.Change change = new BuildingChanges.Change();
		change.story = reader.ReadInt();
		change.room = reader.ReadInt();
		change.changes = reader.ReadSides();
		return change;
	}

	public static List<BuildingChanges.RoomTransfer> _Read_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FRoomTransfer_003E(NetworkReader reader)
	{
		return reader.ReadList<BuildingChanges.RoomTransfer>();
	}

	public static BuildingChanges.RoomTransfer _Read_BuildingChanges_002FRoomTransfer(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		BuildingChanges.RoomTransfer roomTransfer = new BuildingChanges.RoomTransfer();
		roomTransfer.story = reader.ReadInt();
		roomTransfer.from = reader.ReadInt();
		roomTransfer.to = reader.ReadInt();
		roomTransfer.changes = reader.ReadSides();
		return roomTransfer;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitReadWriters()
	{
		Writer<byte>.write = NetworkWriterExtensions.WriteByte;
		Writer<sbyte>.write = NetworkWriterExtensions.WriteSByte;
		Writer<char>.write = NetworkWriterExtensions.WriteChar;
		Writer<bool>.write = NetworkWriterExtensions.WriteBool;
		Writer<ushort>.write = NetworkWriterExtensions.WriteUShort;
		Writer<short>.write = NetworkWriterExtensions.WriteShort;
		Writer<uint>.write = NetworkWriterExtensions.WriteUInt;
		Writer<int>.write = NetworkWriterExtensions.WriteInt;
		Writer<ulong>.write = NetworkWriterExtensions.WriteULong;
		Writer<long>.write = NetworkWriterExtensions.WriteLong;
		Writer<float>.write = NetworkWriterExtensions.WriteFloat;
		Writer<double>.write = NetworkWriterExtensions.WriteDouble;
		Writer<decimal>.write = NetworkWriterExtensions.WriteDecimal;
		Writer<string>.write = NetworkWriterExtensions.WriteString;
		Writer<byte[]>.write = NetworkWriterExtensions.WriteBytesAndSize;
		Writer<ArraySegment<byte>>.write = NetworkWriterExtensions.WriteBytesAndSizeSegment;
		Writer<Vector2>.write = NetworkWriterExtensions.WriteVector2;
		Writer<Vector3>.write = NetworkWriterExtensions.WriteVector3;
		Writer<Vector4>.write = NetworkWriterExtensions.WriteVector4;
		Writer<Vector2Int>.write = NetworkWriterExtensions.WriteVector2Int;
		Writer<Vector3Int>.write = NetworkWriterExtensions.WriteVector3Int;
		Writer<Color>.write = NetworkWriterExtensions.WriteColor;
		Writer<Color32>.write = NetworkWriterExtensions.WriteColor32;
		Writer<Quaternion>.write = NetworkWriterExtensions.WriteQuaternion;
		Writer<Rect>.write = NetworkWriterExtensions.WriteRect;
		Writer<Plane>.write = NetworkWriterExtensions.WritePlane;
		Writer<Ray>.write = NetworkWriterExtensions.WriteRay;
		Writer<Matrix4x4>.write = NetworkWriterExtensions.WriteMatrix4x4;
		Writer<Guid>.write = NetworkWriterExtensions.WriteGuid;
		Writer<NetworkIdentity>.write = NetworkWriterExtensions.WriteNetworkIdentity;
		Writer<NetworkBehaviour>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Transform>.write = NetworkWriterExtensions.WriteTransform;
		Writer<GameObject>.write = NetworkWriterExtensions.WriteGameObject;
		Writer<Uri>.write = NetworkWriterExtensions.WriteUri;
		Writer<ReadyMessage>.write = _Write_Mirror_002EReadyMessage;
		Writer<NotReadyMessage>.write = _Write_Mirror_002ENotReadyMessage;
		Writer<AddPlayerMessage>.write = _Write_Mirror_002EAddPlayerMessage;
		Writer<SceneMessage>.write = _Write_Mirror_002ESceneMessage;
		Writer<SceneOperation>.write = _Write_Mirror_002ESceneOperation;
		Writer<CommandMessage>.write = _Write_Mirror_002ECommandMessage;
		Writer<RpcMessage>.write = _Write_Mirror_002ERpcMessage;
		Writer<SpawnMessage>.write = _Write_Mirror_002ESpawnMessage;
		Writer<ObjectSpawnStartedMessage>.write = _Write_Mirror_002EObjectSpawnStartedMessage;
		Writer<ObjectSpawnFinishedMessage>.write = _Write_Mirror_002EObjectSpawnFinishedMessage;
		Writer<ObjectDestroyMessage>.write = _Write_Mirror_002EObjectDestroyMessage;
		Writer<ObjectHideMessage>.write = _Write_Mirror_002EObjectHideMessage;
		Writer<EntityStateMessage>.write = _Write_Mirror_002EEntityStateMessage;
		Writer<NetworkPingMessage>.write = _Write_Mirror_002ENetworkPingMessage;
		Writer<NetworkPongMessage>.write = _Write_Mirror_002ENetworkPongMessage;
		Writer<AAPoly>.write = SidesSerializer.WriteSides;
		Writer<HouseData.Attackable>.write = AttackableSerializer.WriteAttackableData;
		Writer<StatusEffect.Data[]>.write = AttackableSerializer.WriteStatusEffectData;
		Writer<HouseData.Enemy>.write = AttackableSerializer.WriteEnemyData;
		Writer<HouseData.Consort>.write = AttackableSerializer.WriteConsortData;
		Writer<Vector3?>.write = ExtraWriters.WriteNullable;
		Writer<RectInt>.write = ExtraWriters.WriteRectInt;
		Writer<IStatusEffect>.write = StatusEffectWriter.WriteStatusEffect;
		Writer<HouseData.Item>.write = ItemSerializer.WriteItemData;
		Writer<Item>.write = ItemSerializer.WriteItem;
		Writer<NormalItem>.write = ItemSerializer.WriteNormalItem;
		Writer<PlayerData>.write = _Write_PlayerData;
		Writer<CharacterSettings>.write = _Write_CharacterSettings;
		Writer<PBColor>.write = _Write_PBColor;
		Writer<HairHighlights>.write = _Write_HairHighlights;
		Writer<int[]>.write = _Write_System_002EInt32_005B_005D;
		Writer<Class>.write = _Write_Class;
		Writer<Aspect>.write = _Write_Aspect;
		Writer<KernelSpriteData>.write = _Write_KernelSpriteData;
		Writer<string[]>.write = _Write_System_002EString_005B_005D;
		Writer<ExileData>.write = _Write_ExileData;
		Writer<Exile.Action>.write = _Write_Exile_002FAction;
		Writer<SylladexData>.write = _Write_SylladexData;
		Writer<ModusData>.write = _Write_ModusData;
		Writer<Specibus.Data>.write = _Write_Specibus_002FData;
		Writer<HouseData.Item[]>.write = _Write_HouseData_002FItem_005B_005D;
		Writer<QuestData[]>.write = _Write_QuestData_005B_005D;
		Writer<QuestData>.write = _Write_QuestData;
		Writer<QuestStatus>.write = _Write_Quest_002ENET_002EEnums_002EQuestStatus;
		Writer<List<byte[]>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EByte_005B_005D_003E;
		Writer<Item[]>.write = _Write_Item_005B_005D;
		Writer<Furniture.SpawnMessage>.write = _Write_Furniture_002FSpawnMessage;
		Writer<AAPoly.ShortChains>.write = _Write_AAPoly_002FShortChains;
		Writer<HouseData>.write = _Write_HouseData;
		Writer<HouseData.Story[]>.write = _Write_HouseData_002FStory_005B_005D;
		Writer<HouseData.Story>.write = _Write_HouseData_002FStory;
		Writer<AAPoly[]>.write = _Write_AAPoly_005B_005D;
		Writer<HouseData.Furniture[]>.write = _Write_HouseData_002FFurniture_005B_005D;
		Writer<HouseData.Furniture>.write = _Write_HouseData_002FFurniture;
		Writer<Orientation>.write = _Write_Orientation;
		Writer<RectInt[]>.write = _Write_UnityEngine_002ERectInt_005B_005D;
		Writer<HouseData.DroppedItem[]>.write = _Write_HouseData_002FDroppedItem_005B_005D;
		Writer<HouseData.DroppedItem>.write = _Write_HouseData_002FDroppedItem;
		Writer<HouseData.Attackable[]>.write = _Write_HouseData_002FAttackable_005B_005D;
		Writer<HouseData.NormalItem>.write = _Write_HouseData_002FNormalItem;
		Writer<HouseData.Totem>.write = _Write_HouseData_002FTotem;
		Writer<HouseData.PunchCard>.write = _Write_HouseData_002FPunchCard;
		Writer<HouseData.AlchemyItem>.write = _Write_HouseData_002FAlchemyItem;
		Writer<WeaponKind>.write = _Write_WeaponKind;
		Writer<ArmorKind>.write = _Write_ArmorKind;
		Writer<NormalItem.Tag[]>.write = _Write_NormalItem_002FTag_005B_005D;
		Writer<NormalItem.Tag>.write = _Write_NormalItem_002FTag;
		Writer<SyncedInteractableAction.Message>.write = _Write_SyncedInteractableAction_002FMessage;
		Writer<PlayerJoinMessage>.write = _Write_PlayerJoinMessage;
		Writer<PesterchumMessage>.write = _Write_PesterchumMessage;
		Writer<PesterchumStatusChange>.write = _Write_PesterchumStatusChange;
		Writer<RandomSync>.write = _Write_RandomSync;
		Writer<HostSaveRequest>.write = _Write_HostSaveRequest;
		Writer<ClientSaveResponse>.write = _Write_ClientSaveResponse;
		Writer<SessionPlayer>.write = _Write_SessionPlayer;
		Writer<HostLoadRequest>.write = _Write_HostLoadRequest;
		Writer<StatusEffect.Data>.write = _Write_StatusEffect_002FData;
		Writer<Item.SpawnMessage>.write = _Write_Item_002FSpawnMessage;
		Writer<Attackable>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Building>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<BuildingChanges>.write = _Write_BuildingChanges;
		Writer<List<BuildingChanges.Change>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FChange_003E;
		Writer<BuildingChanges.Change>.write = _Write_BuildingChanges_002FChange;
		Writer<List<BuildingChanges.RoomTransfer>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FRoomTransfer_003E;
		Writer<BuildingChanges.RoomTransfer>.write = _Write_BuildingChanges_002FRoomTransfer;
		Writer<WorldArea>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Reader<byte>.read = NetworkReaderExtensions.ReadByte;
		Reader<sbyte>.read = NetworkReaderExtensions.ReadSByte;
		Reader<char>.read = NetworkReaderExtensions.ReadChar;
		Reader<bool>.read = NetworkReaderExtensions.ReadBool;
		Reader<short>.read = NetworkReaderExtensions.ReadShort;
		Reader<ushort>.read = NetworkReaderExtensions.ReadUShort;
		Reader<int>.read = NetworkReaderExtensions.ReadInt;
		Reader<uint>.read = NetworkReaderExtensions.ReadUInt;
		Reader<long>.read = NetworkReaderExtensions.ReadLong;
		Reader<ulong>.read = NetworkReaderExtensions.ReadULong;
		Reader<float>.read = NetworkReaderExtensions.ReadFloat;
		Reader<double>.read = NetworkReaderExtensions.ReadDouble;
		Reader<decimal>.read = NetworkReaderExtensions.ReadDecimal;
		Reader<string>.read = NetworkReaderExtensions.ReadString;
		Reader<byte[]>.read = NetworkReaderExtensions.ReadBytesAndSize;
		Reader<ArraySegment<byte>>.read = NetworkReaderExtensions.ReadBytesAndSizeSegment;
		Reader<Vector2>.read = NetworkReaderExtensions.ReadVector2;
		Reader<Vector3>.read = NetworkReaderExtensions.ReadVector3;
		Reader<Vector4>.read = NetworkReaderExtensions.ReadVector4;
		Reader<Vector2Int>.read = NetworkReaderExtensions.ReadVector2Int;
		Reader<Vector3Int>.read = NetworkReaderExtensions.ReadVector3Int;
		Reader<Color>.read = NetworkReaderExtensions.ReadColor;
		Reader<Color32>.read = NetworkReaderExtensions.ReadColor32;
		Reader<Quaternion>.read = NetworkReaderExtensions.ReadQuaternion;
		Reader<Rect>.read = NetworkReaderExtensions.ReadRect;
		Reader<Plane>.read = NetworkReaderExtensions.ReadPlane;
		Reader<Ray>.read = NetworkReaderExtensions.ReadRay;
		Reader<Matrix4x4>.read = NetworkReaderExtensions.ReadMatrix4x4;
		Reader<Guid>.read = NetworkReaderExtensions.ReadGuid;
		Reader<Transform>.read = NetworkReaderExtensions.ReadTransform;
		Reader<GameObject>.read = NetworkReaderExtensions.ReadGameObject;
		Reader<NetworkIdentity>.read = NetworkReaderExtensions.ReadNetworkIdentity;
		Reader<NetworkBehaviour>.read = NetworkReaderExtensions.ReadNetworkBehaviour;
		Reader<NetworkBehaviour.NetworkBehaviourSyncVar>.read = NetworkReaderExtensions.ReadNetworkBehaviourSyncVar;
		Reader<Uri>.read = NetworkReaderExtensions.ReadUri;
		Reader<ReadyMessage>.read = _Read_Mirror_002EReadyMessage;
		Reader<NotReadyMessage>.read = _Read_Mirror_002ENotReadyMessage;
		Reader<AddPlayerMessage>.read = _Read_Mirror_002EAddPlayerMessage;
		Reader<SceneMessage>.read = _Read_Mirror_002ESceneMessage;
		Reader<SceneOperation>.read = _Read_Mirror_002ESceneOperation;
		Reader<CommandMessage>.read = _Read_Mirror_002ECommandMessage;
		Reader<RpcMessage>.read = _Read_Mirror_002ERpcMessage;
		Reader<SpawnMessage>.read = _Read_Mirror_002ESpawnMessage;
		Reader<ObjectSpawnStartedMessage>.read = _Read_Mirror_002EObjectSpawnStartedMessage;
		Reader<ObjectSpawnFinishedMessage>.read = _Read_Mirror_002EObjectSpawnFinishedMessage;
		Reader<ObjectDestroyMessage>.read = _Read_Mirror_002EObjectDestroyMessage;
		Reader<ObjectHideMessage>.read = _Read_Mirror_002EObjectHideMessage;
		Reader<EntityStateMessage>.read = _Read_Mirror_002EEntityStateMessage;
		Reader<NetworkPingMessage>.read = _Read_Mirror_002ENetworkPingMessage;
		Reader<NetworkPongMessage>.read = _Read_Mirror_002ENetworkPongMessage;
		Reader<AAPoly>.read = SidesSerializer.ReadSides;
		Reader<HouseData.Attackable>.read = AttackableSerializer.ReadAttackableData;
		Reader<StatusEffect.Data[]>.read = AttackableSerializer.ReadStatusEffectData;
		Reader<HouseData.Enemy>.read = AttackableSerializer.ReadEnemyData;
		Reader<HouseData.Consort>.read = AttackableSerializer.ReadConsortData;
		Reader<Vector3?>.read = ExtraWriters.ReadNullable;
		Reader<RectInt>.read = ExtraWriters.ReadRectInt;
		Reader<IStatusEffect>.read = StatusEffectWriter.ReadStatusEffect;
		Reader<HouseData.Item>.read = ItemSerializer.ReadItemData;
		Reader<Item>.read = ItemSerializer.ReadItem;
		Reader<NormalItem>.read = ItemSerializer.ReadNormalItem;
		Reader<PlayerData>.read = _Read_PlayerData;
		Reader<CharacterSettings>.read = _Read_CharacterSettings;
		Reader<PBColor>.read = _Read_PBColor;
		Reader<HairHighlights>.read = _Read_HairHighlights;
		Reader<int[]>.read = _Read_System_002EInt32_005B_005D;
		Reader<Class>.read = _Read_Class;
		Reader<Aspect>.read = _Read_Aspect;
		Reader<KernelSpriteData>.read = _Read_KernelSpriteData;
		Reader<string[]>.read = _Read_System_002EString_005B_005D;
		Reader<ExileData>.read = _Read_ExileData;
		Reader<Exile.Action>.read = _Read_Exile_002FAction;
		Reader<SylladexData>.read = _Read_SylladexData;
		Reader<ModusData>.read = _Read_ModusData;
		Reader<Specibus.Data>.read = _Read_Specibus_002FData;
		Reader<HouseData.Item[]>.read = _Read_HouseData_002FItem_005B_005D;
		Reader<QuestData[]>.read = _Read_QuestData_005B_005D;
		Reader<QuestData>.read = _Read_QuestData;
		Reader<QuestStatus>.read = _Read_Quest_002ENET_002EEnums_002EQuestStatus;
		Reader<List<byte[]>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EByte_005B_005D_003E;
		Reader<Item[]>.read = _Read_Item_005B_005D;
		Reader<Furniture.SpawnMessage>.read = _Read_Furniture_002FSpawnMessage;
		Reader<AAPoly.ShortChains>.read = _Read_AAPoly_002FShortChains;
		Reader<HouseData>.read = _Read_HouseData;
		Reader<HouseData.Story[]>.read = _Read_HouseData_002FStory_005B_005D;
		Reader<HouseData.Story>.read = _Read_HouseData_002FStory;
		Reader<AAPoly[]>.read = _Read_AAPoly_005B_005D;
		Reader<HouseData.Furniture[]>.read = _Read_HouseData_002FFurniture_005B_005D;
		Reader<HouseData.Furniture>.read = _Read_HouseData_002FFurniture;
		Reader<Orientation>.read = _Read_Orientation;
		Reader<RectInt[]>.read = _Read_UnityEngine_002ERectInt_005B_005D;
		Reader<HouseData.DroppedItem[]>.read = _Read_HouseData_002FDroppedItem_005B_005D;
		Reader<HouseData.DroppedItem>.read = _Read_HouseData_002FDroppedItem;
		Reader<HouseData.Attackable[]>.read = _Read_HouseData_002FAttackable_005B_005D;
		Reader<HouseData.NormalItem>.read = _Read_HouseData_002FNormalItem;
		Reader<HouseData.Totem>.read = _Read_HouseData_002FTotem;
		Reader<HouseData.PunchCard>.read = _Read_HouseData_002FPunchCard;
		Reader<HouseData.AlchemyItem>.read = _Read_HouseData_002FAlchemyItem;
		Reader<WeaponKind>.read = _Read_WeaponKind;
		Reader<ArmorKind>.read = _Read_ArmorKind;
		Reader<NormalItem.Tag[]>.read = _Read_NormalItem_002FTag_005B_005D;
		Reader<NormalItem.Tag>.read = _Read_NormalItem_002FTag;
		Reader<SyncedInteractableAction.Message>.read = _Read_SyncedInteractableAction_002FMessage;
		Reader<PlayerJoinMessage>.read = _Read_PlayerJoinMessage;
		Reader<PesterchumMessage>.read = _Read_PesterchumMessage;
		Reader<PesterchumStatusChange>.read = _Read_PesterchumStatusChange;
		Reader<RandomSync>.read = _Read_RandomSync;
		Reader<HostSaveRequest>.read = _Read_HostSaveRequest;
		Reader<ClientSaveResponse>.read = _Read_ClientSaveResponse;
		Reader<SessionPlayer>.read = _Read_SessionPlayer;
		Reader<HostLoadRequest>.read = _Read_HostLoadRequest;
		Reader<StatusEffect.Data>.read = _Read_StatusEffect_002FData;
		Reader<Item.SpawnMessage>.read = _Read_Item_002FSpawnMessage;
		Reader<Attackable>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Attackable>;
		Reader<BuildingChanges>.read = _Read_BuildingChanges;
		Reader<List<BuildingChanges.Change>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FChange_003E;
		Reader<BuildingChanges.Change>.read = _Read_BuildingChanges_002FChange;
		Reader<List<BuildingChanges.RoomTransfer>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CBuildingChanges_002FRoomTransfer_003E;
		Reader<BuildingChanges.RoomTransfer>.read = _Read_BuildingChanges_002FRoomTransfer;
	}
}
