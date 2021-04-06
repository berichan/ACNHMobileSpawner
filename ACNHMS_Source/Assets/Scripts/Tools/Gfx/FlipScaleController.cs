using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlipScaleController : MonoBehaviour
{
    private List<FlipScale> ChildFlips;
    private int LastFlip = 0;

    public UnityEvent OnFlipForward;

    // Start is called before the first frame update
    void Start()
    {
        ChildFlips = new List<FlipScale>(GetComponentsInChildren<FlipScale>(true));
    }

    public void IncrementAllFlips()
    {
        LastFlip++;
        foreach (var flip in ChildFlips)
            flip.SetFlip(LastFlip);

        if (OnFlipForward != null)
            OnFlipForward.Invoke();
    }
}
