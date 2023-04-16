using UnityEngine;

public class GeneralizedSpirographController : MonoBehaviour
{
	private class SpirographValues
	{
		public float r;

		public float a;

		public float b;

		public float spin;

		public SpirographValues()
		{
		}

		public SpirographValues(float r, float a, float b, float spin)
		{
			this.r = r;
			this.a = a;
			this.b = b;
			this.spin = spin;
		}

		public SpirographValues lerp(SpirographValues o, float t)
		{
			float num = 1f - t;
			return new SpirographValues(r * num + o.r * t, a * num + o.a * t, b * num + o.b * t, spin * num + o.spin * t);
		}
	}

	public int n = 7;

	public int m = 10;

	public bool random;

	public float delta = 0.05f;

	public float thickness = 0.04f;

	public bool followBeats = true;

	private Material mat;

	private SpirographValues prev;

	private SpirographValues next;

	private float t;

	private float moveTime = 0.2f;

	private float tempo = 0.46153846f;

	private float[] beats = new float[220]
	{
		16f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		1f, 1f, 1f, 1f, 9f, 2f, 1f, 1f, 2f, 1f,
		1f, 2f, 2f, 2f, 2f, 2f, 1f, 1f, 2f, 1f,
		1f, 2f, 2f, 4f, 0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f,
		0.25f, 0.25f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f,
		0.25f, 0.25f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f,
		0.25f, 0.25f, 0.5f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f,
		0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f, 1f,
		0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f,
		0.5f, 0.5f, 1f, 0.25f, 0.25f, 0.25f, 0.25f, 1f, 0.5f, 0.5f,
		1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f,
		0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.25f, 0.25f, 0.25f, 0.5f, 0.5f
	};

	private int beat;

	private SpirographValues defaultValues()
	{
		SpirographValues spirographValues = new SpirographValues();
		float num = 2f;
		float num2 = (float)n * num / (float)m;
		spirographValues.r = (num + num2) / 2f;
		spirographValues.a = (num - num2) / spirographValues.r;
		spirographValues.b = spirographValues.a;
		spirographValues.spin = 0f;
		return spirographValues;
	}

	private SpirographValues randomValues()
	{
		SpirographValues spirographValues = new SpirographValues();
		float num = ((!(Random.value < 0.5f)) ? (Random.value * 2f + 1f) : 2f);
		float value = Random.value;
		if (value < 0.33f)
		{
			float num2 = (float)n * num / (float)m;
			spirographValues.r = (num + num2) / 2f;
			spirographValues.a = (num - num2) / spirographValues.r;
			spirographValues.b = spirographValues.a;
		}
		else if (value < 0.67f)
		{
			float num2 = num * Random.value;
			spirographValues.r = (num + num2) / 2f;
			spirographValues.a = (num - num2) / (2f * spirographValues.r);
			spirographValues.b = (float)n * (1f - spirographValues.a) / (float)m - delta;
		}
		else if (value < 0.83f)
		{
			float num2 = num * (3f * Random.value - 1f);
			spirographValues.r = (num + num2) / 2f;
			spirographValues.a = (num - num2) / (2f * spirographValues.r);
			spirographValues.b = spirographValues.a;
		}
		else
		{
			float num2 = num * (3f * Random.value - 1f);
			spirographValues.r = (num + num2) / 2f;
			spirographValues.a = (num - num2) / (2f * spirographValues.r);
			spirographValues.b = spirographValues.a * (4f * Random.value - 2f);
		}
		spirographValues.spin = 100f * (Random.value - 0.5f);
		return spirographValues;
	}

	private void update(SpirographValues v)
	{
		mat.SetFloat("_R", v.r);
		mat.SetFloat("_A", v.a);
		mat.SetFloat("_B", v.b);
		base.gameObject.transform.Rotate(0f, 0f, v.spin * Time.deltaTime);
	}

	private void Start()
	{
		mat = GetComponent<Renderer>().material;
		prev = defaultValues();
		next = prev;
		t = moveTime / 2f;
	}

	private void Update()
	{
		float num = Mathf.Min(t / moveTime, 1f);
		float num2 = 3f * num * num - 2f * num * num * num;
		update(prev.lerp(next, num2));
		if (beat >= beats.Length)
		{
			return;
		}
		t += Time.deltaTime;
		if (followBeats)
		{
			if (t >= beats[beat] * tempo)
			{
				t -= beats[beat] * tempo;
				prev = next;
				next = randomValues();
				beat++;
			}
		}
		else if (t >= tempo)
		{
			t -= tempo;
			prev = next;
			next = randomValues();
		}
	}
}
