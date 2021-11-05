using System.IO;
using UnityEngine;

public class MenuSpriteHelper : StaticSpriteHelperBase
{
    public override string menumapid { get => "C"; } // change when there's a new one
    public override string resourceroot { get => "SpriteLoading"; }
    public override string imgdumpin { get => "imagedump_menu"; }
    public override string imgheaderin { get => "imagedump_menuheader"; }
    public override string imgpointerin { get => "SpritePointer_menu"; }
    public override ushort defaultitemimg { get => 2; }

    public override string fileroot { get { return Application.persistentDataPath + Path.DirectorySeparatorChar + "MenuItem"; } }

    private static MenuSpriteHelper cInstance;

    public new static StaticSpriteHelperBase CurrentInstance { get
        {
            if (cInstance == null)
                cInstance = new MenuSpriteHelper();
            return cInstance;
        } }
}
