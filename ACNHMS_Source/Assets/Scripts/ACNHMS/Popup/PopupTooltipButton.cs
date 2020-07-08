using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupTooltipButton : MonoBehaviour, IPointerClickHandler
{
    [TextArea(3, 20)]
    public string Message;

    public string AcceptLabel = "OK!";

    public void OnPointerClick(PointerEventData eventData)
    {
        UI_Popup.CurrentInstance.CreatePopupChoice(Message, AcceptLabel, () => { });
    }
}
