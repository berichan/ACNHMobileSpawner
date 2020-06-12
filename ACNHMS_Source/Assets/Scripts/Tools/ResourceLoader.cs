using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader 
{
    public static Texture2D GetLeafImage()
    {
        return (Texture2D)Resources.Load("Images/leaf");
    }
}
