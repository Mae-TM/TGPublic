using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QFSW.QC;

namespace Util;

public class MethodProfileSerializer : BasicQcSerializer<MethodProfile>
{
	public override string SerializeFormatted(MethodProfile profile, QuantumTheme theme)
	{
		return JsonConvert.SerializeObject(profile, Formatting.Indented, new StringEnumConverter());
	}
}
