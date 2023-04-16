using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QFSW.QC;
using TheGenesisLib.Models;

namespace Util;

public class LDBItemSerializer : BasicQcSerializer<LDBItem>
{
	public override string SerializeFormatted(LDBItem value, QuantumTheme theme)
	{
		return JsonConvert.SerializeObject(value, Formatting.Indented, new StringEnumConverter());
	}
}
