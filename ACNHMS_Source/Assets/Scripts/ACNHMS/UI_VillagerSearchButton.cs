using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_VillagerSearchButton : MonoBehaviour
{
    public Text _id, _name, _species;
    public Image _sprite, _genderM, _genderF;
    public string id;
    
    public void SetData(string i, string n, string s, string g, Sprite sp)
    {
        _id.text = i;
        id = i;
        _name.text = n;
        _species.text = s;
        if(sp != null)
            _sprite.sprite = sp;
        _genderM.gameObject.SetActive(g.ToLower() == "male");
        _genderF.gameObject.SetActive(g.ToLower() == "female");
    }

    public void OnClick()
    {
        char pref = UI_Settings.GetPrefix();
        string command = $"{pref}order villager:{id}";
        GUIUtility.systemCopyBuffer = command;
        UI_Popup.CurrentInstance.CreatePopupMessage(2f, $"Copied command to Clipboard:\n{command}", () => { });
    }
}
