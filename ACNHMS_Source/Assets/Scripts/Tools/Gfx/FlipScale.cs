using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipScale : MonoBehaviour
{
    public Transform[] Flips;

    public void SetFlip(int index)
    {
        index = mod(index, Flips.Length);

        var curFlip = Flips[index];
        transform.position = curFlip.position;
        transform.rotation = curFlip.rotation;
        transform.localScale = curFlip.localScale;
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
