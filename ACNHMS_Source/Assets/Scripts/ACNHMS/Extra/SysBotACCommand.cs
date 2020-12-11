using NHSE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SysBotACCommand : MonoBehaviour
{
    private char CommandPrefix = '$';

    public void GetCommandFromLoadedItem()
    {
        CommandPrefix = UI_Settings.GetPrefix();
        var item = UI_SearchWindow.LastLoadedSearchWindow.GetAsItem(null);
        if (item == null)
        {
            PopupHelper.CreateError("Please select an item.", 3);
            return;
        }

        var itemAsHex = item.ToBytesClass();
        var asString = BitConverter.ToString(itemAsHex).Replace("-", "");
        asString = flipEndianness(asString);

        var toCopy = $"{CommandPrefix}drop {asString}";
        GUIUtility.systemCopyBuffer = toCopy;
        UI_Popup.CurrentInstance.CreatePopupMessage(1.25f, $"Copied\r\n{toCopy}\r\nto clipboard.", () => { });
    }

    // terrible way to flip endianness, feel free to make this better
    private static string flipEndianness(string uintArray)
    {
        var chars = uintArray.ToCharArray();
        if (chars.Length % 8 != 0)
            throw new Exception("Endian flips require 8-byte data structures.");

        var splitBlocks = sliceArray(chars, 8);
        splitBlocks = splitBlocks.Reverse().ToArray();
        List<char> toRet = new List<char>();
        foreach (var splitArray in splitBlocks)
            toRet.AddRange(fix32BitEndian(splitArray));
        
        return new string(toRet.ToArray());
    }

    private static char[] fix32BitEndian(char[] eigthChars)
    {
        List<char[]> twoByteList = new List<char[]>();
        for (int i = 0; i < eigthChars.Length; i += 2) // filter into 2 char blocks
        {
            char[] toAdd = new char[2] { eigthChars[i], eigthChars[i + 1] };
            twoByteList.Add(toAdd);
        }
        twoByteList.Reverse();
        char[][] twoDArray = twoByteList.ToArray();

        List<char> oneDArray = new List<char>();
        foreach (char[] arrayOfChars in twoDArray)
            foreach (char wowChar in arrayOfChars)
                oneDArray.Add(wowChar);

        return oneDArray.ToArray();
    }

    private static T[][] sliceArray<T>(T[] source, int maxResultElements)
    {
        int numberOfArrays = source.Length / maxResultElements;
        if (maxResultElements * numberOfArrays < source.Length)
            numberOfArrays++;
        T[][] target = new T[numberOfArrays][];
        for (int index = 0; index < numberOfArrays; index++)
        {
            int elementsInThisArray = Math.Min(maxResultElements, source.Length - index * maxResultElements);
            target[index] = new T[elementsInThisArray];
            Array.Copy(source, index * maxResultElements, target[index], 0, elementsInThisArray);
        }
        return target;
    }
}
