using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class ItemIconLine : MonoBehaviour
{
    public Button[] MenuIconButtons;

    public List<ushort> Items;

    public void InitFor(ushort[] items, Action<ushort> selectedItemAction)
    {
        Items = new List<ushort>(items);

        foreach (var b in MenuIconButtons)
            b.gameObject.SetActive(false);
        for (int i = 0; i < Items.Count; ++i) 
        {
            int tmpVar = i;
            Texture2D t2d = MenuItemSpriteHelper.GetIconTexture(Items[i]);
            MenuIconButtons[i].gameObject.SetActive(true);
            MenuIconButtons[i].GetComponentInChildren<RawImage>().texture = t2d;
            MenuIconButtons[i].onClick.RemoveAllListeners();
            MenuIconButtons[i].onClick.AddListener(delegate { selectedItemAction(Items[tmpVar]); });
        }
    }

    public Vector2 Select(ushort itemIdIfWeHaveIt)
    {
        int index = Items.IndexOf(itemIdIfWeHaveIt);
        if (index > 0)
            SelectButton(MenuIconButtons[index]);

        return GetComponent<RectTransform>().anchoredPosition;
    }

    public void SelectButton(Button b)
    {
        //b.Select();
        ForceSelectGameObject(b.gameObject);
    }

    private void ForceSelectGameObject(GameObject gObject)
    {
        ExecuteEvents.Execute(gObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        if (EventSystem.current.currentSelectedGameObject == gObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        EventSystem.current.SetSelectedGameObject(gObject);
    }
}
