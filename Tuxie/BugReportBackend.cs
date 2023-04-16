using System;
using System.Collections;
using System.IO;
using Ionic.Zip;
using SevenZip;
using UnityEngine;
using UnityEngine.Networking;

namespace Tuxie;

internal class BugReportBackend
{
	public class UploadUpdateEventArgs : EventArgs
	{
		public BugReportUI.ProgressStatus ProgressStatus;

		public UploadUpdateEventArgs(BugReportUI.ProgressStatus progressStatus)
		{
			ProgressStatus = progressStatus;
		}
	}

	public class HttpErrorEventArgs : EventArgs
	{
		public string error;

		public HttpErrorEventArgs(string error)
		{
			this.error = error;
		}
	}

	private class progressinfo : ICodeProgress
	{
		private BugReportBackend parent;

		public progressinfo(BugReportBackend parent)
		{
			this.parent = parent;
		}

		public void SetProgress(long inSize, long outSize)
		{
			parent.OnUploadUpdate(new UploadUpdateEventArgs(new BugReportUI.ProgressStatus
			{
				screenshot = true,
				logcopied = true,
				savedsession = true,
				progress1 = inSize,
				progress2 = outSize
			}));
		}
	}

	public delegate void DGDoneRaiser();

	public delegate void DGScreenshotDoneRaiser();

	public BugReportUI parent;

	private IEnumerator compressorcoroutine;

	public byte[] imageBytes;

	public long originalFileSize;

	private progressinfo p;

	public event EventHandler<UploadUpdateEventArgs> UploadUpdate;

	public event EventHandler<HttpErrorEventArgs> HttpError;

	public event DGDoneRaiser OnDone;

	public event DGScreenshotDoneRaiser OnScreenshotDone;

	protected virtual void OnUploadUpdate(UploadUpdateEventArgs e)
	{
		this.UploadUpdate?.Invoke(this, e);
	}

	protected virtual void OnHttpError(HttpErrorEventArgs e)
	{
		this.HttpError?.Invoke(this, e);
	}

	public IEnumerator captureScreenshotBlocking()
	{
		yield return new WaitForEndOfFrame();
		Texture2D texture2D = new Texture2D(Screen.width, Screen.height);
		texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture2D.Apply();
		imageBytes = texture2D.EncodeToPNG();
		this.OnScreenshotDone?.Invoke();
	}

	public IEnumerator upload(string datafiledir, string message)
	{
		ZipFile zipFile = new ZipFile();
		MemoryStream ms = new MemoryStream();
		zipFile.AddFiles(Directory.GetFiles(datafiledir, "*.bin"));
		zipFile.Save(ms);
		OnUploadUpdate(new UploadUpdateEventArgs(new BugReportUI.ProgressStatus
		{
			screenshot = true,
			logcopied = true,
			savedsession = true
		}));
		if (!File.Exists(datafiledir + "log_tail.lzma"))
		{
			using MemoryStream outMemStream = new MemoryStream();
			int num = 2097152;
			byte[] buffer = new byte[num];
			using (FileStream fileStream = new FileStream(datafiledir + "log.txt", FileMode.Open, FileAccess.Read))
			{
				if (fileStream.Length > num)
				{
					fileStream.Seek(-num, SeekOrigin.End);
				}
				fileStream.Read(buffer, 0, num);
				outMemStream.Write(buffer, 0, num);
			}
			outMemStream.Seek(0L, SeekOrigin.Begin);
			p = new progressinfo(this);
			originalFileSize = num;
			using FileStream output = new FileStream(datafiledir + "log_tail.lzma", FileMode.CreateNew, FileAccess.Write);
			Compression compression = new Compression(outMemStream, output, p);
			compressorcoroutine = compression.CompressLZMA(p);
			yield return parent.StartCoroutine(compressorcoroutine);
		}
		if (!File.Exists(datafiledir + "log.lzma"))
		{
			using FileStream output = new FileStream(datafiledir + "log.txt", FileMode.Open, FileAccess.Read);
			using FileStream compressedLogWriter = new FileStream(datafiledir + "log.lzma", FileMode.CreateNew, FileAccess.Write);
			originalFileSize = output.Length;
			p = new progressinfo(this);
			Compression compression = new Compression(output, compressedLogWriter, p);
			compressorcoroutine = compression.CompressLZMA(p);
			yield return parent.StartCoroutine(compressorcoroutine);
		}
		OnUploadUpdate(new UploadUpdateEventArgs(new BugReportUI.ProgressStatus
		{
			screenshot = true,
			logcopied = true,
			savedsession = true,
			compressed = true
		}));
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("text", message);
		wWWForm.AddBinaryData("unitylog", File.ReadAllBytes(datafiledir + "log.lzma"), "unitylog.bin", "application/binary");
		wWWForm.AddBinaryData("unitylog_tail", File.ReadAllBytes(datafiledir + "log_tail.lzma"), "unitylog_tail.bin", "application/binary");
		wWWForm.AddBinaryData("screenshot", imageBytes, "screenshot.png", "image/png");
		wWWForm.AddBinaryData("sessionzip", ms.ToArray(), "session.zip", "application/zip");
		UnityWebRequest www = UnityWebRequest.Post(Settings.APIURL + "/bugreport/", wWWForm);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError)
		{
			Debug.Log(www.error);
			OnHttpError(new HttpErrorEventArgs(www.error));
			yield break;
		}
		Debug.Log(www.downloadHandler.text);
		OnUploadUpdate(new UploadUpdateEventArgs(new BugReportUI.ProgressStatus
		{
			screenshot = true,
			logcopied = true,
			savedsession = true,
			compressed = true,
			uploaded = true,
			complete = true
		}));
		this.OnDone?.Invoke();
	}
}
