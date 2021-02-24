using System;
using System.Linq;
using UnityEngine;

// From SysBot.ACNHOrders: https://github.com/berichan/SysBot.ACNHOrders
[Serializable]
public class PosRotAnchor
{
    public const int SIZE = 16;

    [SerializeField]
    public string Name = string.Empty;
    [SerializeField]
    public byte[] AnchorBytes { get; private set; } = new byte[SIZE];

    public byte[] AssignableBytes
    {
        set
        {
            if (value.Length < SIZE)
                throw new Exception("Setting an anchor of the incorrect size.");
            AnchorBytes = value;
        }
    }

    public byte[] Anchor1
    {
        get { return AnchorBytes.Take(12).ToArray(); }
        set { Array.Copy(value, AnchorBytes, 12); }
    }

    public byte[] Anchor2
    {
        get { return AnchorBytes.Skip(12).ToArray(); }
        set { Array.Copy(value, 0, AnchorBytes, 12, 4); }
    }

    public PosRotAnchor() { }

    public PosRotAnchor(byte[] anchorData, string name)
    {
        if (anchorData.Length < SIZE)
            throw new Exception("Setting an anchor of the incorrect size.");

        AnchorBytes = anchorData;
        Name = name;
    }

    public bool IsEmpty()
    {
        foreach (var b in AnchorBytes)
            if (b != 0)
                return false;
        return true;
    }
}
