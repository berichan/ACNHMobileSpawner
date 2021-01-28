using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Freeze : IUI_Additional
{
    private const string SBBUrl = "https://github.com/berichan/sys-botbase/releases";

    public void GoToDownloadPage() => Application.OpenURL(SBBUrl);
}
