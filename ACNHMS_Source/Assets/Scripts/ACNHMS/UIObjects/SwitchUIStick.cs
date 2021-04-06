using UnityEngine;
using NHSE.Injection;
using System;

public class SwitchUIStick : MonoBehaviour
{
    private readonly Vector2[] ControllerFlips = new Vector2[4]
    {
        new Vector2(-1, 1),
        new Vector2(1, 1),
        new Vector2(1, -1),
        new Vector2(-1, -1)
    };

    public Joystick Stick;
    public SwitchUIController Controller;
    public SwitchStick WhichStick;

    public int SendMilliseconds;

    private DateTime LastUpdateTime;

    private Vector2 CurrentFlip = new Vector2(-1, 1);
    private int CurrentAxis = 0;

    void Start()
    {
        Stick.OnUpdateStick += UpdateStick;
        Stick.OnInteractStick += OnInteract;

        LastUpdateTime = DateTime.Now;
    }

    public void RotateAxis90()
    {
        CurrentAxis = mod(CurrentAxis + 1, ControllerFlips.Length);
        CurrentFlip = ControllerFlips[CurrentAxis];
    }

    private void UpdateStick(Vector2 direction)
    {
        var now = DateTime.Now;
        if (Math.Abs((now - LastUpdateTime).TotalMilliseconds) > SendMilliseconds)
            LastUpdateTime = now;
        else
            return;

        switch (CurrentAxis)
        {
            case 0: Controller.SetStick(WhichStick, (short)(-direction.y * short.MaxValue), (short)(direction.x * short.MaxValue)); break;
            case 1: Controller.SetStick(WhichStick, (short)(direction.x * short.MaxValue), (short)(direction.y * short.MaxValue)); break;
            case 2: Controller.SetStick(WhichStick, (short)(direction.y * short.MaxValue), (short)(-direction.x * short.MaxValue)); break;
            case 3: Controller.SetStick(WhichStick, (short)(direction.x * short.MaxValue), (short)(-direction.y * short.MaxValue)); break;
        }
    }

    private void OnInteract(bool interact)
    {
        if (!interact)
            Controller.SetStick(WhichStick, 0, 0);
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
