using UnityEngine.UI;

public interface IInteractableAction
{
	bool enabled { get; }

	string Desc { get; }

	void Execute();

	void ApplyToImage(Image image);
}
