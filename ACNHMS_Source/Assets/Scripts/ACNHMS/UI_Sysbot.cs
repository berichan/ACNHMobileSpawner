using UnityEngine;
using UnityEngine.UI;

public class UI_Sysbot : MonoBehaviour
{
	public const string IPKEY = "IP_SYS";

	public const string SOCKETKEY = "SOCK_SYS";

	public string DefaultIp = "192.168.0.1";

	public string DefaultSocket = "6000";

	public Text ConnectedText;

	public InputField IP;

	public InputField Socket;

	public GameObject RootConnected;

	public GameObject RootNotConnected;

	private SysBotController sysBot;

	private void Start()
	{
		SetConnected(val: false);
		DefaultIp = PlayerPrefs.GetString("IP_SYS", DefaultIp);
		DefaultSocket = PlayerPrefs.GetString("SOCK_SYS", DefaultSocket);
		IP.text=(DefaultIp);
		Socket.text=(DefaultSocket);
	}

	private void Update()
	{
	}

	public void AssignSysbot(SysBotController sb)
	{
		sysBot = sb;
	}

	public void SetConnected(bool val)
	{
		RootConnected.SetActive(val);
		RootNotConnected.SetActive(!val);
	}

	public void TryConnect()
	{
		string error = "";
		if (!sysBot.Connect(IP.text, Socket.text, out error))
		{
			SetConnected(val: false);
			ConnectedText.text=("Connection failed: " + error);
			return;
		}
		SetConnected(val: true);
		ConnectedText.text=("Connected successfully");
		PlayerPrefs.SetString("IP_SYS", IP.text);
		PlayerPrefs.SetString("SOCK_SYS", Socket.text);
	}
}
