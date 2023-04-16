using System.Collections.Generic;
using System.IO;
using System.Linq;
using QFSW.QC;

namespace Util;

public class CreatureNameSuggestor : BasicCachedQcSuggestor<string>
{
	protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
	{
		return context.HasTag<CreatureNameTag>();
	}

	protected override IQcSuggestion ItemToSuggestion(string name)
	{
		return new RawSuggestion(name);
	}

	protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
	{
		return from filepath in SpawnHelper.instance.GetCreatureNames()
			select Path.GetFileNameWithoutExtension(filepath);
	}
}
