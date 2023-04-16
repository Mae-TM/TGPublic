using QFSW.QC;

namespace Util;

public sealed class FactionNameAttribute : SuggestorTagAttribute
{
	private readonly IQcSuggestorTag[] _tags = new IQcSuggestorTag[1] { default(FactionNameTag) };

	public override IQcSuggestorTag[] GetSuggestorTags()
	{
		return _tags;
	}
}
