using System.Collections.Generic;
using System.Linq;
using QFSW.QC;
using TheGenesisLib.Models;

namespace Util;

public class ItemCodeSuggestor : BasicCachedQcSuggestor<string>
{
	protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
	{
		return context.HasTag<ItemCodeTag>();
	}

	protected override IQcSuggestion ItemToSuggestion(string item)
	{
		return new RawSuggestion(item);
	}

	protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.AllItems.Select((LDBItem i) => i.Code);
	}
}
