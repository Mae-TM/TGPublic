using System;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
	public enum Alignment
	{
		Horizontal,
		Vertical
	}

	public enum FitType
	{
		Uniform,
		Width,
		Height,
		FixedRows,
		FixedColumns,
		FixedBoth
	}

	public Alignment alignment;

	public FitType fitType;

	public int rows;

	public int columns;

	public Vector2 cellSize;

	public Vector2 spacing;

	public bool fitX;

	public bool fitY;

	public override void CalculateLayoutInputVertical()
	{
		base.CalculateLayoutInputHorizontal();
		if (base.rectChildren.Count == 0)
		{
			return;
		}
		Rect rect = base.rectTransform.rect;
		float width = rect.width;
		float height = rect.height;
		int val;
		int val2;
		if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
		{
			fitX = true;
			fitY = true;
			float num = width / height;
			val = Mathf.CeilToInt(Mathf.Sqrt((float)base.rectChildren.Count / num));
			val = Math.Min(rows, val);
			val2 = Mathf.CeilToInt(Mathf.Sqrt((float)base.rectChildren.Count * num));
			val2 = Math.Min(columns, val2);
		}
		else
		{
			val = rows;
			val2 = columns;
		}
		if (fitType == FitType.Height || fitType == FitType.FixedRows || fitType == FitType.Uniform)
		{
			val2 = Mathf.CeilToInt((float)base.rectChildren.Count / (float)val);
		}
		if (fitType == FitType.Width || fitType == FitType.FixedColumns || fitType == FitType.Uniform)
		{
			val = Mathf.CeilToInt((float)base.rectChildren.Count / (float)val2);
		}
		float num2;
		float num3;
		if (alignment == Alignment.Horizontal)
		{
			num2 = width / (float)val2 - spacing.x / (float)val2 * (float)(val2 - 1) - (float)base.padding.left / (float)val2 - (float)base.padding.right / (float)val2;
			num3 = height / (float)val - spacing.y / (float)val * (float)(val - 1) - (float)base.padding.top / (float)val - (float)base.padding.bottom / (float)val;
		}
		else
		{
			num3 = width / (float)val2 - spacing.x / (float)val2 * (float)(val2 - 1) - (float)base.padding.left / (float)val2 - (float)base.padding.right / (float)val2;
			num2 = height / (float)val - spacing.y / (float)val * (float)(val - 1) - (float)base.padding.top / (float)val - (float)base.padding.bottom / (float)val;
		}
		cellSize.x = (fitX ? num2 : cellSize.x);
		cellSize.y = (fitY ? num3 : cellSize.y);
		for (int i = 0; i < base.rectChildren.Count; i++)
		{
			RectTransform rect2 = base.rectChildren[i];
			if (alignment == Alignment.Horizontal)
			{
				int num4 = i / val2;
				int num5 = i % val2;
				float pos = cellSize.x * (float)num5 + spacing.x * (float)num5 + (float)base.padding.left;
				float pos2 = cellSize.y * (float)num4 + spacing.y * (float)num4 + (float)base.padding.top;
				SetChildAlongAxis(rect2, 0, pos, cellSize.x);
				SetChildAlongAxis(rect2, 1, pos2, cellSize.y);
			}
			else
			{
				int num4 = i / val;
				int num5 = i % val;
				float pos3 = cellSize.x * (float)num5 + spacing.x * (float)num5 + (float)base.padding.left;
				float pos4 = cellSize.y * (float)num4 + spacing.y * (float)num4 + (float)base.padding.top;
				SetChildAlongAxis(rect2, 0, pos4, cellSize.y);
				SetChildAlongAxis(rect2, 1, pos3, cellSize.x);
			}
		}
	}

	public override void SetLayoutHorizontal()
	{
	}

	public override void SetLayoutVertical()
	{
	}
}
