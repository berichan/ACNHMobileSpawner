using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System;

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

        public byte[] ReadBytes(uint offset, int length)
        {
            if (length > MaximumTransferSize)
                return ReadBytesLarge(offset, length);
            lock (_sync)
            {
                var cmd = SwitchCommand.Peek(offset, length);
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep((length / 256) + UI_Settings.GetThreadSleepTime());
                var buffer = new byte[(length * 2) + 1];
                var _ = ReadInternal(buffer);
                return Decoder.ConvertHexByteStringToBytes(buffer);
            }
        }

        public void WriteBytes(byte[] data, uint offset)
        {
            if (data.Length > MaximumTransferSize)
                WriteBytesLarge(data, offset);
            else
                lock (_sync)
                {
                SendInternal(SwitchCommand.Poke(offset, data));

                // give it time to push data back
                Thread.Sleep((data.Length / 256) + UI_Settings.GetThreadSleepTime());
                }
        }

        public void FreezeBytes(byte[] data, uint offset)
        {
            lock (_sync)
            {
                SendInternal(SwitchCommand.Freeze(offset, data));

                // give it time to push data back
                Thread.Sleep((data.Length / 256) + UI_Settings.GetThreadSleepTime());
            }
        }

        public void UnFreezeBytes(uint offset)
        {
            lock (_sync)
            {
                SendInternal(SwitchCommand.UnFreeze(offset));

                // give it time to push data back
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
    }

    public enum InjectionType
    {
        Generic,
        Pouch,
    }
}
