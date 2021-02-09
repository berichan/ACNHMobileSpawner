using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OffsetHelper
{
    // some helpers
    public const ulong PlayerSize = 0x10E3A8;
    public const ulong PlayerOtherStartPadding = 0x36A50;

    // player other 
    public const ulong InventoryOffset = 0xACDAD530; // player 0 (A)
    private const ulong playerOtherStart = InventoryOffset - 0x10; // helps to get other values, unused 

    public const ulong WalletAddress = InventoryOffset + 0xB8;
    public const ulong MilesAddress = InventoryOffset - 0x25590;
    public const ulong BankAddress = InventoryOffset + 0x224CC;

    // main player offsets functions
    private static ulong getPlayerStart(ulong invOffset) => invOffset - 0x10 - PlayerOtherStartPadding + 0x110;
    public static ulong getPlayerIdAddress(ulong invOffset) => getPlayerStart(invOffset) + 0xAFA8;
    public static ulong getPlayerProfileMainAddress(ulong invOffset) => getPlayerStart(invOffset) + 0x116A0;
    public static ulong getManpu(ulong invOffset) => invOffset - 0x10 + 0xAF7C + 72;

    // main save offsets
    public const ulong TurnipAddress = 0xABE151EC;
    public const ulong VillagerAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x10;
    public const ulong VillagerHouseAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x417634;
    public const ulong BackupSaveDiff = 0x849C50;

    public const ulong FieldItemStart = VillagerAddress - 0x10 + 0x20ac08;
    public const ulong LandMakingMapStart = FieldItemStart + 0xAAA00;
    public const ulong OutsideFieldStart = FieldItemStart + 0xCF998;
    public const ulong MainFieldStructurStart = FieldItemStart + 0xCF600;

    // other addresses
    public const ulong ArriverNameLocAddress = 0xB66F4EE0;

    public const ulong TextSpeedAddress = 0xBA21BB8;

    public const ulong DodoAddress = 0xA97E15C;
    public const ulong OnlineSessionAddress = 0x91FD740;
    // pointers
    public static readonly long[] PlayerCoordJumps = new long[5] { 0x396F5A0L, 0x18L, 0x178L, 0xD0L, 0xDAL };
}
