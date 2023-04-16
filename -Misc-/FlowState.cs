using System;
using UnityEngine;

public class FlowState
{
	public GUIContent content;

	public string choice = "";

	public FlowState[] next;

	public Enum result;

	public int audioClip = -1;

	private string vowels = "aeiouAEIOU";

	public FlowState(string nchoice, string texture, string ntext, int naudioClip, FlowState[] nnext = null)
		: this(nchoice, texture, ntext, nnext)
	{
		audioClip = naudioClip;
	}

	public FlowState(string nchoice, string texture, string ntext, FlowState[] nnext = null)
	{
		content = new GUIContent(ntext, Resources.Load<Texture2D>("Story/" + texture));
		next = nnext ?? new FlowState[0];
		choice = nchoice;
	}

	public FlowState(string texture, string ntext, int naudioClip, FlowState[] nnext = null)
		: this(texture, ntext, nnext)
	{
		audioClip = naudioClip;
	}

	public FlowState(string texture, string ntext, FlowState[] nnext = null)
	{
		content = new GUIContent(ntext, Resources.Load<Texture2D>("Story/" + texture));
		next = nnext ?? new FlowState[0];
	}

	public FlowState(string nchoice, Enum nresult, int audioClip = -1)
	{
		if (vowels.IndexOf(nresult.ToString()[0]) >= 0)
		{
			content = new GUIContent("You're an " + nresult.ToString() + "!");
		}
		else
		{
			content = new GUIContent("You're a " + nresult.ToString() + "!");
		}
		result = nresult;
		choice = nchoice;
		next = new FlowState[0];
	}
}
