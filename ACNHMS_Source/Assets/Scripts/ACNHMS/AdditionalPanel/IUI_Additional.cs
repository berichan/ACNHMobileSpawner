using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Injection;

// needs to be a monobehaviour but we want to use this like an interface for all intents and purposes
public class IUI_Additional : MonoBehaviour
{
    public string PanelName = "FUNCTIONALITYNAME";

    public bool RequiresActiveConnection = false;

    [HideInInspector]
    public IRAMReadWriter CurrentConnection;
}
