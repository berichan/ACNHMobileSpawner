using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NH_CreationEngine;
using NHSE.Core;

public class UI_VillagerSelectionButton : MonoBehaviour
{
    public RawImage ImageToPutGuyOn;
    public Button SelectButton;
    public Text VillagerName;

    string internalName;
    UI_VillagerSelect root;

    public void Start()
    {
        
    }

    public void InitialiseFor(string internalName, UI_VillagerSelect caller, SpriteParser sp)
    {
        this.internalName = internalName;
        root = caller;

        SelectButton.onClick.AddListener(delegate { SelectMe(); });
        VillagerName.text = GameInfo.Strings.GetVillager(internalName);
        ImageToPutGuyOn.texture = SpriteBehaviour.PullTextureFromParser(sp, internalName);
    }

    public void SelectMe()
    {
        root.SelectVillager(internalName);
    }
}
