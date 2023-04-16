using Quest.NET.Interfaces;

namespace Quest.NET;

public class QuestText : IQuestText
{
	private string _name;

	private string _descriptionSummary;

	private string _hint;

	private string _dialog;

	public string Name => _name;

	public string DescriptionSummary => _descriptionSummary;

	public string Hint => _hint;

	public string Dialog => _dialog;

	public QuestText(string name, string descriptionSummary, string hint, string dialog)
	{
		_name = name;
		_descriptionSummary = descriptionSummary;
		_hint = hint;
		_dialog = dialog;
	}
}
