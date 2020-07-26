using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NetworkInfo
{
    private static List<System.Net.NetworkInformation.Ping> pingers = new List<System.Net.NetworkInformation.Ping>();
    private static int instances = 0;

    private static object @lock = new object();

    private static int result = 0;
    private static int timeOut = 100;

    private static int ttl = 5;

    private static List<string> activeIP = new List<string>();

    public static void TryCreatePingRequest(string passedIP)
    {
        var strArr = passedIP.Split('.');
        if (strArr.Length < 4)
        {
            PopupHelper.CreateError("IP could not be parsed correctly. Check and enter a valid IP address.", 2f);
            return;
        }
        string baseString = string.Format("{0}.{1}.{2}.", strArr[0], strArr[1], strArr[2]);
        UI_Popup.CurrentInstance.CreatePopupChoice(string.Format("The network helper tool will attempt to ping everything on your network that starts with {0}*", baseString),
                                                        "Start", () => { RunDaemon(baseString); }, null,
                                                        "Cancel", () => { });
    }

    private static void RunDaemon(string rootIP)
    {
        try
        {
            FindAndPing(rootIP);
            string s = string.Join("\r\n", activeIP.ToArray());
            UI_Popup.CurrentInstance.CreatePopupChoice("The following IP addresses were found:\r\n" + s, "OK!", () => { });
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public static void FindAndPing(string baseIP)
    {
        //string baseIP = "192.168.0.";
        activeIP.Clear();        
        UnityEngine.Debug.Log(string.Format("Pinging 255 destinations of D-class in {0}*", baseIP));

        CreatePingers(255);

        PingOptions po = new PingOptions(ttl, true);
        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        byte[] data = enc.GetBytes("abababababababababababababababab");

        SpinWait wait = new SpinWait();
        int cnt = 1;

        Stopwatch watch = Stopwatch.StartNew();

        foreach (System.Net.NetworkInformation.Ping p in pingers)
        {
            lock (@lock)
            {
                instances += 1;
            }

            p.SendAsync(string.Concat(baseIP, cnt.ToString()), timeOut, data, po);
            cnt += 1;
        }

        while (instances > 0)
        {
            wait.SpinOnce();
        }

        watch.Stop();

        DestroyPingers();

        UnityEngine.Debug.Log(string.Format("Finished in {0}. Found {1} active IP-addresses.", watch.Elapsed.ToString(), result));
        Console.ReadKey();

    }

    public static void Ping_completed(object s, PingCompletedEventArgs e)
    {
        lock (@lock)
        {
            instances -= 1;
        }

        if (e.Reply.Status == IPStatus.Success)
        {
            UnityEngine.Debug.Log(string.Concat("Active IP: ", e.Reply.Address.ToString()));
            result += 1;
            activeIP.Add(e.Reply.Address.ToString());
        }
        else
        {
            //Debug.Log(String.Concat("Non-active IP: ", e.Reply.Address.ToString()))
        }
    }


    private static void CreatePingers(int cnt)
    {
        for (int i = 1; i <= cnt; i++)
        {
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            p.PingCompleted += Ping_completed;
            pingers.Add(p);
        }
    }

    private static void DestroyPingers()
    {
        foreach (System.Net.NetworkInformation.Ping p in pingers)
        {
            p.PingCompleted -= Ping_completed;
            p.Dispose();
        }

        pingers.Clear();

    }
}
