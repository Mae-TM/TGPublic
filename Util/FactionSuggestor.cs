using System.Collections.Generic;
using System.IO;
using System.Linq;
using QFSW.QC;
using UnityEngine;

namespace Util;

public class FactionSuggestor : BasicCachedQcSuggestor<string>
{
	protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
	{
		return context.HasTag<FactionNameTag>();
	}

	protected override IQcSuggestion ItemToSuggestion(string item)
	{
		return new RawSuggestion(item);
	}

	protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
	{
		return from f in Directory.EnumerateFiles(Path.Combine(Application.streamingAssetsPath, "Factions"), "*.json")
			select Path.GetFileNameWithoutExtension(f);
	}
}
