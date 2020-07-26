using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class NetworkInfoAsync
{
    static string macIdentifiers = "http://standards-oui.ieee.org/oui/oui.txt";
    static CountdownEvent countdown;
    static int upCount = 0;
    static object lockObj = new object();
    const bool resolveNames = true;
    static List<string> idents = null;
    static List<string> activeIP = null;

    public static void TryCreatePingRequest(string passedIP)
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
        s = "\r\nLocation permission may be required to use this tool on Android.";
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
#endif
        UI_Popup.CurrentInstance.CreatePopupChoice(string.Format("The network helper tool will attempt to ping everything on your network that starts with {0}*{1}", baseString, s),
                                                        "Start", () => { GetPermissionToRunDaemonIfRequired(baseString); }, null,
                                                        "Cancel", () => { });
    }

    private static void GetPermissionToRunDaemonIfRequired(string baseString)
    {
        UI_Popup.CurrentInstance.CreatePopupMessage(0.001f, "Generating ping requests, this will take up to 2 minutes", () => { RunDaemon(baseString); });
    }

    private static void RunDaemon(string rootIP)
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            PopupHelper.CreateError("Unable to create pingers, no access to local network.", 2f);
            return;
        }
#endif
        try
        {
            SendDaemonAsync(rootIP);
            string s = string.Join("\r\n", activeIP.ToArray());
            UI_Popup.CurrentInstance.CreatePopupChoice($"The following {upCount.ToString()} IP addresses were found:\r\n" + s, "OK!", () => { });
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public static void SendDaemonAsync(string ipBase = "192.168.0.")
    {
        activeIP = new List<string>();
        upCount = 0;
        try
        {
            string ident = string.Empty;
            using (var webc = new WebClient())
                ident = webc.DownloadString(macIdentifiers);
            Console.WriteLine("Identities downloaded");
            idents = new List<string>(ident.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            idents = idents.Where(x => !x.StartsWith("\t")).ToList();
            UnityEngine.Debug.Log("Identities created");
        }
        catch { }

        countdown = new CountdownEvent(1);
        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = 1; i < 255; i++)
        {
            string ip = ipBase + i.ToString();
            string[] fourByteIp = ip.Split('.');
            if (fourByteIp.Length != 4)
                throw new Exception("IP address did not contain four values");
            byte[] ipBytes = new byte[4] { byte.Parse(fourByteIp[0]), byte.Parse(fourByteIp[1]), byte.Parse(fourByteIp[2]), byte.Parse(fourByteIp[3]) };
            System.Net.IPAddress addr = new System.Net.IPAddress(ipBytes);

            Ping p = new Ping();
            p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
            countdown.AddCount();
            p.SendPingAsync(addr, 500, null);
        }
        countdown.Signal();
        countdown.Wait();
        sw.Stop();
        TimeSpan span = new TimeSpan(sw.ElapsedTicks);
        UnityEngine.Debug.Log(string.Format("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, upCount));
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

    static void p_PingCompleted(object sender, PingCompletedEventArgs e)
    {
        string ip = (string)e.UserState;
        if (e.Reply != null && e.Reply.Status == IPStatus.Success)
        {
            if (resolveNames)
            {
                if (ip == null)
                    ip = e.Reply.Address.ToString();
                var mac = getMacByIp(ip);
                var addit = string.Empty;
                if (idents != null && mac != null)
                {
                    var manufacturer = idents.Find(ByFirst3MacValues(mac));
                    if (manufacturer != string.Empty)
                        addit = manufacturer.Split('\t')[2];
                }

                Console.WriteLine("{0} is up: ({1} ms) ({2}) {3}", ip, e.Reply.RoundtripTime, mac, addit);
                if (addit.StartsWith("Nintendo", StringComparison.OrdinalIgnoreCase))
                    addit = "<color=red>" + addit + "</color>";
                activeIP.Add(string.Format("IP: {0} Manufacturer: {1}", ip, addit));
            }
            else
            {
                Console.WriteLine("{0} is up: ({1} ms)", ip, e.Reply.RoundtripTime);
            }
            lock (lockObj)
            {
                upCount++;
            }
        }
        else if (e.Reply == null)
        {
            Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
        }
        countdown.Signal();
        if (countdown.CurrentCount == 1)
            countdown.Signal(); // Mono pls
    }

    public static string getMacByIp(string ip)
    {
        var macIpPairs = GetAllMacAddressesAndIppairs();
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

    public static List<MacIpPair> GetAllMacAddressesAndIppairs()
    {
        List<MacIpPair> mip = new List<MacIpPair>();
        System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
        pProcess.StartInfo.FileName = "arp";
        pProcess.StartInfo.Arguments = "-a ";
        pProcess.StartInfo.UseShellExecute = false;
        pProcess.StartInfo.RedirectStandardOutput = true;
        pProcess.StartInfo.CreateNoWindow = true;
        pProcess.Start();
        string cmdOutput = pProcess.StandardOutput.ReadToEnd();
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
}
