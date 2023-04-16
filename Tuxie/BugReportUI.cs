using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Tuxie;

public class BugReportUI : MonoBehaviour
{
	public class ProgressStatus
	{
		public bool screenshot;

		public bool logcopied;

		public bool savedsession;

		public bool compressed;

		public long progress1;

		public long progress2;

		public bool uploaded;

		public long uploadedprogress;

		public bool complete;
	}

	private Transform statuslist;

	public GameObject Step1;

	public GameObject Step2;

	public GameObject Step3;

	public GameObject ErrorHandler;

	public Text errormessage;

	public InputField MessageBox;

	public Text LandingPage;

	public Text LaunchBugReportButtonText;

	private static string datafiledir = "";

	private BugReportBackend backend = new BugReportBackend();

	public object ProgressLock = new object();

	private volatile ProgressStatus _UIProgressStatus;

	private IEnumerator beforesubmitcoroutine;

	private IEnumerator cr;

	public ProgressStatus UIProgressStatus
	{
		get
		{
			return _UIProgressStatus;
		}
		set
		{
			lock (ProgressLock)
			{
				_UIProgressStatus = value;
			}
		}
	}

	public void ShowUI()
	{
		base.gameObject.SetActive(value: true);
		base.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		if (statuslist == null)
		{
			statuslist = base.transform.Find("Viewport/Content/Step2/StatusList");
			beforesubmitcoroutine = beforesubmit();
			StartCoroutine(beforesubmitcoroutine);
			Step1.SetActive(value: true);
			Step2.SetActive(value: false);
			Step3.SetActive(value: false);
		}
	}

	public void CloseUI()
	{
		base.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
	}

	private void LoadTextFiles()
	{
		MessageBox.text = File.ReadAllText(Application.streamingAssetsPath + "/BugReportQuestionaire.txt");
		LandingPage.text = File.ReadAllText(Application.streamingAssetsPath + "/BugReportGetYouOutOfThere.txt");
	}

	public void ResetUI()
	{
		Step1.SetActive(value: true);
		Step2.SetActive(value: false);
		Step3.SetActive(value: false);
		beforesubmitcoroutine = beforesubmit();
		StartCoroutine(beforesubmitcoroutine);
	}

	private void Start()
	{
	}

	public void OnBugReportSubmit()
	{
		cr = doBugReport();
		StartCoroutine(cr);
	}

	public void ResendBugreport()
	{
	}

	public void update_statusmessage(ProgressStatus progressStatus)
	{
		lock (ProgressLock)
		{
			setCheckBox("ScreenshotTaken", progressStatus.screenshot, "Screenshot taken");
			setCheckBox("LogCopied", progressStatus.logcopied, "Log copied");
			setCheckBox("SavedSession", progressStatus.savedsession, "Saved session");
			if (backend.originalFileSize != 0L)
			{
				string text = (Convert.ToDouble(progressStatus.progress1) / Convert.ToDouble(backend.originalFileSize) * 100.0).ToString("0.00");
				string text2 = progressStatus.progress1 / 1000 / 1000 + "Mb";
				string text3 = progressStatus.progress2 / 1000 + "Kb";
				setCheckBox("CompressedLog", progressStatus.compressed, progressStatus.compressed ? "Compressed" : ("Compressed (Progress: " + text + " % Compressed: " + text2 + " To: " + text3 + ")"));
			}
			else
			{
				setCheckBox("CompressedLog", progressStatus.compressed, "Divide by zero :(");
			}
			setCheckBox("Uploaded", progressStatus.uploaded, "Uploaded");
			setCheckBox("Completed", progressStatus.complete, "Completed");
		}
	}

	private void Update()
	{
		try
		{
			update_statusmessage(UIProgressStatus);
		}
		catch (NullReferenceException)
		{
		}
	}

	public void setCheckBox(string name, bool ischecked, string text)
	{
		Transform obj = statuslist.Find(name);
		Text componentInChildren = obj.GetComponentInChildren<Text>();
		obj.GetComponentInChildren<Toggle>().isOn = ischecked;
		componentInChildren.text = text;
	}

	private void Backend_UploadUpdate(object sender, BugReportBackend.UploadUpdateEventArgs e)
	{
		UIProgressStatus = e.ProgressStatus;
	}

	private void Backend_HttpError(object sender, BugReportBackend.HttpErrorEventArgs e)
	{
		ErrorHandler.SetActive(value: true);
		errormessage.text = e.error;
		LaunchBugReportButtonText.text = "Error!";
	}

	private void Backend_OnDone()
	{
		Step1.SetActive(value: false);
		Step2.SetActive(value: false);
		Step3.SetActive(value: true);
		LaunchBugReportButtonText.text = "Done!";
	}

	private IEnumerator beforesubmit()
	{
		yield return 0;
		LoadTextFiles();
		string text = Application.streamingAssetsPath + "/../../BugReports/";
		Directory.CreateDirectory(text);
		DirectoryInfo directoryInfo = new DirectoryInfo(text);
		FileInfo[] files = directoryInfo.GetFiles();
		for (int i = 0; i < files.Length; i++)
		{
			files[i].Delete();
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		for (int i = 0; i < directories.Length; i++)
		{
			directories[i].Delete(recursive: true);
		}
		datafiledir = text + DateTime.Now.ToString("ddMMyyHHmmss") + "/";
		Directory.CreateDirectory(datafiledir);
		backend = new BugReportBackend();
		backend.parent = this;
		backend.UploadUpdate += Backend_UploadUpdate;
		backend.HttpError += Backend_HttpError;
		backend.OnDone += Backend_OnDone;
		UIProgressStatus = new ProgressStatus();
		base.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
		yield return 0;
		yield return backend.captureScreenshotBlocking();
		base.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		yield return 0;
		UIProgressStatus.screenshot = true;
		File.Copy(Application.consoleLogPath, datafiledir + "log.txt");
		UIProgressStatus.logcopied = true;
	}

	private IEnumerator doBugReport()
	{
		Step1.SetActive(value: false);
		Step2.SetActive(value: true);
		Step3.SetActive(value: false);
		ErrorHandler.SetActive(value: false);
		LaunchBugReportButtonText.text = "Uploading!";
		yield return backend.upload(datafiledir, base.transform.Find("Viewport/Content/Step1/DebugFirstPageTextbox").GetComponent<InputField>().text);
	}
}
