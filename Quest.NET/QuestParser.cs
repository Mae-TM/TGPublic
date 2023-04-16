using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using Newtonsoft.Json;
using Quest.NET.Interfaces;
using UnityEngine;

namespace Quest.NET;

public class QuestParser
{
	private readonly Script script;

	private readonly IDictionary<string, Type> objectiveList;

	private readonly IDictionary<string, Type> rewardList;

	public QuestParser()
	{
		objectiveList = GetTypes(typeof(IQuestObjective), "Objective");
		rewardList = GetTypes(typeof(IReward), "Reward");
		script = new Script(CoreModules.Preset_SoftSandbox);
		script.Options.ScriptLoader = new FileSystemScriptLoader();
		((ScriptLoaderBase)script.Options.ScriptLoader).IgnoreLuaPathGlobal = true;
		script.Options.DebugPrint = Debug.Log;
		UserData.RegisterType<Exile.Action>();
		script.Globals["Exile"] = UserData.CreateStatic<Exile.Action>();
		script.Globals["SetExile"] = new Func<Exile.Action, bool, bool, bool>(Exile.SetAction);
		UserData.RegisterType<Player>();
		UserData.RegisterType<KernelSprite>();
		script.Globals["Player"] = UserData.CreateStatic<Player>();
		UserData.RegisterType<Quest>();
		script.Globals["SetCallbackGenerator"] = (Action<Quest, int, ScriptFunctionDelegate>)delegate(Quest quest, int index, ScriptFunctionDelegate callbackGenerator)
		{
			((CallbackObjective)quest.Objectives[index]).SetCallbackGenerator(callbackGenerator);
		};
		script.DoString("function SetCallback(quest, index, event, remove)\n                function CallbackGenerator(Complete)\n                    if remove then\n                        callback = function() Complete() end\n                        event(callback)\n                        return function() remove(callback) end\n                    end\n                    if type(event) == 'function' then\n                        event = event()\n                        if not event then return nil end\n                    end\n                    callback = function() Complete() end\n                    event.add(callback)\n                    return function() event.remove(callback) end\n                end\n                SetCallbackGenerator(quest, index, CallbackGenerator)\n            end");
		UserData.RegisterType<Guardian>();
		script.Globals["GetGuardian"] = new Func<Guardian>(GetComponentInHouse<Guardian>);
		UserData.RegisterType<Cruxtruder>();
		script.Globals["GetCruxtruder"] = new Func<Cruxtruder>(GetComponentInHouse<Cruxtruder>);
		UserData.RegisterType<TotemLathe>();
		script.Globals["GetTotemLathe"] = new Func<TotemLathe>(GetComponentInHouse<TotemLathe>);
		UserData.RegisterType<SpeakAction>();
	}

	private static T GetComponentInHouse<T>()
	{
		return Player.player.transform.root.GetComponentInChildren<T>();
	}

	private static Dictionary<string, Type> GetTypes(Type baseType, string suffix)
	{
		Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
		foreach (Type item in from t in baseType.Assembly.GetTypes()
			where baseType.IsAssignableFrom(t) && !t.IsAbstract
			select t)
		{
			string name = item.Name;
			name = name.Substring(0, name.Length - suffix.Length);
			name = name.ToLowerInvariant();
			dictionary[name] = item;
		}
		return dictionary;
	}

	public Quest Parse(string name)
	{
		if (!StreamingAssets.TryGetFile("Quests/" + name + ".json", out var path))
		{
			return null;
		}
		string text = null;
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
		Quest quest;
		using (StreamReader reader = File.OpenText(path))
		{
			using JsonTextReader jsonTextReader = new JsonTextReader(reader);
			string name2 = null;
			string descriptionSummary = null;
			List<IQuestObjective> list = new List<IQuestObjective>();
			List<IReward> list2 = new List<IReward>();
			List<string> next = null;
			List<IQuestObjective> list3 = new List<IQuestObjective>();
			while (jsonTextReader.Read())
			{
				if (jsonTextReader.TokenType != JsonToken.PropertyName)
				{
					continue;
				}
				switch ((string)jsonTextReader.Value)
				{
				case "title":
					name2 = jsonTextReader.ReadAsString();
					break;
				case "description":
					descriptionSummary = jsonTextReader.ReadAsString();
					break;
				case "objectives":
					jsonTextReader.Read();
					while (jsonTextReader.Read() && jsonTextReader.TokenType != JsonToken.EndArray)
					{
						jsonTextReader.Read();
						Type objectType3 = objectiveList[jsonTextReader.ReadAsString().ToLowerInvariant()];
						jsonTextReader.Read();
						jsonTextReader.Read();
						list.Add((IQuestObjective)jsonSerializer.Deserialize(jsonTextReader, objectType3));
						jsonTextReader.Read();
					}
					break;
				case "specialObjectives":
					jsonTextReader.Read();
					while (jsonTextReader.Read() && jsonTextReader.TokenType != JsonToken.EndArray)
					{
						jsonTextReader.Read();
						Type objectType2 = objectiveList[jsonTextReader.ReadAsString().ToLowerInvariant()];
						jsonTextReader.Read();
						jsonTextReader.Read();
						list3.Add((IQuestObjective)jsonSerializer.Deserialize(jsonTextReader, objectType2));
						jsonTextReader.Read();
					}
					break;
				case "rewards":
					jsonTextReader.Read();
					while (jsonTextReader.Read() && jsonTextReader.TokenType != JsonToken.EndArray)
					{
						jsonTextReader.Read();
						Type objectType = rewardList[jsonTextReader.ReadAsString().ToLowerInvariant()];
						jsonTextReader.Read();
						jsonTextReader.Read();
						list2.Add((IReward)jsonSerializer.Deserialize(jsonTextReader, objectType));
						jsonTextReader.Read();
					}
					break;
				case "next":
					jsonTextReader.Read();
					next = jsonSerializer.Deserialize<List<string>>(jsonTextReader);
					break;
				case "script":
					text = jsonTextReader.ReadAsString();
					break;
				}
			}
			quest = new Quest(new QuestText(name2, descriptionSummary, null, null), new QuestIdentifier(name), list, list2, next, list3);
		}
		if (text != null)
		{
			script.Globals["quest"] = quest;
			if (text.EndsWith(".lua"))
			{
				StreamingAssets.TryGetFile("Quests/" + text, out var path2);
				script.DoFile(path2);
			}
			else
			{
				script.DoString(text);
			}
		}
		return quest;
	}
}
