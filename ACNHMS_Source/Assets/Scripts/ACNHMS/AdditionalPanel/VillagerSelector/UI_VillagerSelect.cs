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
    public InputField SearchField;
    public GameObject NothingFoundText;

    private List<UI_VillagerSelectionButton> spawnedButtons;
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

        SearchField.onValueChanged.AddListener(delegate { updateShownButtons(SearchField.text); });

        if (initialized)
            return;

        var villagerNameLang = new Dictionary<string, string>(GameInfo.Strings.VillagerMap).OrderBy(x => x.Value);
        var villagerPhraseMap = GameInfo.Strings.VillagerPhrase;
        spawnedButtons = new List<UI_VillagerSelectionButton>();
        foreach (var villager in villagerNameLang)
        {
            if (!villagerPhraseMap.ContainsKey(villager.Key)) // make sure this is a villager we can get and not a special npc
                continue;

            UI_VillagerSelectionButton ins = Instantiate(ButtonPrefab.gameObject).GetComponent<UI_VillagerSelectionButton>();
            ins.transform.parent = ButtonPrefab.transform.parent;
            ins.transform.localScale = ButtonPrefab.transform.localScale;
            ins.InitialiseFor(villager.Key, this, villagerParser);
            ins.gameObject.SetActive(true);
            spawnedButtons.Add(ins);
        }
        
        NothingFoundText.SetActive(false);
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

    private void updateShownButtons(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            onlyShowButtons(spawnedButtons);
            return;
        }

        List<UI_VillagerSelectionButton> toShowButtons;
        searchTerm = searchTerm.ToLower();

        if (UI_Settings.GetSearchMode() == StringSearchMode.StartsWith)
            toShowButtons = spawnedButtons.Where(x => x.VillagerName.text.ToLower().StartsWith(searchTerm)).ToList();
        else
            toShowButtons = spawnedButtons.Where(x => x.VillagerName.text.ToLower().Contains(searchTerm)).ToList();

        onlyShowButtons(toShowButtons);
    }

    private void onlyShowButtons(List<UI_VillagerSelectionButton> toShow)
    {
        foreach (var button in spawnedButtons)
            button.gameObject.SetActive(toShow.Contains(button));

        NothingFoundText.SetActive(toShow.Count == 0);
    }
}
