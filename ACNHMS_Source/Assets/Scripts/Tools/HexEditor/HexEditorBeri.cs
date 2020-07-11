using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HexEditorBeri : MonoBehaviour
{
    public float LinePadAmount = 5f;
    public HexLine HexLineTemplate;
    public GameObject ViewPortContent;
    public Button ButtonCompleter;
    public VerticalLayoutGroup VLG;

    public int BytesPerLine { get => HexLineTemplate.bytes.Count; }

    private Action<byte[]> onEditEndCall;

    private List<HexLine> spawnedHexLines;

    public void Start()
    {
    }

    public void InitialiseWithBytes(byte[] initialBytes, string completedButtonLabel, Action<byte[]> onEnd)
    {
        gameObject.SetActive(true);
        VLG.enabled = true;
        ContentSizeFitter csf = VLG.GetComponent<ContentSizeFitter>();
        if (csf != null)
            csf.enabled = true;

        onEditEndCall = onEnd;
        ButtonCompleter.onClick.RemoveAllListeners();
        ButtonCompleter.onClick.AddListener(delegate { gameObject.SetActive(false); onEditEndCall(buildBytes()); });
        ButtonCompleter.GetComponentInChildren<Text>().text = completedButtonLabel;

        if (spawnedHexLines != null)
            foreach (var line in spawnedHexLines)
                Destroy(line.gameObject);

        spawnedHexLines = new List<HexLine>();
        HexLineTemplate.gameObject.SetActive(false);
        int lineBytes = BytesPerLine;
        for (int i = 0; i < initialBytes.Length; i += lineBytes)
        {
            HexLine ins = Instantiate(HexLineTemplate.gameObject).GetComponent<HexLine>();
            ins.transform.parent = HexLineTemplate.transform.parent;
            ins.transform.localScale = HexLineTemplate.transform.localScale;
            ins.gameObject.SetActive(true);
            int lineBytesToTake = Math.Min(lineBytes, initialBytes.Length - i);
            ins.InitialiseWithBytes(initialBytes.Skip(i).Take(lineBytesToTake).ToArray(), (uint)i);

            spawnedHexLines.Add(ins);
        }

        StopAllCoroutines();
        StartCoroutine(waitForVerticalLayout());
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

    private byte[] buildBytes()
    {
        List<byte> toReturn = new List<byte>();
        foreach (HexLine hl in spawnedHexLines)
            toReturn.AddRange(hl.UpdatedBytes);
        return toReturn.ToArray();
    }
}
