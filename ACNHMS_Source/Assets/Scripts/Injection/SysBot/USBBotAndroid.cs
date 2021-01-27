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

        public byte[] ReadBytes(uint offset, int length)
        {
            if (length > MaximumTransferSize)
                return ReadBytesLarge(offset, length);
            lock (_sync)
            {
                byte[] cmd = SwitchCommand.PeekRaw(offset, length);
                AndroidUSBUtils.CurrentInstance.WriteToEndpoint(cmd);

                // give it time to push data back
                Thread.Sleep((length / 256) + UI_Settings.GetThreadSleepTime());

                byte[] buffer = AndroidUSBUtils.CurrentInstance.ReadEndpoint(length);
                return buffer;
            }
        }

        public void WriteBytes(byte[] data, uint offset)
        {
            if (data.Length > MaximumTransferSize)
                WriteBytesLarge(data, offset);
            else
                lock (_sync)
                {
                    AndroidUSBUtils.CurrentInstance.WriteToEndpoint(SwitchCommand.PokeRaw(offset, data));

                    // give it time to push data back
                    Thread.Sleep((data.Length / 256) + UI_Settings.GetThreadSleepTime());
                }
        }

        private void WriteBytesLarge(byte[] data, uint offset)
        {
            int byteCount = data.Length;
            for (int i = 0; i < byteCount; i += MaximumTransferSize)
                WriteBytes(SubArray(data, i, MaximumTransferSize), offset + (uint)i);
        }

        private byte[] ReadBytesLarge(uint offset, int length)
        {
            List<byte> read = new List<byte>();
            for (int i = 0; i < length; i += MaximumTransferSize)
                read.AddRange(ReadBytes(offset + (uint)i, Math.Min(MaximumTransferSize, length - i)));
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
    }
}
