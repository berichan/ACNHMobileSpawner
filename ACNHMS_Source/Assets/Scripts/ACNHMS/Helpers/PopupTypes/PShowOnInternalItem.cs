using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PShowOnInternalItem : PBase
{
    const string POPUPKEY = "SHOWINTERNALKEY";

    // Start is called before the first frame update
    public void Start()
    {
        if (PlayerPrefs.GetString(POPUPKEY, "Y") == "N")
            return;

            UEP.FireAfterFrames(1);
    }

    public void NeverShowAgain()
    {
        PlayerPrefs.SetString(POPUPKEY, "N");
    }
}
