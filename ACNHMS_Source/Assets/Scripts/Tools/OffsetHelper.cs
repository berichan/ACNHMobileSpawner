using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OffsetHelper
{
    // some helpers
    public const ulong PlayerSize = 0x6D6D0;
    public const ulong PlayerOtherStartPadding = 0x35E40;

    // player other (Personal Offsets 13)
    public const ulong InventoryOffset = 0xABA526A8; // player 0 (A)
    private const ulong playerOtherStart = InventoryOffset - 0x10; // helps to get other values, unused 

    public const ulong WalletAddress = InventoryOffset + 0xB8;
    public const ulong MilesAddress = InventoryOffset - 0x24980;
    public const ulong BankAddress = InventoryOffset + 0x3451C;

    // main player offsets functions
    private static ulong getPlayerStart(ulong invOffset) => invOffset - 0x10 - PlayerOtherStartPadding + 0x110;
    public static ulong getPlayerIdAddress(ulong invOffset) => getPlayerStart(invOffset) + 0xAFA8;


    // main save offsets
    public const ulong TurnipAddress = 0xAA890CB0;
    public const ulong VillagerAddress = TurnipAddress - 0x411F40;
    public const ulong VillagerHouseAddress = VillagerAddress + 0x40E228;
    public const ulong VillagerHouseBufferDiff = 0xACEDA0;
    //public const ulong HeapVillagerHouse = 0x2A9B94A8;
}
