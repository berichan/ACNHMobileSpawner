using System.IO;
using UnityEngine;
using NH_CreationEngine;

public class MenuItemSpriteHelper
{
    private const string menumapid = "1"; // change when there's a new one
    private const string resourceroot = "SpriteLoading";
    private const string imgdumpin = "imagedump_menu";
    private const string imgheaderin = "imagedump_menuheader";
    private const string imgpointerin = "SpritePointer_menu";
    private const ushort defaultitemimg = 2;

    private static string fileroot { get { return Application.persistentDataPath + Path.DirectorySeparatorChar + "MenuItem"; } }

    private static SpriteParser sp;
    private static bool inited = false;

    public static SpriteParser CurrentParser { get { Initialize(); return sp; } }
    
    public static void Initialize()
    {
        if (inited)
            return;
        if (!Directory.Exists(fileroot))
            Directory.CreateDirectory(fileroot);

        if (!File.Exists(getOutputItemPath(imgdumpin)))
        {
            // clear folder
            Directory.Delete(fileroot);
            Directory.CreateDirectory(fileroot);

            // copy files over
            byte[] imgdump = ((TextAsset)Resources.Load(resourceroot + "/" + imgdumpin)).bytes;
            byte[] imgheader = ((TextAsset)Resources.Load(resourceroot + "/" + imgheaderin)).bytes;
            byte[] imgpointer = ((TextAsset)Resources.Load(resourceroot + "/" + imgpointerin)).bytes;

            File.WriteAllBytes(getOutputItemPath(imgdumpin), imgdump);
            File.WriteAllBytes(getOutputItemPath(imgheaderin), imgheader);
            File.WriteAllBytes(getOutputItemPath(imgpointerin), imgpointer);
        }

        sp = new SpriteParser(getOutputItemPath(imgdumpin), getOutputItemPath(imgheaderin), getOutputItemPath(imgpointerin));
        inited = true;
    }

    public static Texture2D GetIconTexture(ushort itemId)
    {
        Initialize();
        var tex = sp.GetTexture(itemId, 0);
        if (tex == null) tex = sp.GetTexture(2, 0);

        return tex;
    }

    private static string getIdedPath(string inPath) => inPath + menumapid;
    private static string getOutputItemPath(string inPath) => Path.Combine(fileroot, Path.GetFileName(getIdedPath(inPath)));
}
