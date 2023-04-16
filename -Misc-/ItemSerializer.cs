using Mirror;

public static class ItemSerializer
{
	public static void WriteItemData(this NetworkWriter writer, HouseData.Item item)
	{
		if (item == null)
		{
			writer.WriteByte(0);
		}
		else if (!(item is HouseData.NormalItem value))
		{
			if (!(item is HouseData.AlchemyItem value2))
			{
				if (!(item is HouseData.PunchCard value3))
				{
					if (item is HouseData.Totem value4)
					{
						writer.WriteByte(4);
						writer.Write(value4);
					}
				}
				else
				{
					writer.WriteByte(3);
					writer.Write(value3);
				}
			}
			else
			{
				writer.WriteByte(2);
				writer.Write(value2);
			}
		}
		else
		{
			writer.WriteByte(1);
			writer.Write(value);
		}
	}

	public static HouseData.Item ReadItemData(this NetworkReader reader)
	{
		return reader.ReadByte() switch
		{
			1 => reader.Read<HouseData.NormalItem>(), 
			2 => reader.Read<HouseData.AlchemyItem>(), 
			3 => reader.Read<HouseData.PunchCard>(), 
			4 => reader.Read<HouseData.Totem>(), 
			_ => null, 
		};
	}

	public static void WriteItem(this NetworkWriter writer, Item item)
	{
		writer.Write(item?.NetIdentity);
		writer.WriteItemData(item);
	}

	public static Item ReadItem(this NetworkReader reader)
	{
		NetworkIdentity networkIdentity = reader.Read<NetworkIdentity>();
		HouseData.Item item = reader.ReadItemData();
		if (!networkIdentity)
		{
			return item;
		}
		return networkIdentity.GetComponent<ItemObject>().Item;
	}

	public static void WriteNormalItem(this NetworkWriter writer, NormalItem item)
	{
		writer.WriteItem(item);
	}

	public static NormalItem ReadNormalItem(this NetworkReader reader)
	{
		return (NormalItem)reader.ReadItem();
	}
}
