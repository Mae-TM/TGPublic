using System.IO;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct CharacterSettings
{
	[ProtoMember(1)]
	public string eyes;

	[ProtoMember(2)]
	public string mouth;

	[ProtoMember(3)]
	public string shirt;

	[ProtoMember(5)]
	public string hairtop;

	[ProtoMember(6)]
	public string hairbottom;

	[ProtoMember(7)]
	public int symbol;

	[ProtoMember(8)]
	public float whiteHair;

	[ProtoMember(9)]
	public PBColor color;

	[ProtoMember(10)]
	public bool isRobot;

	[ProtoMember(11)]
	public byte[] customSymbol;

	[ProtoMember(12)]
	public HairHighlights hairHighlights;

	private Sprite symbolSprite;

	public CharacterSettings(string eyes = "aanone", string mouth = "aanone", string shirt = "aanone", string hairtop = "aanone", string hairbottom = "aanone", HairHighlights hairHighlights = HairHighlights.Pale, int symbol = 0, float whiteHair = 1f, Color color = default(Color), bool isRobot = false, bool loadSymbol = false)
	{
		this.eyes = eyes;
		this.mouth = mouth;
		this.shirt = shirt;
		this.hairtop = hairtop;
		this.hairbottom = hairbottom;
		this.hairHighlights = hairHighlights;
		this.symbol = symbol;
		this.whiteHair = whiteHair;
		this.color = color;
		this.isRobot = isRobot;
		if (symbol == -1 && loadSymbol)
		{
			try
			{
				customSymbol = File.ReadAllBytes(Application.streamingAssetsPath + "/symbol.png");
			}
			catch
			{
				customSymbol = null;
			}
		}
		else
		{
			customSymbol = null;
		}
		symbolSprite = null;
	}

	public Sprite GetSymbol()
	{
		if (symbolSprite == null)
		{
			if (symbol != -1)
			{
				symbolSprite = Resources.LoadAll<Sprite>("Player/symbol/symb")[symbol];
			}
			else
			{
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(customSymbol);
				symbolSprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			}
		}
		return symbolSprite;
	}
}
