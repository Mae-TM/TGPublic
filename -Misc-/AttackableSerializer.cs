using Mirror;
using UnityEngine;

public static class AttackableSerializer
{
	public static void WriteAttackableData(this NetworkWriter writer, HouseData.Attackable attackable)
	{
		if (attackable == null)
		{
			Debug.Log("Writing null Attackable!!!");
			writer.WriteByte(0);
		}
		else if (!(attackable is HouseData.Enemy value))
		{
			if (attackable is HouseData.Consort value2)
			{
				writer.WriteByte(3);
				writer.Write(value2);
				return;
			}
			writer.WriteByte(1);
			writer.Write(attackable.name);
			writer.Write(attackable.pos);
			writer.Write(attackable.health);
			writer.Write(attackable.statusEffects);
		}
		else
		{
			writer.WriteByte(2);
			writer.Write(value);
		}
	}

	public static HouseData.Attackable ReadAttackableData(this NetworkReader reader)
	{
		HouseData.Attackable attackable = reader.ReadByte() switch
		{
			1 => new HouseData.Attackable
			{
				name = reader.Read<string>(),
				pos = reader.Read<Vector3>(),
				health = reader.Read<float>(),
				statusEffects = reader.Read<StatusEffect.Data[]>()
			}, 
			2 => reader.Read<HouseData.Enemy>(), 
			3 => reader.Read<HouseData.Consort>(), 
			_ => null, 
		};
		Debug.Log($"Read Attackable: {attackable}");
		return attackable;
	}

	public static void WriteStatusEffectData(this NetworkWriter writer, StatusEffect.Data[] statusEffect)
	{
		if (statusEffect == null)
		{
			writer.Write(0);
			return;
		}
		writer.Write(statusEffect.Length);
		for (int i = 0; i < statusEffect.Length; i++)
		{
			StatusEffect.Data data = statusEffect[i];
			writer.Write(data.type);
			writer.Write(data.protoData);
			writer.Write(data.endTime);
		}
	}

	public static StatusEffect.Data[] ReadStatusEffectData(this NetworkReader reader)
	{
		int num = reader.Read<int>();
		StatusEffect.Data[] array = new StatusEffect.Data[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new StatusEffect.Data
			{
				type = reader.Read<string>(),
				protoData = reader.Read<byte[]>(),
				endTime = reader.Read<float>()
			};
		}
		return array;
	}

	public static void WriteEnemyData(this NetworkWriter writer, HouseData.Enemy enemy)
	{
		writer.Write(enemy.name);
		writer.Write(enemy.pos);
		writer.Write(enemy.health);
		writer.Write(enemy.statusEffects);
		writer.Write(enemy.type);
	}

	public static HouseData.Enemy ReadEnemyData(this NetworkReader reader)
	{
		return new HouseData.Enemy
		{
			name = reader.Read<string>(),
			pos = reader.Read<Vector3>(),
			health = reader.Read<float>(),
			statusEffects = reader.Read<StatusEffect.Data[]>(),
			type = reader.Read<int>()
		};
	}

	public static void WriteConsortData(this NetworkWriter writer, HouseData.Consort consort)
	{
		writer.Write(consort.name);
		writer.Write(consort.pos);
		writer.Write(consort.health);
		writer.Write(consort.statusEffects);
		writer.Write(consort.job);
		writer.Write(consort.quests);
	}

	public static HouseData.Consort ReadConsortData(this NetworkReader reader)
	{
		return new HouseData.Consort
		{
			name = reader.Read<string>(),
			pos = reader.Read<Vector3>(),
			health = reader.Read<float>(),
			statusEffects = reader.Read<StatusEffect.Data[]>(),
			job = reader.Read<Consort.Job>(),
			quests = reader.Read<string[]>()
		};
	}
}
