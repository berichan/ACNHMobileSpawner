using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class UI_Teleport : IUI_Additional
{
    private string TeleportPath => Path.Combine(Application.persistentDataPath, "teleports.bin");

    public UI_TeleportButton TeleportButtonPrefab;
    public GameObject NoTeleports;

    public InputField NewAnchorName;

    private List<PosRotAnchor> teleports = new List<PosRotAnchor>();
    private List<UI_TeleportButton> spawnedButtons = new List<UI_TeleportButton>();

    private bool beriBase = false;

    // Start is called before the first frame update
    void Start()
    {
        loadAnchors();
        var ver = getVersion().TrimEnd('\0').TrimEnd('\n');
        var verLower = ver.ToLower();
        beriBase = verLower.EndsWith("beri");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendAnchorToGame(PosRotAnchor pra)
    {
        var offset = PointerSolver.FollowMainPointer(CurrentConnection, OffsetHelper.PlayerCoordJumps, beriBase);
        CurrentConnection.WriteBytes(pra.Anchor1, offset, NHSE.Injection.RWMethod.Absolute);
        CurrentConnection.WriteBytes(pra.Anchor2, offset + 0x3A, NHSE.Injection.RWMethod.Absolute);
    }

    public void LoadAnchorFromGameAndSave()
    {
        var currPos = GetCurrentPlayerPositionalData(string.Empty);
        var townName = UI_Player.GetFirstPlayerTownName(CurrentConnection);
        currPos.Name = $"{(NewAnchorName.text == string.Empty ? $"Unnamed {DateTime.Now:yyyyMMdd_HHmmss}" : NewAnchorName.text)} ({townName})";

        NewAnchorName.text = string.Empty;
        teleports.Add(currPos); saveAnchors(); loadAnchors();
    }

    public PosRotAnchor GetCurrentPlayerPositionalData(string name)
    {
        var offset = PointerSolver.FollowMainPointer(CurrentConnection, OffsetHelper.PlayerCoordJumps, beriBase);
        var bytesA = CurrentConnection.ReadBytes(offset, 0xA, NHSE.Injection.RWMethod.Absolute);
        var bytesB = CurrentConnection.ReadBytes(offset + 0x3A, 0x4, NHSE.Injection.RWMethod.Absolute);
        var sequentinalAnchor = bytesA.Concat(bytesB).ToArray();
        return new PosRotAnchor(sequentinalAnchor, name);
    }

    public void DeleteAnchor(PosRotAnchor pra)
    {
        UI_Popup.CurrentInstance.CreatePopupChoice($"Really delete anchor {pra.Name}? This action cannot be reversed.", "No", () => { }, null,
                                                    "Yes, delete it", () => { teleports.Remove(pra); saveAnchors(); loadAnchors(); });
    }

    void updateGUI()
    {
        foreach (var spawned in spawnedButtons)
            Destroy(spawned.gameObject);
        spawnedButtons.Clear();

        TeleportButtonPrefab.gameObject.SetActive(false);

        if (teleports.Count < 1)
        {
            NoTeleports.gameObject.SetActive(true);
            return;
        }
        NoTeleports.gameObject.SetActive(false);

        for (int i = 0; i < teleports.Count; ++i)
        {
            var tb = Instantiate(TeleportButtonPrefab.gameObject).GetComponent<UI_TeleportButton>();
            tb.transform.SetParent(TeleportButtonPrefab.transform.parent, false);
            tb.transform.localScale = TeleportButtonPrefab.transform.localScale;

            tb.Parent = this;
            tb.AssignedAnchor = teleports[i];
            tb.TeleportName.text = teleports[i].Name;
            tb.gameObject.SetActive(true);
            spawnedButtons.Add(tb);
        }
    }

    private void loadAnchors()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(TeleportPath, FileMode.OpenOrCreate);
        var anchors = file.Length > 0 ? (PosRotAnchor[])bf.Deserialize(file) : new PosRotAnchor[0];
        file.Close();

        if (anchors != null)
            teleports = new List<PosRotAnchor>(anchors);
        
        updateGUI();
    }

    private void saveAnchors()
    {
        if (teleports.Count < 1)
            return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(TeleportPath, FileMode.OpenOrCreate);
        bf.Serialize(file, teleports.ToArray());
        file.Close();
    }

    private string getVersion() => System.Text.Encoding.UTF8.GetString(CurrentConnection.GetVersion());
}
