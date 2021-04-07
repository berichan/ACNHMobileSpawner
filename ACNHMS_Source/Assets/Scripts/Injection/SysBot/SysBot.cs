using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NHSE.Injection
{
    public class SysBot : IRAMReadWriter
    {
        public string IP = "192.168.1.65";
        public int Port = 6000;
        public Socket Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);
        public bool Connected { get; private set; }
        public int MaximumTransferSize { get { return 8192; } }

        private readonly object _sync = new object();

        public void Connect(string ip, int port)
        {
            IP = ip;
            Port = port;
            lock (_sync)
            {
                Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);
                Connection.Connect(IP, Port);
                Connected = true;
            }
        }

        public void Disconnect()
        {
            lock (_sync)
            {
                Connection.Disconnect(false);
                Connected = false;
            }
        }

        private int ReadInternal(byte[] buffer)
        {
            int br = Connection.Receive(buffer, 0, 1, SocketFlags.None);
            while (buffer[br - 1] != (byte)'\n')
                br += Connection.Receive(buffer, br, 1, SocketFlags.None);
            return br;
        }

        private int SendInternal(byte[] buffer) => Connection.Send(buffer);

        public int Read(byte[] buffer)
        {
            lock (_sync)
                return ReadInternal(buffer);
        }

        public byte[] ReadBytes(ulong offset, int length, RWMethod method = RWMethod.Heap)
        {
            if (length > MaximumTransferSize)
                return ReadBytesLarge(offset, length, method);
            lock (_sync)
            {
                var cmd = SwitchCommandMethodHelper.GetPeekCommand(offset, length, method, false);
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep((length / 256) + UI_Settings.GetThreadSleepTime());
                var buffer = new byte[(length * 2) + 1];
                var _ = ReadInternal(buffer);
                return Decoder.ConvertHexByteStringToBytes(buffer);
            }
        }

        public void WriteBytes(byte[] data, ulong offset, RWMethod method = RWMethod.Heap)
        {
            if (data.Length > MaximumTransferSize)
                WriteBytesLarge(data, offset, method);
            else
                lock (_sync)
                {
                    SendInternal(SwitchCommandMethodHelper.GetPokeCommand(offset, data, method, false));

                    // give it time to push data back
                    Thread.Sleep((data.Length / 256) + UI_Settings.GetThreadSleepTime());
                }
        }

        public void SendBytes(byte[] encodeData)
        {
            lock (_sync)
            {
                SendInternal(encodeData);
            }
        }

        public byte[] GetVersion()
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.Version();
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
                var buffer = new byte[9];
                var _ = ReadInternal(buffer);
                return buffer;
            }
        }

        public byte[] GetBattery()
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.BatteryCharge();
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
                var buffer = new byte[5];
                var _ = ReadInternal(buffer);
                return buffer;
            }
        }

        public ulong FollowMainPointer(long[] jumps)
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.FollowMainPointer(jumps);
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
                var buffer = new byte[17];
                var _ = ReadInternal(buffer);
                var bytes = Decoder.ConvertHexByteStringToBytes(buffer);
                bytes = bytes.Reverse().ToArray();
                return BitConverter.ToUInt64(bytes, 0);
            }
        }

        public byte[] PeekMainPointer(long[] jumps, int length)
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.PeekMainPointer(jumps, length);
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep((length / 256) + UI_Settings.GetThreadSleepTime());
                var buffer = new byte[(length * 2) + 1];
                var _ = ReadInternal(buffer);
                return Decoder.ConvertHexByteStringToBytes(buffer);
            }
        }

        public void FreezeBytes(byte[] data, uint offset)
        {
            lock (_sync)
            {
                SendInternal(SwitchCommand.Freeze(offset, data));

                // wait for it to create freezers
                Thread.Sleep((data.Length / 256) + UI_Settings.GetThreadSleepTime());
            }
        }

        public void UnFreezeBytes(uint offset)
        {
            lock (_sync)
            {
                SendInternal(SwitchCommand.UnFreeze(offset));

                // wait for freezes to clear and poll again
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
            }
        }

        public byte GetFreezeCount()
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.FreezeCount();
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
                var buffer = new byte[3];
                var _ = ReadInternal(buffer);
                return Decoder.ConvertHexByteStringToBytes(buffer)[0];
            }
        }

        public void UnfreezeAll()
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.FreezeClear();
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
            }
        }

        public void FreezePause()
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.FreezePause();
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
            }
        }
    
        public void FreezeUnpause()
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.FreezeUnpause();
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
            }
        }

        public void Configure(string name, string value)
        {
            lock (_sync)
            {
                var cmd = SwitchCommand.Configure(name, value);
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep(1 + UI_Settings.GetThreadSleepTime());
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
    }

    public enum InjectionType
    {
        Generic,
        Pouch,
    }
}
