using System.Collections;
using System.Reflection;
using UnityEngine;

public static class StructComparison
{
	public static bool EqualsDeep<T>(this T left, T right)
	{
		if (right == null)
		{
			return false;
		}
		if ((object)left == (object)right)
		{
			return true;
		}
		if (left.GetType() != right.GetType())
		{
			return false;
		}
		FieldInfo[] fields = right.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(right);
			object value2 = fieldInfo.GetValue(left);
			Debug.Log($"Comparing {right.GetType().Name}.{fieldInfo.Name}: {value2} == {value}");
			if (value == null && value2 == null)
			{
				continue;
			}
			if (value2 is ICollection || value is ICollection)
			{
				if (!(value is ICollection collection) || !(value2 is ICollection collection2) || collection2.Count <= 0)
				{
					continue;
				}
				bool flag = false;
				foreach (object item in collection2)
				{
					foreach (object item2 in collection)
					{
						if (item.EqualsDeep(item2))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			else if (value2.GetType().IsValueType || value2 is string)
			{
				if (!value2.Equals(value))
				{
					return false;
				}
			}
			else if (!value2.EqualsDeep(value))
			{
				return false;
			}
		}
		return true;
	}
}
