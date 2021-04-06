using NHSE.Injection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum PressType : byte
{
    Joystick,
    ButtonPress,
    ButtonRelease
}

public class SysBotExecution
{
    public PressType PType { get; set; }
    public SwitchButton ButtonToPress { get; set; }
    public SwitchStick StickToMove { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
}

public class SwitchUIController : MonoBehaviour
{
    public IUI_Additional Connection;
    private ConcurrentQueue<SysBotExecution> ExecutionQueue = new ConcurrentQueue<SysBotExecution>();

    // Start is called before the first frame update
    void Start()
    {
        bool network = UI_Settings.GetInjectionProtocol() == InjectionProtocol.Sysbot;
        Task.Run(async () => { await DoSocketQueue(network, CancellationToken.None); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Detach()
    {
        Connection.CurrentConnection.SendBytes(SwitchCommand.DetachController());
    }

    public void Press(SwitchButton b, bool up)
    {
        ExecutionQueue.Enqueue(new SysBotExecution()
        {
            PType = up ? PressType.ButtonRelease : PressType.ButtonPress,
            ButtonToPress = b
        });
    }

    public void Click(SwitchButton b)
    {

    }

    public void SetStick(SwitchStick s, short x, short y)
    {
        Debug.Log($"StickState: {x}, {y}");
        ExecutionQueue.Enqueue(new SysBotExecution()
        {
            PType = PressType.Joystick,
            StickToMove = s,
            X = x,
            Y = y
        });
    }

    public async Task DoSocketQueue(bool isNetwork, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (ExecutionQueue.TryDequeue(out var item))
            {
                byte[] command = Array.Empty<byte>();
                switch (item.PType)
                {
                    case PressType.ButtonPress: command = SwitchCommand.Hold(item.ButtonToPress, isNetwork); break;
                    case PressType.ButtonRelease: command = SwitchCommand.Release(item.ButtonToPress, isNetwork); break;
                    case PressType.Joystick: command = SwitchCommand.SetStick(item.StickToMove, item.X, item.Y); break;
                }
                try { Connection.CurrentConnection.SendBytes(command); } catch { }
            }

            await Task.Delay(1, token).ConfigureAwait(false);
        }
    }
}
