using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClasspectQuiz : MonoBehaviour
{
	public struct Question
	{
		public readonly string text;

		public readonly string[] options;

		public readonly byte[] moons;

		public readonly Aspect[][] aspects;

		public readonly Class[][] classes;

		public Question(string text, StreamReader reader)
		{
			Queue<string> queue = new Queue<string>();
			Queue<byte> queue2 = new Queue<byte>();
			Queue<Aspect[]> queue3 = new Queue<Aspect[]>();
			Queue<Class[]> queue4 = new Queue<Class[]>();
			Queue<Aspect> queue5 = new Queue<Aspect>();
			Queue<Class> queue6 = new Queue<Class>();
			string text2 = reader.ReadLine();
			while (!string.IsNullOrEmpty(text2))
			{
				queue.Enqueue(text2.Trim());
				text2 = reader.ReadLine();
				byte b = (byte)(text2.Contains("Prospit") ? 1u : 0u);
				if (text2.Contains("Derse"))
				{
					b = (byte)(b | 2u);
				}
				queue2.Enqueue(b);
				text2 = reader.ReadLine();
				for (Aspect aspect = Aspect.Time; aspect < Aspect.Count; aspect++)
				{
					if (text2.Contains(aspect.ToString()))
					{
						queue5.Enqueue(aspect);
					}
				}
				queue3.Enqueue(queue5.ToArray());
				queue5.Clear();
				text2 = reader.ReadLine();
				for (Class @class = Class.Maid; @class < Class.Count; @class++)
				{
					if (text2.Contains(@class.ToString()))
					{
						queue6.Enqueue(@class);
					}
				}
				queue4.Enqueue(queue6.ToArray());
				queue6.Clear();
				text2 = reader.ReadLine();
			}
			this.text = text.Trim();
			options = queue.ToArray();
			moons = queue2.ToArray();
			classes = queue4.ToArray();
			aspects = queue3.ToArray();
		}
	}

	[SerializeField]
	private Text[] buttonText;

	[SerializeField]
	private Text content;

	[SerializeField]
	private Text pageText;

	private TypingEffect typeEffect;

	[SerializeField]
	private int[] moonScore;

	[SerializeField]
	private int[] classScore;

	[SerializeField]
	private int[] aspectScore;

	private readonly Queue<Question> questions = new Queue<Question>();

	[SerializeField]
	private LobbyClasspectWizard LobbyClasspectWizard;

	public void OnEnable()
	{
		ResetQuiz();
		NextQuestion();
	}

	public void ResetQuiz()
	{
		typeEffect = new TypingEffect(content);
		moonScore = new int[2];
		classScore = new int[12];
		aspectScore = new int[12];
		questions.Clear();
		FileInfo[] array = StreamingAssets.GetDirectoryContents("Quiz", "*.txt", SearchOption.AllDirectories).ToArray();
		using StreamReader reader = array[Random.Range(0, array.Length)].OpenText();
		ReadQuestions(reader);
	}

	private void ReadQuestions(StreamReader reader)
	{
		uint num = uint.Parse(reader.ReadLine());
		byte b = byte.Parse(reader.ReadLine());
		string text = reader.ReadLine();
		while (text != null && num != 0)
		{
			while (text == string.Empty)
			{
				text = reader.ReadLine();
			}
			if (Random.Range(0f, num) < (float)(int)b)
			{
				questions.Enqueue(new Question(text, reader));
				b = (byte)(b - 1);
				text = reader.ReadLine();
			}
			else
			{
				while (!string.IsNullOrEmpty(text))
				{
					text = reader.ReadLine();
				}
			}
			num--;
		}
	}

	public void NextQuestion()
	{
		pageText.text = $"{questions.Count} left";
		if (questions.Count == 0)
		{
			TestFinished();
			return;
		}
		Question question = questions.Peek();
		typeEffect.SetText(question.text);
		StopAllCoroutines();
		StartCoroutine(Type());
		for (int i = 0; i < buttonText.Length; i++)
		{
			if (i < question.options.Length)
			{
				buttonText[i].transform.parent.gameObject.SetActive(value: true);
				buttonText[i].text = question.options[i];
			}
			else
			{
				buttonText[i].transform.parent.gameObject.SetActive(value: false);
			}
		}
	}

	public void ButtonClick(int i)
	{
		Question question = questions.Dequeue();
		if ((question.moons[i] & 1) == 1)
		{
			moonScore[0]++;
		}
		if ((question.moons[i] & 2) == 2)
		{
			moonScore[1]++;
		}
		Aspect[] array = question.aspects[i];
		foreach (Aspect aspect in array)
		{
			aspectScore[(int)aspect]++;
		}
		Class[] array2 = question.classes[i];
		foreach (Class @class in array2)
		{
			classScore[(int)@class]++;
		}
		NextQuestion();
	}

	private IEnumerator Type()
	{
		bool flag = true;
		typeEffect.Reset();
		while (flag)
		{
			yield return new WaitForSeconds(0.02f);
			flag = typeEffect.TypeNext();
		}
	}

	public void TestFinished()
	{
		if (moonScore[0] > moonScore[1])
		{
			Debug.LogError("Prospit");
		}
		else if (moonScore[0] < moonScore[1])
		{
			Debug.LogError("Derse");
		}
		else
		{
			Debug.LogError("Undetermined moon");
		}
		ClassPick.score = classScore;
		AspectPick.score = aspectScore;
		LobbyClasspectWizard.TriggerOnDone();
	}
}
