using UnityEngine;

public interface ItemAcceptor
{
	Rect GetItemRect();

	bool AcceptItem(Item item);

	bool IsActive(Item item);

	void Hover(Item item);
}
