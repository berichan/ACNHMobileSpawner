using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader 
{
    public static Texture2D GetLeafImage() => (Texture2D)Resources.Load("Images/leaf");
    public static Texture2D GetTreeImage() => (Texture2D)Resources.Load("Images/AEC_waifu2x");
    public static Texture2D GetExclaimImage() => (Texture2D)Resources.Load("Images/exclaim_fullsize");
    public static TextAsset GetInternalHexes() => (TextAsset)Resources.Load("Lists/InternalHexList");
    public static Texture2D GetAngryIsabelle() => (Texture2D)Resources.Load("Images/angryisabellehead");
}
