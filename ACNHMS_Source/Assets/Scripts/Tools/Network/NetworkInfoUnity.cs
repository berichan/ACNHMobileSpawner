using System;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Net;
using System.Linq;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class NetworkInfoUnity : MonoBehaviour
{
    const float TIMEOUTSECONDS = 3.5f;

    static string macIdentifiers = "http://standards-oui.ieee.org/oui/oui.txt";
    static CountdownEvent countdown;
    static int upCount = 0;
    static int pingCount = 0;
    const bool resolveNames = true;
    static List<string> idents = null;
    static List<string> activeIP = null;
    static Dictionary<string, string> hexToManufacturerHash;
    static string myip;

    ReferenceContainer<float> percentageDone;

    public static void TryCreatePingObject(string passedIP)
    {
        GameObject g = new GameObject("Ping Object");
        var pingclass = g.AddComponent<NetworkInfoUnity>();
        pingclass.TryCreatePingRequest(passedIP);
    }

    public void TryCreatePingRequest(string passedIP)
    {
        var strArr = passedIP.Split('.');
        if (strArr.Length < 4)
        {
            PopupHelper.CreateError("IP could not be parsed correctly. Check and enter a valid IP address.", 2f);
            return;
        }
        string baseString = string.Format("{0}.{1}.{2}.", strArr[0], strArr[1], strArr[2]);
        string s = string.Empty;
#if PLATFORM_ANDROID
        s = "\r\nNetwork state permission may be required to use this tool on Android.";
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_WIFI_STATE"))
        {
            Permission.RequestUserPermission("android.permission.ACCESS_WIFI_STATE");
        }
#endif
        UI_Popup.CurrentInstance.CreatePopupChoice(string.Format("The network helper tool will attempt to ping everything on your network that starts with {0}* and will then show you the list of IPs. This will take about 15-20 seconds and a small 4mb list of manufacturers will be downloaded.{1}", baseString, s),
                                                        "Start", () => { GetPermissionToRunDaemonIfRequired(baseString); }, null,
                                                        "Cancel", () => { });
    }

    private void GetPermissionToRunDaemonIfRequired(string baseString)
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Downloading Manufacturer (Mfr) table and creating hashes...", () => { RunDaemon(baseString); });
    }

    private void RunDaemon(string rootIP)
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_WIFI_STATE"))
        {
            PopupHelper.CreateError("Unable to create pingers, no access to local network.", 2f);
            return;
        }
#endif
        try
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SendUnityDaemon(rootIP);
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }

    public void SendUnityDaemon(string ipBase = "192.168.0.")
    {
        activeIP = new List<string>();
        upCount = 0;
        pingCount = 0;
        myip = IPManager.GetIP(ADDRESSFAM.IPv4);
#if UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_EDITOR_WIN
        try
        {
            string ident = string.Empty;
            using (var webc = new WebClient())
                ident = webc.DownloadString(macIdentifiers);
            Console.WriteLine("Identities downloaded");
            idents = new List<string>(ident.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            idents = idents.Where(x => !x.StartsWith("\t")).Where(x => !x.StartsWith(" ")).ToList();
            hexToManufacturerHash = new Dictionary<string, string>();
            foreach (var line in idents)
            {
                string validMac = line.Split(' ')[0];
                string[] splitline = line.Split('\t');
                if (validMac != string.Empty)
                    if (validMac.Split('-').Length > 2)
                        if (line.Split('\t').Length > 0)
                            if (!hexToManufacturerHash.ContainsKey(validMac))
                                hexToManufacturerHash.Add(validMac, line.Split('\t')[splitline.Length-1]);
            }
            UnityEngine.Debug.Log("Identities created: "+hexToManufacturerHash.Count.ToString());
        }
        catch { }
#endif

        percentageDone = new ReferenceContainer<float>(0);

        for (int i = 1; i < 255; i++)
        {
            string ipToPing = ipBase + i.ToString();
            StartCoroutine(createPinger(ipToPing, TIMEOUTSECONDS + (0.05f * i)));
        }

        UI_Popup.CurrentInstance.CreateProgressBar("Generating pingers for local addresses, please wait...", percentageDone);

        StartCoroutine(createTimer(0.1f + TIMEOUTSECONDS + (0.05f * 255f), () => 
        {
            //percentageDone.UpdateValue(1);
            string s = string.Join("\r\n", activeIP.ToArray());
            UI_Popup.CurrentInstance.CreatePopupChoice($"The following {upCount.ToString()} IP addresses were found:\r\n" + s, 
                                                        "OK!", () => { Screen.sleepTimeout = SleepTimeout.SystemSetting; }, null,
                                                        "Copy list to clipboard", () => { Screen.sleepTimeout = SleepTimeout.SystemSetting; GUIUtility.systemCopyBuffer = s.Replace("<color=red>", string.Empty).Replace("</color>", string.Empty); });
        }));
    }

    private System.Collections.IEnumerator createPinger(string ip, float timeout)
    {
#if !UNITY_WEBGL
        if (ip != myip)
        {
            UnityEngine.Ping ping = new UnityEngine.Ping(ip);
            float timer = 0;
            while (!ping.isDone && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (ping.time != -1)
            {
                upCount++;
                string mac = null;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                mac = getMacByIp(ip);
#elif UNITY_ANDROID
            mac = AndroidUSBUtils.CurrentInstance.GetMacAddressQ(ip);
#endif
                string addit = "Unknown";
                if (idents != null && mac != null)
                {
                    string macLookup = getFirst3MacValues(mac.ToUpper());
                    if (hexToManufacturerHash.ContainsKey(macLookup))
                        addit = hexToManufacturerHash[macLookup];
                    if (addit.StartsWith("Nintendo", StringComparison.OrdinalIgnoreCase))
                        addit = "<color=red>" + addit + "</color>";
                    activeIP.Add(string.Format("IP: {0} Mfr: {1}", ip, addit));
                }
                else
                    activeIP.Add(string.Format("IP: {0}", ip));
            }
            ping.DestroyPing();
        }
        else
            activeIP.Add(string.Format("IP: {0} (This device)", ip));

        pingCount++;
        percentageDone.UpdateValue(pingCount / 255f);
#else
        yield return null;
#endif
    }

    private System.Collections.IEnumerator createTimer(float time, System.Action onEnd)
    {
        yield return new WaitForSeconds(time);
        onEnd.Invoke();
    }

    static Predicate<string> ByFirst3MacValues(string mac)
    {
        return delegate (string entireLine)
        {
            string validMac = entireLine.Split(' ')[0];
            if (validMac != string.Empty)
                if (validMac.Split('-').Length > 2)
                    return (mac.StartsWith(validMac.Split(' ')[0]));
            return false;
        };
    }

    public static string getMacByIp(string ip)
    {
        var macIpPairs = GetAllMacAddressesAndIppairs(ip);
        int index = macIpPairs.FindIndex(x => x.IpAddress == ip);
        if (index >= 0)
        {
            return macIpPairs[index].MacAddress.ToUpper();
        }
        else
        {
            return null;
        }
    }

    public static List<MacIpPair> GetAllMacAddressesAndIppairs(string ipCheck)
    {
        List<MacIpPair> mip = new List<MacIpPair>();
        string cmdOutput;
        System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
        pProcess.StartInfo.FileName = "arp";
        pProcess.StartInfo.Arguments = "-a ";
        pProcess.StartInfo.UseShellExecute = false;
        pProcess.StartInfo.RedirectStandardOutput = true;
        pProcess.StartInfo.CreateNoWindow = true;
        pProcess.Start();
        cmdOutput = pProcess.StandardOutput.ReadToEnd();
        string pattern = @"(?<ip>([0-9]{1,3}\.?){4})\s*(?<mac>([a-f0-9]{2}-?){6})";

        foreach (Match m in Regex.Matches(cmdOutput, pattern, RegexOptions.IgnoreCase))
        {
            mip.Add(new MacIpPair()
            {
                MacAddress = m.Groups["mac"].Value,
                IpAddress = m.Groups["ip"].Value
            });
        }

        return mip;
    }
    public struct MacIpPair
    {
        public string MacAddress;
        public string IpAddress;
    }

    private string getFirst3MacValues(string mac)
    {
        string[] split = mac.Split('-');
        if (split.Length < 3)
            return mac;
        return $"{split[0]}-{split[1]}-{split[2]}";
    }
}
