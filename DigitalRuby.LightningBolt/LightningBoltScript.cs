using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.LightningBolt;

[RequireComponent(typeof(LineRenderer))]
public class LightningBoltScript : MonoBehaviour
{
	[Tooltip("The game object where the lightning will emit from. If null, StartPosition is used.")]
	public GameObject StartObject;

	[Tooltip("The start position where the lightning will emit from. This is in world space if StartObject is null, otherwise this is offset from StartObject position.")]
	public Vector3 StartPosition;

	[Tooltip("The game object where the lightning will end at. If null, EndPosition is used.")]
	public GameObject EndObject;

	[Tooltip("The end position where the lightning will end at. This is in world space if EndObject is null, otherwise this is offset from EndObject position.")]
	public Vector3 EndPosition;

	[Range(0f, 8f)]
	[Tooltip("How manu generations? Higher numbers create more line segments.")]
	public int Generations = 6;

	[Range(0.01f, 1f)]
	[Tooltip("How long each bolt should last before creating a new bolt. In ManualMode, the bolt will simply disappear after this amount of seconds.")]
	public float Duration = 0.05f;

	private float timer;

	[Range(0f, 1f)]
	[Tooltip("How chaotic should the lightning be? (0-1)")]
	public float ChaosFactor = 0.15f;

	[Tooltip("In manual mode, the trigger method must be called to create a bolt")]
	public bool ManualMode;

	[Range(1f, 64f)]
	[Tooltip("The number of rows in the texture. Used for animation.")]
	public int Rows = 1;

	[Range(1f, 64f)]
	[Tooltip("The number of columns in the texture. Used for animation.")]
	public int Columns = 1;

	[Tooltip("The animation mode for the lightning")]
	public LightningBoltAnimationMode AnimationMode = LightningBoltAnimationMode.PingPong;

	[NonSerialized]
	[HideInInspector]
	public System.Random RandomGenerator = new System.Random();

	private LineRenderer lineRenderer;

	private List<KeyValuePair<Vector3, Vector3>> segments = new List<KeyValuePair<Vector3, Vector3>>();

	private int startIndex;

	private Vector2 size;

	private Vector2[] offsets;

	private int animationOffsetIndex;

	private int animationPingPongDirection = 1;

	private bool orthographic;

	private bool startHadObject;

	private bool endHadObject;

	private void GetPerpendicularVector(ref Vector3 directionNormalized, out Vector3 side)
	{
		if (directionNormalized == Vector3.zero)
		{
			side = Vector3.right;
			return;
		}
		float x = directionNormalized.x;
		float y = directionNormalized.y;
		float z = directionNormalized.z;
		float num = Mathf.Abs(x);
		float num2 = Mathf.Abs(y);
		float num3 = Mathf.Abs(z);
		float num4;
		float num5;
		float num6;
		if (num >= num2 && num2 >= num3)
		{
			num4 = 1f;
			num5 = 1f;
			num6 = (0f - (y * num4 + z * num5)) / x;
		}
		else if (num2 >= num3)
		{
			num6 = 1f;
			num5 = 1f;
			num4 = (0f - (x * num6 + z * num5)) / y;
		}
		else
		{
			num6 = 1f;
			num4 = 1f;
			num5 = (0f - (x * num6 + y * num4)) / z;
		}
		side = new Vector3(num6, num4, num5).normalized;
	}

	private void GenerateLightningBolt(Vector3 start, Vector3 end, int generation, int totalGenerations, float offsetAmount)
	{
		if (generation < 0 || generation > 8)
		{
			return;
		}
		if (orthographic)
		{
			start.z = (end.z = Mathf.Min(start.z, end.z));
		}
		segments.Add(new KeyValuePair<Vector3, Vector3>(start, end));
		if (generation == 0)
		{
			return;
		}
		if (offsetAmount <= 0f)
		{
			offsetAmount = (end - start).magnitude * ChaosFactor;
		}
		while (generation-- > 0)
		{
			int num = startIndex;
			startIndex = segments.Count;
			for (int i = num; i < startIndex; i++)
			{
				start = segments[i].Key;
				end = segments[i].Value;
				Vector3 vector = (start + end) * 0.5f;
				RandomVector(ref start, ref end, offsetAmount, out var result);
				vector += result;
				segments.Add(new KeyValuePair<Vector3, Vector3>(start, vector));
				segments.Add(new KeyValuePair<Vector3, Vector3>(vector, end));
			}
			offsetAmount *= 0.5f;
		}
	}

	public void RandomVector(ref Vector3 start, ref Vector3 end, float offsetAmount, out Vector3 result)
	{
		if (orthographic)
		{
			Vector3 normalized = (end - start).normalized;
			Vector3 vector = new Vector3(0f - normalized.y, normalized.x, normalized.z);
			float num = (float)RandomGenerator.NextDouble() * offsetAmount * 2f - offsetAmount;
			result = vector * num;
		}
		else
		{
			Vector3 directionNormalized = (end - start).normalized;
			GetPerpendicularVector(ref directionNormalized, out var side);
			float num2 = ((float)RandomGenerator.NextDouble() + 0.1f) * offsetAmount;
			float angle = (float)RandomGenerator.NextDouble() * 360f;
			result = Quaternion.AngleAxis(angle, directionNormalized) * side * num2;
		}
	}

	private void SelectOffsetFromAnimationMode()
	{
		if (AnimationMode == LightningBoltAnimationMode.None)
		{
			lineRenderer.material.mainTextureOffset = offsets[0];
			return;
		}
		int num;
		if (AnimationMode == LightningBoltAnimationMode.PingPong)
		{
			num = animationOffsetIndex;
			animationOffsetIndex += animationPingPongDirection;
			if (animationOffsetIndex >= offsets.Length)
			{
				animationOffsetIndex = offsets.Length - 2;
				animationPingPongDirection = -1;
			}
			else if (animationOffsetIndex < 0)
			{
				animationOffsetIndex = 1;
				animationPingPongDirection = 1;
			}
		}
		else if (AnimationMode == LightningBoltAnimationMode.Loop)
		{
			num = animationOffsetIndex++;
			if (animationOffsetIndex >= offsets.Length)
			{
				animationOffsetIndex = 0;
			}
		}
		else
		{
			num = RandomGenerator.Next(0, offsets.Length);
		}
		if (num >= 0 && num < offsets.Length)
		{
			lineRenderer.material.mainTextureOffset = offsets[num];
		}
		else
		{
			lineRenderer.material.mainTextureOffset = offsets[0];
		}
	}

	private void UpdateLineRenderer()
	{
		int num = segments.Count - startIndex + 1;
		lineRenderer.positionCount = num;
		if (num >= 1)
		{
			int num2 = 0;
			lineRenderer.SetPosition(num2++, segments[startIndex].Key);
			for (int i = startIndex; i < segments.Count; i++)
			{
				lineRenderer.SetPosition(num2++, segments[i].Value);
			}
			segments.Clear();
			SelectOffsetFromAnimationMode();
		}
	}

	private void Start()
	{
		orthographic = false;
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 0;
		UpdateFromMaterialChange();
		startHadObject = StartObject != null;
		endHadObject = EndObject != null;
	}

	private void Update()
	{
		orthographic = false;
		if (timer <= 0f)
		{
			if (ManualMode)
			{
				timer = Duration;
				lineRenderer.positionCount = 0;
			}
			else
			{
				Trigger();
			}
		}
		timer -= Time.deltaTime;
	}

	public void Trigger()
	{
		timer = Duration + Mathf.Min(0f, timer);
		Vector3 start;
		if (StartObject == null)
		{
			if (startHadObject)
			{
				UnityEngine.Object.Destroy(base.gameObject, timer);
				return;
			}
			start = StartPosition;
		}
		else
		{
			start = StartObject.transform.position + StartPosition;
		}
		Vector3 end;
		if (EndObject == null)
		{
			if (endHadObject)
			{
				UnityEngine.Object.Destroy(base.gameObject, timer);
				return;
			}
			end = EndPosition;
		}
		else
		{
			end = EndObject.transform.position + EndPosition;
		}
		startIndex = 0;
		GenerateLightningBolt(start, end, Generations, Generations, 0f);
		UpdateLineRenderer();
	}

	public void UpdateFromMaterialChange()
	{
		size = new Vector2(1f / (float)Columns, 1f / (float)Rows);
		lineRenderer.material.mainTextureScale = size;
		offsets = new Vector2[Rows * Columns];
		for (int i = 0; i < Rows; i++)
		{
			for (int j = 0; j < Columns; j++)
			{
				offsets[j + i * Columns] = new Vector2((float)j / (float)Columns, (float)i / (float)Rows);
			}
		}
	}
}
