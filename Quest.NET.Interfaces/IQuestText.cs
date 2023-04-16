using ProtoBuf;

namespace Quest.NET.Interfaces;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public interface IQuestText
{
	string Name { get; }

	string DescriptionSummary { get; }

	string Hint { get; }

	string Dialog { get; }
}
