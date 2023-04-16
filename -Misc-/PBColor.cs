using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct PBColor
{
	[ProtoMember(1, Name = "Hue")]
	public float h;

	[ProtoMember(2, Name = "Saturation")]
	public float s;

	[ProtoMember(3, Name = "Value")]
	public float v;

	public PBColor(float h, float s, float v)
	{
		this.h = h;
		this.s = s;
		this.v = v;
	}

	public static implicit operator Color(PBColor i)
	{
		return Color.HSVToRGB(i.h, i.s, i.v);
	}

	public static implicit operator PBColor(Color i)
	{
		PBColor result = default(PBColor);
		Color.RGBToHSV(i, out result.h, out result.s, out result.v);
		return result;
	}
}
