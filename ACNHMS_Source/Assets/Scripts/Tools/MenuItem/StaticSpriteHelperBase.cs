using System.IO;
using UnityEngine;
using NH_CreationEngine;

public class StaticSpriteHelperBase : ISpriteHelper
{
    public virtual string menumapid { get; } // change when there's a new one
    public virtual string resourceroot { get; }
    public virtual string imgdumpin { get; }
    public virtual string imgheaderin { get; }
    public virtual string imgpointerin { get; }
    public virtual ushort defaultitemimg { get; }

    public virtual string fileroot { get; }

    public SpriteParser sp { get; private set; }
    public bool inited { get; private set; }

    public SpriteParser GetCurrentParser() { Initialize(); return sp; }

    public static StaticSpriteHelperBase CurrentInstance { get; }

    public void Initialize()
    {
        if (inited)
            return;
        if (!Directory.Exists(fileroot))
            Directory.CreateDirectory(fileroot);

        if (!File.Exists(getOutputItemPath(imgdumpin)))
        {
            // clear folder
            Directory.Delete(fileroot, true);
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

    public Texture2D GetIconTexture(string itemId)
    {
        Initialize();
        var tex = sp.GetTexture(itemId, 0);
        if (tex == null)
            tex = sp.GetTexture(2, 0);

        return tex;
    }

    public Texture2D GetIconTexture(ushort itemId, string format = "X") => GetIconTexture(itemId.ToString(format));

    private string getIdedPath(string inPath) => inPath + menumapid;
    private string getOutputItemPath(string inPath) => Path.Combine(fileroot, Path.GetFileName(getIdedPath(inPath)));
}
