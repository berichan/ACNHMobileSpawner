using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader 
{
    public static Texture2D GetLeafImage() => (Texture2D)Resources.Load("Images/leaf");
    public static Texture2D GetTreeImage() => (Texture2D)Resources.Load("Images/AEC_waifu2x");
}
