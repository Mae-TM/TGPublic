using System.Collections.Generic;
using System.IO;
using System.Linq;
using Steamworks.Ugc;
using UnityEngine;

public static class StreamingAssets
{
	private static IEnumerable<string> Paths => (from item in AbstractAttachedSingletonManager<SteamManager>.Instance.SubscribedItems.Where(delegate(Steamworks.Ugc.Item item)
		{
			if (item.Directory == null)
			{
				Debug.LogError($"Workshop item had a null directory, ignoring: {item.Id}");
			}
			return item.Directory != null;
		})
		select item.Directory).Prepend(Application.streamingAssetsPath);

	public static bool TryGetFile(string name, out string path)
	{
		path = Paths.Select((string path) => Path.Combine(path, name)).FirstOrDefault(File.Exists);
		return path != null;
	}

	public static IEnumerable<string> ReadLines(string name)
	{
		return Paths.Select((string path) => Path.Combine(path, name)).Where(File.Exists).SelectMany(File.ReadLines);
	}

	public static string[] ReadAllLines(string name)
	{
		return ReadLines(name).ToArray();
	}

	public static IEnumerable<FileInfo> GetDirectoryContents(string name, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
	{
		return (from path in Paths
			select new DirectoryInfo(Path.Combine(path, name)) into directory
			where directory.Exists
			select directory).SelectMany((DirectoryInfo dir) => dir.EnumerateFiles(searchPattern, searchOption));
	}
}
