using NHSE.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Joypad : IUI_Additional
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScreen(bool on)
    {
        CurrentConnection.SendBytes(SwitchCommand.SetScreen(on));
    }
}
