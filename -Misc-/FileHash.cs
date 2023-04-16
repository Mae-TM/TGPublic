using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal class FileHash
{
	private static string GetMd5(string input)
	{
		byte[] array = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	private static string GetMd5(byte[] byteStream)
	{
		byte[] array = MD5.Create().ComputeHash(byteStream);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	private static StreamReader GetFile(string fileURI)
	{
		string text = "";
		StreamReader streamReader = null;
		using (FileStream stream = File.Open(fileURI, FileMode.Open))
		{
			streamReader = new StreamReader(stream);
			text = streamReader.ToString();
		}
		if (text.Equals(""))
		{
			throw new IOException("Can't open file. That's weird.");
		}
		return streamReader;
	}

	private static string GetPWD()
	{
		return Directory.GetCurrentDirectory();
	}

	private static string GetTempDir()
	{
		return Path.GetTempPath();
	}

	private static string GetAppPath()
	{
		return Application.dataPath;
	}

	private static string[] GetFolderHash()
	{
		string[] files = Directory.GetFiles(GetAppPath() + "/Resources/", "*", SearchOption.AllDirectories);
		string[] array = new string[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			string input = "";
			using (StreamReader streamReader = GetFile(files[i]))
			{
				input = streamReader.ReadToEnd();
			}
			array[i] = GetMd5(input);
		}
		return array;
	}

	private static string[] GetRemoteHashes()
	{
		UnityWebRequest unityWebRequest = UnityWebRequest.Get(Settings.APIURL + "/get_hashes");
		unityWebRequest.SendWebRequest();
		while (!unityWebRequest.isDone)
		{
		}
		return JsonUtility.FromJson<string[]>(unityWebRequest.downloadHandler.text);
	}

	public static void DownloadUpdate()
	{
	}

	public static bool CheckHashes()
	{
		string[] folderHash = GetFolderHash();
		string[] remoteHashes = GetRemoteHashes();
		List<string> list = new List<string>(folderHash);
		list.Sort();
		List<string> list2 = new List<string>(remoteHashes);
		list2.Sort();
		return list.Equals(list2);
	}
}
