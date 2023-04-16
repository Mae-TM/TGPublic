using QFSW.QC;
using TheGenesisLib.Models;

namespace Util;

public class LDBRecipeSerializer : BasicQcSerializer<LDBRecipe>
{
	public override string SerializeFormatted(LDBRecipe value, QuantumTheme theme)
	{
		string text = ((value.Method == LDBRecipe.Methods.AND) ? "&&" : "||");
		return value.ItemA + " " + text + " " + value.ItemB + " -> " + value.Result.Code;
	}
}
