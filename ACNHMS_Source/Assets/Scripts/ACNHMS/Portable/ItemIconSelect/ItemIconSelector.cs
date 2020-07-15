using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NH_CreationEngine;
using System;
using System.Linq;
using NHSE.Core;

public class ItemIconSelector : MonoBehaviour
{
    public ItemIconLine IconLineTemplate;
    public int ItemLineCount { get => IconLineTemplate.MenuIconButtons.Length; }

    public VerticalLayoutGroup VLG;

    private SpriteParser currentFilteredParser;
    private Action<string> onValueChanged;
    private List<ItemIconLine> spawnedIcons;
    private bool inited = false;

    public void Initialize(SpriteParser iconSpriteParser, Action<string> valueChanged, StaticSpriteHelperBase cSHB)
    {
        currentFilteredParser = iconSpriteParser;
        onValueChanged = valueChanged;

        if (inited)
            return;

        var pointerShuffled = currentFilteredParser.SpritePointerHeader;
        IOrderedEnumerable<KeyValuePair<string, ByteBoundary>> pointerOrdered;
        if (ushort.TryParse(pointerShuffled.ElementAt(0).Key, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out var _))
            pointerOrdered = pointerShuffled.OrderBy(x => ItemInfo.GetItemKind(ushort.Parse(x.Key, System.Globalization.NumberStyles.HexNumber)));
        else
            pointerOrdered = pointerShuffled.OrderBy(x => x.Key);
        var pointer = pointerOrdered.ToDictionary(pair => pair.Key, pair => pair.Value);

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
            ins.InitFor(pointer.Keys.Skip(i).Take(valsToTake).ToArray(), onValueChanged, cSHB);
            spawnedIcons.Add(ins);
        }

        inited = true;

        StopAllCoroutines();
        StartCoroutine(waitForVerticalLayout());
    }

    public void SelectItemGlobal(string itemId)
    {
        var point = currentFilteredParser.SpritePointerTable.FirstOrDefault((x) => x.Key == itemId);
        if (point.Value == null)
            return;
        string toSelect = point.Value;

        foreach (var spawned in spawnedIcons)
        {
            if (spawned.Items.Contains(toSelect))
            {
                var pos = spawned.Select(toSelect);
                var posContent = IconLineTemplate.transform.parent.GetComponent<RectTransform>().anchoredPosition;
                posContent.y = -pos.y - (spawned.MenuIconButtons[0].GetComponent<RectTransform>().sizeDelta.y/2);
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

    /*private string[] ushortTostringArray(IEnumerable<ushort> strings, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.HexNumber)
    {
        List<string> toRet = new List<string>();
        foreach (var s in strings)
            toRet.Add(ushort.Parse(s, ns));

        return toRet.ToArray();
    }*/
}
