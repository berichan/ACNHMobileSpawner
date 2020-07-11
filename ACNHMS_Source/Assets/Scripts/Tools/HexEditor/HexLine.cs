using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class HexLine : MonoBehaviour
{
    public Text Offset, Viewable;
    public List<ByteInputField> bytes;

    private byte[] initialBytes;
    private byte[] updatedBytes;

    public byte[] UpdatedBytes { get => updatedBytes; }

    private int initialisedWithCount = 0;

    public void InitialiseWithBytes(byte[] bytesToWrite, uint offset)
    {
        if (bytesToWrite.Length > bytes.Count)
            throw new System.Exception("Attempted to write more bytes than we have fields for.");

        initialisedWithCount = bytesToWrite.Length;

        initialBytes = bytesToWrite;
        updatedBytes = bytesToWrite;

        for (int i = 0; i < bytesToWrite.Length; ++i)
            bytes[i].InputBytes.text = bytesToWrite[i].ToString("X2");

        // Remove the ones we don't want, but for reusability turn on the ones we do
        for (int i = 0; i < bytes.Count; ++i)
            bytes[i].gameObject.SetActive(i < bytesToWrite.Length);

        Offset.text = offset.ToString("X8");
        updateViewString();
    }

    /// <summary>
    /// Called by an update by an InputField in the editor.
    /// </summary>
    /// <param name="bifIndex">Index of the byte input field.</param>
    public void ValueChangedEnd(int bifIndex)
    {
        if (byte.TryParse(bytes[bifIndex].InputBytes.text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var nByte))
            updatedBytes[bifIndex] = nByte;

        // does both, keeps padding but also reverts if the value is incorrect
        bytes[bifIndex].InputBytes.text = updatedBytes[bifIndex].ToString("X2");

        updateViewString();
    }

    private void updateViewString()
    {
        char[] build = new char[updatedBytes.Length];
        for (int i = 0; i < updatedBytes.Length; ++i)
            build[i] = (char.IsWhiteSpace(bytes[i].ParsedAsChar) || char.IsControl(bytes[i].ParsedAsChar)) ? '.' : bytes[i].ParsedAsChar;
        Viewable.text = new string(build);
    }
}
