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
    public const ulong InventoryOffset = 0xACDB0530; // player 0 (A)
    private const ulong playerOtherStart = InventoryOffset - 0x10; // helps to get other values, unused 

    public const ulong WalletAddress = InventoryOffset + 0xB8;
    public const ulong MilesAddress = InventoryOffset - 0x25590;
    public const ulong BankAddress = InventoryOffset + 0x224CC;

    // main player offsets functions
    private static ulong getPlayerStart(ulong invOffset) => invOffset - 0x10 - PlayerOtherStartPadding + 0x110;
    public static ulong getPlayerIdAddress(ulong invOffset) => getPlayerStart(invOffset) + 0xAFA8;
    public static ulong getPlayerProfileMainAddress(ulong invOffset) => getPlayerStart(invOffset) + 0x116A0;
    public static ulong getManpu(ulong invOffset) => invOffset - 0x10 + 0xAF7C + 72;
    public static ulong getTownNameAddress(ulong invOffset) => getPlayerIdAddress(invOffset) - 0xB8 + 0x04;

    // main save offsets
    public const ulong TurnipAddress = 0xABE181EC;
    public const ulong VillagerAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x10;
    public const ulong VillagerHouseAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x417634;
    public const ulong BackupSaveDiff = 0x849C50;

    public const ulong FieldItemStart = VillagerAddress - 0x10 + 0x20ac08;
    public const ulong LandMakingMapStart = FieldItemStart + 0xAAA00;
    public const ulong OutsideFieldStart = FieldItemStart + 0xCF998;
    public const ulong MainFieldStructurStart = FieldItemStart + 0xCF600;

    // other addresses
    public const ulong ArriverNameLocAddress = 0xB66F8208; 
    public const ulong ArriverVillageLocAddress = ArriverNameLocAddress - 0x1C;

    public const ulong TextSpeedAddress = 0xBA21BB8;

    public const ulong DodoAddress = 0xA98115C; 
    public const ulong OnlineSessionAddress = 0x9200740; 
    public const ulong OnlineSessionVisitorAddress = 0x9D2FFB0;
    public const ulong OnlineSessionVisitorSize = 0x448; // reverse order

    // pointers
    public static readonly long[] PlayerCoordJumps = new long[5] { 0x398C380L, 0x18L, 0x178L, 0xD0L, 0xD8L };
}
