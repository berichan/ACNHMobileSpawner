using System.IO;
using UnityEngine;

public class IconSpriteHelper : StaticSpriteHelperBase
{
    public override string menumapid { get => "2"; } // change when there's a new one
    public override string resourceroot { get => "SpriteLoading/UnitIcons"; }
    public override string imgdumpin { get => "imagedump_manual"; }
    public override string imgheaderin { get => "imagedump_manualheader"; }
    public override string imgpointerin { get => "BeriPointer_unit"; }
    public override ushort defaultitemimg { get => 2; }

    public override string fileroot { get { return Application.persistentDataPath + Path.DirectorySeparatorChar + "UnitItem"; } }

    private static IconSpriteHelper cInstance;

    public new static StaticSpriteHelperBase CurrentInstance
    {
        get
        {
            if (cInstance == null)
                cInstance = new IconSpriteHelper();
            return cInstance;
        }
    }
}
