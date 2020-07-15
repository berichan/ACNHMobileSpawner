using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiImage : MonoBehaviour
{
    public RawImage[] Images;
    public Text[] Texts;
    public MaskableGraphic[] Additionals;

    public void SetRawImageArraySkipNulls(Texture2D[] toAssign)
    {
        for (int i = 0; i < toAssign.Length; ++i)
        {
            if (toAssign[i] == null)
                continue;

            Images[i].texture = toAssign[i];
        }
    }

    public void SetRawImageAll(Texture2D toAssign)
    {
        foreach (var img in Images)
            img.texture = toAssign;
    }
}
