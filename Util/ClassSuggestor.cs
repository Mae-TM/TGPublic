using System;
using System.Collections.Generic;
using QFSW.QC;

namespace Util;

public class ClassSuggestor : BasicCachedQcSuggestor<string>
{
	protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
	{
		return context.HasTag<ClassNameTag>();
	}

	protected override IQcSuggestion ItemToSuggestion(string name)
	{
		return new RawSuggestion(name);
	}

	protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
	{
		return Enum.GetNames(typeof(Class));
	}
}
