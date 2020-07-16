using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventPopup : MonoBehaviour
{
    [TextArea(3,20)]
    public string Message;
    public string Button1Msg, Button2Msg;
    public Color MsgColor = Color.white;

    public UnityEvent FirstButtonEvent;
    public UnityEvent SecondButtonEvent;

    public void Fire() { UI_Popup.CurrentInstance.CreatePopupChoice(Message, Button1Msg, () => { FirstButtonEvent.Invoke(); }, MsgColor, Button2Msg, () => { SecondButtonEvent.Invoke(); }); }

    public void FireAfterFrames(int numFrames) => StartCoroutine(waitFrameEvent(() => { Fire(); }, numFrames));

    IEnumerator waitFrameEvent(System.Action act, int numFrames)
    {
        for (int i = 0; i < numFrames; ++i)
            yield return new WaitForEndOfFrame();

        act.Invoke();
    }
}
