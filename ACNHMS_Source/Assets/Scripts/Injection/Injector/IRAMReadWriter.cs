namespace NHSE.Injection
{
    public interface IRAMReadWriter
    {
        int MaximumTransferSize { get; }
        bool Connected { get; }
        byte[] ReadBytes(uint offset, int length);
        void WriteBytes(byte[] data, uint offset);
        byte[] GetVersion();
        ulong FollowMainPointer(int[] jumps);
        void FreezeBytes(byte[] data, uint offset);
        void UnFreezeBytes(uint offset);
        byte GetFreezeCount();
        void UnfreezeAll();
    }
}