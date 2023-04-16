using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

public class TreeModus : Modus
{
	[ProtoContract]
	public struct TreeData
	{
		[ProtoMember(1)]
		public bool balance;

		[ProtoMember(2)]
		public HouseData.Item[] data;
	}

	private class TreeNode : Card
	{
		public TreeNode l;

		public TreeNode r;

		public string k;

		public bool red;

		public TreeNode p { get; private set; }

		public TreeNode(Modus modus, Item ni, string nk, Transform parent, Transform bigParent)
			: base(modus, ni, parent, default(Vector2), bigParent, default(Vector2), 1)
		{
			k = nk;
		}

		public TreeNode(Modus modus, Item ni, string nk, TreeNode np, bool nred = false)
			: base(modus, ni, new Vector2(-16f, -100f), np, new Vector2(-32f, 0f))
		{
			k = nk;
			p = np;
			red = nred;
		}

		public void SetParent(TreeNode parent)
		{
			p = parent;
		}
	}

	private TreeNode root;

	private int itemCount;

	private bool balance = true;

	private bool isBalanced = true;

	private new void Awake()
	{
		base.Awake();
		itemCapacity = 6;
		AddToggle("auto-balance", status: true, SetBalance);
		SetColor(new Color(150f, 255f, 0f));
		SetIcon("Tree");
	}

	private void SetBalance(bool to)
	{
		balance = to;
		if (!(!isBalanced && to))
		{
			return;
		}
		if (root != null)
		{
			Queue<TreeNode> queue = new Queue<TreeNode>();
			queue.Enqueue(root.l);
			root.l = null;
			queue.Enqueue(root.r);
			root.r = null;
			root.red = false;
			while (queue.Count != 0)
			{
				TreeNode treeNode = queue.Dequeue();
				if (treeNode == null)
				{
					continue;
				}
				queue.Enqueue(treeNode.l);
				treeNode.l = null;
				queue.Enqueue(treeNode.r);
				treeNode.r = null;
				treeNode.red = true;
				TreeNode treeNode2 = root;
				while (treeNode2 != null)
				{
					if (string.Compare(treeNode2.k, treeNode.k) > 0)
					{
						if (treeNode2.r == null)
						{
							treeNode2.r = treeNode;
							break;
						}
						treeNode2 = treeNode2.r;
					}
					else
					{
						if (treeNode2.l == null)
						{
							treeNode2.l = treeNode;
							break;
						}
						treeNode2 = treeNode2.l;
					}
				}
				BalanceInserted(treeNode);
			}
		}
		isBalanced = true;
	}

	private TreeNode FindItem(Item it)
	{
		TreeNode treeNode = root;
		string itemName = it.GetItemName();
		Stack<TreeNode> stack = new Stack<TreeNode>();
		while (true)
		{
			if (treeNode == null && stack.Count != 0)
			{
				treeNode = stack.Pop();
			}
			if (treeNode == null || treeNode.item == it)
			{
				break;
			}
			int num = string.Compare(itemName, treeNode.k);
			if (num > 0)
			{
				treeNode = treeNode.r;
				continue;
			}
			if (num == 0 && treeNode.r != null)
			{
				stack.Push(treeNode.r);
			}
			treeNode = treeNode.l;
		}
		return treeNode;
	}

	private void Rotate(TreeNode n, bool right)
	{
		TreeNode treeNode = (right ? n.l : n.r);
		TreeNode treeNode2 = (right ? treeNode.r : treeNode.l);
		if (right)
		{
			n.l = treeNode2;
		}
		else
		{
			n.r = treeNode2;
		}
		treeNode2?.SetParent(n);
		treeNode.SetParent(n.p);
		if (n.p == null)
		{
			root = treeNode;
		}
		else if (n == n.p.l)
		{
			n.p.l = treeNode;
		}
		else
		{
			n.p.r = treeNode;
		}
		if (right)
		{
			treeNode.r = n;
		}
		else
		{
			treeNode.l = n;
		}
		n.SetParent(treeNode);
	}

	protected override bool AddItemToModus(Item toAdd)
	{
		if (itemCount == itemCapacity)
		{
			return false;
		}
		itemCount++;
		TreeNode treeNode = root;
		if (treeNode == null)
		{
			root = new TreeNode(this, toAdd, toAdd.GetItemName(), base.transform.GetChild(0), base.transform.GetChild(1));
			return true;
		}
		string itemName = toAdd.GetItemName();
		while (treeNode != null)
		{
			if (string.Compare(itemName, treeNode.k) > 0)
			{
				if (treeNode.r == null)
				{
					treeNode.r = new TreeNode(this, toAdd, itemName, treeNode, nred: true);
					treeNode = treeNode.r;
					break;
				}
				treeNode = treeNode.r;
			}
			else
			{
				if (treeNode.l == null)
				{
					treeNode.l = new TreeNode(this, toAdd, itemName, treeNode, nred: true);
					treeNode = treeNode.l;
					break;
				}
				treeNode = treeNode.l;
			}
		}
		if (balance)
		{
			BalanceInserted(treeNode);
		}
		else
		{
			isBalanced = false;
		}
		return true;
	}

	private void BalanceInserted(TreeNode n)
	{
		TreeNode treeNode = null;
		while (n.p != null && n.p.red)
		{
			bool flag = n.p == n.p.p.r;
			treeNode = (flag ? n.p.p.l : n.p.p.r);
			if (treeNode != null && treeNode.red)
			{
				n.p.red = false;
				treeNode.red = false;
				n.p.p.red = true;
				n = n.p.p;
				continue;
			}
			if (n == (flag ? n.p.l : n.p.r))
			{
				n = n.p;
				Rotate(n, flag);
			}
			n.p.red = false;
			n.p.p.red = true;
			Rotate(n.p.p, !flag);
		}
		root.red = false;
	}

	protected override bool IsRetrievable(Card item)
	{
		TreeNode treeNode = item as TreeNode;
		if (treeNode.l == null)
		{
			return treeNode.r == null;
		}
		return false;
	}

	protected override IEnumerable<Card> GetItemList()
	{
		List<Card> items = new List<Card>(itemCount);
		if (root != null)
		{
			GetItems(root, ref items);
		}
		return items;
	}

	private void GetItems(TreeNode n, ref List<Card> items)
	{
		if (n.l != null)
		{
			GetItems(n.l, ref items);
		}
		items.Add(n);
		if (n.r != null)
		{
			GetItems(n.r, ref items);
		}
	}

	public override ModusData Save()
	{
		HouseData.Item[] array = new HouseData.Item[GetItemList().Count()];
		int num = 0;
		foreach (Card item in GetItemList())
		{
			if (item != null && item.item != null)
			{
				array[num] = item.item.Save();
			}
			else
			{
				array[num] = null;
			}
			num++;
		}
		TreeData obj = default(TreeData);
		obj.balance = balance;
		obj.data = array;
		byte[] modusSpecificData = ProtobufHelpers.ProtoSerialize(obj);
		ModusData result = default(ModusData);
		result.capacity = itemCapacity;
		result.modusSpecificData = modusSpecificData;
		return result;
	}

	public override void Load(ModusData data)
	{
		TreeData treeData = ProtobufHelpers.ProtoDeserialize<TreeData>(data.modusSpecificData);
		balance = treeData.balance;
		itemCapacity = data.capacity;
		if (treeData.data != null)
		{
			for (int i = 0; i < treeData.data.Length; i++)
			{
				AddItemToModus(treeData.data[i]);
			}
		}
	}

	protected override void Load(Item[] items)
	{
		itemCount = 0;
		foreach (Item item in items)
		{
			if (item != null)
			{
				AddItemToModus(item);
			}
		}
	}

	public override int GetAmount()
	{
		return itemCount;
	}

	protected override bool RemoveItemFromModus(Card item)
	{
		if (!(item is TreeNode treeNode))
		{
			return false;
		}
		if (treeNode.l == null && treeNode.r == null)
		{
			if (treeNode == root)
			{
				root = null;
			}
			else
			{
				if (balance && !treeNode.red)
				{
					BalanceDeleted(treeNode);
				}
				if (treeNode == treeNode.p.l)
				{
					treeNode.p.l = null;
				}
				if (treeNode == treeNode.p.r)
				{
					treeNode.p.r = null;
				}
			}
			itemCount--;
			return true;
		}
		return false;
	}

	private void BalanceDeleted(TreeNode n)
	{
		while (n != root && !n.red)
		{
			bool flag = n == n.p.r;
			TreeNode treeNode = (flag ? n.p.l : n.p.r);
			if (treeNode.red)
			{
				treeNode.red = false;
				n.p.red = true;
				Rotate(n.p, flag);
				treeNode = (flag ? n.p.l : n.p.r);
			}
			if ((treeNode.l == null || !treeNode.l.red) && (treeNode.r == null || !treeNode.r.red))
			{
				treeNode.red = true;
				n = n.p;
				continue;
			}
			if (flag)
			{
				if (treeNode.l == null || !treeNode.l.red)
				{
					treeNode.r.red = false;
					treeNode.red = true;
					Rotate(treeNode, right: false);
					treeNode = n.p.l;
				}
				treeNode.l.red = false;
			}
			else
			{
				if (treeNode.r == null || !treeNode.r.red)
				{
					treeNode.l.red = false;
					treeNode.red = true;
					Rotate(treeNode, right: true);
					treeNode = n.p.r;
				}
				treeNode.r.red = false;
			}
			treeNode.red = n.p.red;
			n.p.red = false;
			Rotate(n.p, flag);
			n = root;
		}
		n.red = false;
	}

	protected override void UpdatePositions()
	{
		if (root != null)
		{
			base.UpdatePositions();
			UpdatePositions(root, new Vector2(0f, 0f), out var _);
		}
	}

	private float UpdatePositions(TreeNode n, Vector2 pos, out float size)
	{
		n.bigTarget.y = pos.y;
		n.BringToFront();
		if (n.l != null && n.r != null)
		{
			n.bigTarget.x = (UpdatePositions(n.l, pos - new Vector2(0f, complexcardsize.y * 2f / 3f), out size) + UpdatePositions(n.r, pos + new Vector2((n.l != null && n.l.r == null && n.r.l == null) ? (size * 2f / 3f) : size, (0f - complexcardsize.y) * 2f / 3f), out var size2)) / 2f;
			size = ((n.l != null && n.r != null && n.l.r == null && n.r.l == null) ? (size * 2f / 3f) : size) + size2;
		}
		else if (n.l != null)
		{
			n.bigTarget.x = UpdatePositions(n.l, pos - new Vector2(0f, complexcardsize.y * 2f / 3f), out size) + 0.2f * complexcardsize.x;
			size += 0.2f * complexcardsize.x;
		}
		else if (n.r != null)
		{
			n.bigTarget.x = UpdatePositions(n.r, pos + new Vector2(0.2f * complexcardsize.x, (0f - complexcardsize.y) * 2f / 3f), out size) - 0.2f * complexcardsize.x;
			size += 0.2f * complexcardsize.x;
		}
		else
		{
			n.bigTarget.x = pos.x;
			size = complexcardsize.x;
		}
		return n.bigTarget.x;
	}

	public override Rect GetItemRect()
	{
		return new Rect(new Vector2(base.transform.GetChild(1).position.x + ((root == null) ? 0f : root.bigTarget.x), base.transform.GetChild(1).position.y), complexcardsize);
	}
}
