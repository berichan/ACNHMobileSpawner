using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class ItemIconLine : MonoBehaviour
{
    public Button[] MenuIconButtons;

    public List<string> Items;

    public void InitFor(string[] items, Action<string> selectedItemAction, StaticSpriteHelperBase spriteHelper)
    {
        Items = new List<string>(items);

        foreach (var b in MenuIconButtons)
            b.gameObject.SetActive(false);
        for (int i = 0; i < Items.Count; ++i) 
        {
            int tmpVar = i;
            var firstInstance = spriteHelper.GetCurrentParser().SpritePointerTable.FirstOrDefault(x => x.Value == Items[i]);
            if (firstInstance.Key == null)
            {
                MenuIconButtons[i].interactable = false;
                MenuIconButtons[i].GetComponentInChildren<RawImage>().color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            }
            Texture2D t2d = spriteHelper.GetIconTexture(firstInstance.Key);
            MenuIconButtons[i].gameObject.SetActive(true);
            MenuIconButtons[i].GetComponentInChildren<RawImage>().texture = t2d;
            MenuIconButtons[i].onClick.RemoveAllListeners();
            MenuIconButtons[i].onClick.AddListener(delegate { selectedItemAction(Items[tmpVar]); });
        }
    }

    public Vector2 Select(string itemIdIfWeHaveIt)
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
