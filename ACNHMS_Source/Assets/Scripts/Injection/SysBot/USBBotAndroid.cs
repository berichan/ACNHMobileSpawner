using System;
using System.Collections.Generic;
using System.Threading;


namespace NHSE.Injection
{
    public class USBBotAndroid : IRAMReadWriter
    {
        public bool Connected { get; private set; }
        private readonly object _sync = new object();
        public int MaximumTransferSize { get { return 468; } }

        public bool Connect()
        {
            lock (_sync)
            {
                Connected = AndroidUSBUtils.CurrentInstance.ConnectUSB();
                return Connected;
            }
        }

        public byte[] ReadBytes(ulong offset, int length, RWMethod method = RWMethod.Heap)
        {
            if (length > MaximumTransferSize)
                return ReadBytesLarge(offset, length, method);
            lock (_sync)
            {
                byte[] cmd = SwitchCommandMethodHelper.GetPeekCommand(offset, length, method, true);
                AndroidUSBUtils.CurrentInstance.WriteToEndpoint(cmd);

                // give it time to push data back
                Thread.Sleep((length / 256) + UI_Settings.GetThreadSleepTime());

                byte[] buffer = AndroidUSBUtils.CurrentInstance.ReadEndpoint(length);
                return buffer;
            }
        }

        public void WriteBytes(byte[] data, ulong offset, RWMethod method = RWMethod.Heap)
        {
            if (data.Length > MaximumTransferSize)
                WriteBytesLarge(data, offset, method);
            else
                lock (_sync)
                {
                    AndroidUSBUtils.CurrentInstance.WriteToEndpoint(SwitchCommandMethodHelper.GetPokeCommand(offset, data, method, true));

                    // give it time to push data back
                    Thread.Sleep((data.Length / 256) + UI_Settings.GetThreadSleepTime());
                }
        }

        public byte[] GetVersion()
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.Version();
                AndroidUSBUtils.CurrentInstance.WriteToEndpoint(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
                var buffer = AndroidUSBUtils.CurrentInstance.ReadEndpoint(9);
                return Decoder.ConvertHexByteStringToBytes(buffer);
            }
        }

        private void WriteBytesLarge(byte[] data, ulong offset, RWMethod method)
        {
            int byteCount = data.Length;
            for (int i = 0; i < byteCount; i += MaximumTransferSize)
                WriteBytes(SubArray(data, i, MaximumTransferSize), offset + (uint)i, method);
        }

        private byte[] ReadBytesLarge(ulong offset, int length, RWMethod method)
        {
            List<byte> read = new List<byte>();
            for (int i = 0; i < length; i += MaximumTransferSize)
                read.AddRange(ReadBytes(offset + (uint)i, Math.Min(MaximumTransferSize, length - i), method));
            return read.ToArray();
        }

        private static T[] SubArray<T>(T[] data, int index, int length)
        {
            if (index + length > data.Length)
                length = data.Length - index;
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public void FreezeBytes(byte[] data, uint offset)
        {
            throw new NotImplementedException();
        }

        public void UnFreezeBytes(uint offset)
        {
            throw new NotImplementedException();
        }

        public byte GetFreezeCount()
        {
            throw new NotImplementedException();
        }

        public void UnfreezeAll()
        {
            throw new NotImplementedException();
        }

        public ulong FollowMainPointer(long[] jumps)
        {
            throw new NotImplementedException();
        }
    }
}
