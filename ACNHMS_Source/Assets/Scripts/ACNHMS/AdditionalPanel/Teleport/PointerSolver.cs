using System;
using System.Linq;
using NHSE.Injection;

public static class PointerSolver
{
    public static ulong FollowMainPointer(IRAMReadWriter connection, long[] jumps, bool isBeriBase)
    {
        // beri sys-botbase can solve entire pointer 
        if (isBeriBase)
            return (ulong)((long)connection.FollowMainPointer(jumps.Take(jumps.Length - 1).ToArray()) + jumps[jumps.Length - 1]);

        // solve pointer manually
        var ofs = (ulong)jumps[0]; // won't work with negative first jump
        var address = BitConverter.ToUInt64(connection.ReadBytes(ofs, 8, RWMethod.Main), 0);
        for (int i = 1; i < jumps.Length-1; ++i)
        {
            var jump = jumps[i];
            if (jump > 0)
                address += (ulong)jump;
            else
                address -= (ulong)Math.Abs(jump);

            byte[] bytes = connection.ReadBytes(address, 0x8, RWMethod.Absolute);
            address = BitConverter.ToUInt64(bytes, 0);
        }
        return address + (ulong)jumps[jumps.Length - 1];
    }
}
