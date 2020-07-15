using UnityEngine;
using NH_CreationEngine;

public interface ISpriteHelper
{
    string menumapid { get; } // change when there's a new one
    string resourceroot { get; }
    string imgdumpin { get; }
    string imgheaderin { get; }
    string imgpointerin { get; }
    ushort defaultitemimg { get; }

    string fileroot { get; }

    SpriteParser sp { get; }
    bool inited { get; }

    SpriteParser GetCurrentParser();
    void Initialize();
    Texture2D GetIconTexture(string itemId);
    Texture2D GetIconTexture(ushort itemId, string format);
}