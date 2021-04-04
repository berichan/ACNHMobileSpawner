using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = SIZE)]
public class TimeBlock
{
    public const int SIZE = 0x6;

    public ushort Year { get; set; }
    public byte Month { get; set; }
    public byte Day { get; set; }
    public byte Hour { get; set; }
    public byte Minute { get; set; }

}
