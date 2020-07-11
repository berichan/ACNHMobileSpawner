using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class ByteInputField : MonoBehaviour
{
    public InputField InputBytes; // pre-assign to make this faster

    public byte Parsed { get => byte.Parse(InputBytes.text, System.Globalization.NumberStyles.HexNumber); }
    public char ParsedAsChar { get => (char)Parsed; }
    
}
