using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NH_CreationEngine;
using System;
using System.Linq;

public class ItemIconSelector : MonoBehaviour
{
    public ItemIconLine IconLineTemplate;
    public int ItemLineCount { get => IconLineTemplate.MenuIconButtons.Length; }

    public VerticalLayoutGroup VLG;

    private SpriteParser currentFilteredParser;
    private Action<ushort> onValueChanged;
    private List<ItemIconLine> spawnedIcons;
    private bool inited = false;

    public void Initialize(SpriteParser iconSpriteParser, Action<ushort> onValueChanged)
    {
        currentFilteredParser = iconSpriteParser;
        var pointer = currentFilteredParser.SpritePointerHeader;

        if (spawnedIcons != null)
            foreach (var icon in spawnedIcons)
                Destroy(icon.gameObject);

        spawnedIcons = new List<ItemIconLine>();
        IconLineTemplate.gameObject.SetActive(false);
        int iLineCount = ItemLineCount;
        for (int i = 0; i < pointer.Count; i += iLineCount)
        {
            ItemIconLine ins = Instantiate(IconLineTemplate.gameObject).GetComponent<ItemIconLine>();
            ins.transform.parent = IconLineTemplate.transform.parent;
            ins.transform.localScale = IconLineTemplate.transform.localScale;
            ins.gameObject.SetActive(true);

            int valsToTake = Math.Min(iLineCount, pointer.Count - i);
            ins.InitFor(stringToUshortArray(pointer.Keys.Skip(i).Take(valsToTake)), onValueChanged);
            spawnedIcons.Add(ins);
        }

        inited = true;

        StopAllCoroutines();
        StartCoroutine(waitForVerticalLayout());
    }

    public void SelectItemGlobal(ushort itemId)
    {
        var point = currentFilteredParser.SpritePointerTable.FirstOrDefault((x) => ushort.Parse(x.Key, System.Globalization.NumberStyles.HexNumber) == itemId);
        if (point.Value == null)
            return;
        ushort toSelect = ushort.Parse(point.Value, System.Globalization.NumberStyles.HexNumber);

        foreach (var spawned in spawnedIcons)
        {
            if (spawned.Items.Contains(toSelect))
            {
                var pos = spawned.Select(toSelect);
                var posContent = IconLineTemplate.transform.parent.GetComponent<RectTransform>().anchoredPosition;
                posContent.y = -pos.y - (spawned.MenuIconButtons[0].GetComponent<RectTransform>().sizeDelta.y);
                IconLineTemplate.transform.parent.GetComponent<RectTransform>().anchoredPosition = posContent;
                break;
            }
        }
    }

    IEnumerator waitForVerticalLayout()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // need 2 frames incase we updated first
        VLG.enabled = false;

        ContentSizeFitter csf = VLG.GetComponent<ContentSizeFitter>();
        if (csf != null)
            csf.enabled = false;
    }

    private ushort[] stringToUshortArray(IEnumerable<string> strings, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.HexNumber)
    {
        List<ushort> toRet = new List<ushort>();
        foreach (var s in strings)
            toRet.Add(ushort.Parse(s, ns));

        return toRet.ToArray();
    }
}
