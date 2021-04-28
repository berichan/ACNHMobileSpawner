using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OffsetHelper
{
    // some helpers
    public const ulong PlayerSize = 0x10E500;
    public const ulong PlayerOtherStartPadding = 0x36A50;

    // player other 
    public const ulong InventoryOffset = 0xAE61F840; // player 0 (A) 
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
    public const ulong TurnipAddress = 0xAD618B84;
    public const ulong VillagerAddress = TurnipAddress - 0x2d40 - 0x43be2c + 0x10;
    public const ulong VillagerHouseAddress = TurnipAddress - 0x2d40 - 0x43be2c + 0x43abe4;
    public const ulong BackupSaveDiff = 0x86D590;

    public const ulong FieldItemStart = VillagerAddress - 0x10 + 0x22e1b8;
    public const ulong LandMakingMapStart = FieldItemStart + 0xAAA00;
    public const ulong OutsideFieldStart = FieldItemStart + 0xCF998;
    public const ulong MainFieldStructurStart = FieldItemStart + 0xCF600;

    // other addresses
    public const ulong ArriverNameLocAddress = 0xB66D6FF8;
    public const ulong ArriverVillageLocAddress = ArriverNameLocAddress - 0x1C;

    public const ulong TextSpeedAddress = 0xBACE3B8; 
    public const ulong ChatBufferSize = 0x1E;

    public const ulong DodoAddress = 0xA98F15C;
    public const ulong OnlineSessionAddress = 0x920E740;
    public const ulong OnlineSessionVisitorAddress = 0x9F974B8;
    public const ulong OnlineSessionVisitorSize = 0x448; // reverse order

    public const ulong TimeAddress = 0x0BAC54E8;

    // pointers
    public static readonly long[] PlayerCoordJumps = new long[5] { 0x3A08B40L, 0x18L, 0x178L, 0xD0L, 0xD8L };
    public static readonly long[] ChatCoordJumps = new long[2] { 0x03CCD158L, 0x40L };

    // exefs (main)
    public const ulong AnimationSpeedOffset = 0x037D0090;
    public const ulong WalkSpeedOffset = 0x01134780;
    public const ulong TimeStateAddress = 0x00278C20;
}
