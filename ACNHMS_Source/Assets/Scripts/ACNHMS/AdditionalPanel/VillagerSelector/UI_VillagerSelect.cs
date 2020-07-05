using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;
using System;
using System.Linq;
using NH_CreationEngine;

public class UI_VillagerSelect : MonoBehaviour
{
    public string LastSelectedVillager = "";

    public UI_VillagerSelectionButton ButtonPrefab;
    public Text SelectedVillagerName;

    public Button VillagerAcceptButton;
    public Text VillagerAcceptText;

    private Action endGood, endBad;
    private SpriteParser villagerParser;
    private bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(Action onAccept, Action onEndCancel, SpriteParser villParser)
    {
        endGood = onAccept;
        endBad = onEndCancel;
        villagerParser = villParser;
        ButtonPrefab.gameObject.SetActive(false);
        SelectedVillagerName.text = "No villager selected";
        LastSelectedVillager = string.Empty;
        gameObject.SetActive(true);

        VillagerAcceptText.text = "Send villager (none selected)";
        VillagerAcceptButton.interactable = false;

        if (initialized)
            return;

        var villagerNameLang = new Dictionary<string, string>(GameInfo.Strings.VillagerMap).OrderBy(x => x.Value);
        var villagerPhraseMap = GameInfo.Strings.VillagerPhrase;
        foreach (var villager in villagerNameLang)
        {
            if (!villagerPhraseMap.ContainsKey(villager.Key)) // make sure this is a villager we can get and not a special npc
                continue;

            UI_VillagerSelectionButton ins = Instantiate(ButtonPrefab.gameObject).GetComponent<UI_VillagerSelectionButton>();
            ins.transform.parent = ButtonPrefab.transform.parent;
            ins.transform.localScale = ButtonPrefab.transform.localScale;
            ins.InitialiseFor(villager.Key, this, villagerParser);
            ins.gameObject.SetActive(true);
        }

        initialized = true;
    }

    public void SelectVillager(string internalName)
    {
        LastSelectedVillager = internalName;
        SelectedVillagerName.text = GameInfo.Strings.GetVillager(internalName);
        VillagerAcceptText.text = string.Format("Send villager ({0})", SelectedVillagerName.text);
        VillagerAcceptButton.interactable = true;
    }

    public void SendBack(bool success)
    {
        if (success)
            endGood();
        else
            endBad();
        gameObject.SetActive(false);
    }
}
