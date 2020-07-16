using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PShowOnNoSprites : PBase
{
    const string POPUPKEY = "SHOWSPRKEY";

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString(POPUPKEY, "Y") == "N")
            return;
        if (!SpriteBehaviour.SpritesExist())
            UEP.FireAfterFrames(1);
    }

    public void NeverShowAgain()
    {
        PlayerPrefs.SetString(POPUPKEY, "N");
    }
}
