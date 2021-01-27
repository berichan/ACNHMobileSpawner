using System;
using System.Collections.Generic;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using MonoLibUsb;

namespace NHSE.Injection
{
    public class USBBot : IRAMReadWriter
    {
        private const byte READPOINT = 129;
        private const byte WRITEPOINT = 1;

        private UsbDevice SwDevice;
        private UsbEndpointReader reader;
        private UsbEndpointWriter writer;

        public int MaximumTransferSize { get { return 468; } }

        public bool Connected { get; private set; }

        private readonly object _sync = new object();

        MonoUsbSessionHandle context;

        // there is no connect, we just check if we can open and claim an interface, and if we can then we can "connect"
        public bool Connect()
        {
            lock (_sync)
            {
                if (context != null)
                {
                    if (!context.IsClosed)
                        context.Close();
                    context.Dispose();
                    context = null;
                }

                context = new MonoUsbSessionHandle();
                var usbHandle = MonoUsbApi.OpenDeviceWithVidPid(context, 1406, 12288);
                if (usbHandle != null)
                {
                    if (MonoUsbApi.ClaimInterface(usbHandle, 0) == 0)
                    {
                        MonoUsbApi.ReleaseInterface(usbHandle, 0);
                        usbHandle.Close();
                        Connected = true;
                        return true;
                    }
                    usbHandle.Close();
                }
                else
                    throw new Exception("Console not found or usb driver failed to open device. Is the console connected and is libusb configured correctly?");

                Connected = false;
                UnityEngine.Debug.Log("Failed");
                return false;
            }
        }

        private MonoUsbDeviceHandle getUsableAndOpenUsbHandle()
        {
            lock (_sync)
            {
                if (context != null)
                {
                    if (!context.IsClosed)
                        context.Close();
                    context.Dispose();
                    context = null;
                }

                context = new MonoUsbSessionHandle();
                var usbHandle = MonoUsbApi.OpenDeviceWithVidPid(context, 1406, 12288);
                if (usbHandle != null)
                {
                    if (MonoUsbApi.ClaimInterface(usbHandle, 0) == 0)
                    {
                        return usbHandle;
                    }
                    usbHandle.Close();
                }
                
                UnityEngine.Debug.Log("Failed");
                return null;
            }
        }

        private void CleanUpHandle(MonoUsbDeviceHandle handle)
        {
            MonoUsbApi.ReleaseInterface(handle, 0);
            handle.Close();
            Disconnect(false);
        }

        public void Disconnect(bool setConnectionStatus = true)
        {
            lock (_sync)
            {
                if (context != null)
                {
                    context.Dispose();
                    context = null;
                }
                if (setConnectionStatus)
                    Connected = false;
            }
        }

        private int ReadInternal(byte[] buffer)
        {
            var handle = getUsableAndOpenUsbHandle();
            if (handle == null)
                throw new Exception("USB writer is null, you may have disconnected the device during previous function");

            byte[] sizeOfReturn = new byte[4];

            MonoUsbApi.BulkTransfer(handle, READPOINT, sizeOfReturn, 4, out var _, 5000);

            // read stack
            MonoUsbApi.BulkTransfer(handle, READPOINT, buffer, buffer.Length, out var len, 5000);
            CleanUpHandle(handle);
            return len;
        }

        private int SendInternal(byte[] buffer)
        {
            var handle = getUsableAndOpenUsbHandle();
            if (handle == null)
                throw new Exception("USB writer is null, you may have disconnected the device during previous function");

            uint pack = (uint)buffer.Length + 2;
            byte[] packed = BitConverter.GetBytes(pack);
            var ec = MonoUsbApi.BulkTransfer(handle, WRITEPOINT, packed, packed.Length, out var _, 5000);
            if (ec != 0)
            {
                string err = MonoUsbSessionHandle.LastErrorString;
                CleanUpHandle(handle);
                throw new Exception(err);
            }
            ec = MonoUsbApi.BulkTransfer(handle, WRITEPOINT, buffer, buffer.Length, out var len, 5000);
            if (ec != 0)
            {
                string err = MonoUsbSessionHandle.LastErrorString;
                CleanUpHandle(handle);
                throw new Exception(err);
            }

            CleanUpHandle(handle);
            return len;
        }

        public int Read(byte[] buffer)
        {
            lock (_sync)
            {
                return ReadInternal(buffer);
            }
        }

        public byte[] ReadBytes(uint offset, int length)
        {
            if (length > MaximumTransferSize)
                return ReadBytesLarge(offset, length);
            lock (_sync)
            {
                var cmd = SwitchCommand.PeekRaw(offset, length);
                SendInternal(cmd);

                // give it time to push data back
                Thread.Sleep((length / 256) + UI_Settings.GetThreadSleepTime());

                var buffer = new byte[length];
                var _ = ReadInternal(buffer);
                //return Decoder.ConvertHexByteStringToBytes(buffer);
                return buffer;
            }
        }

        public void WriteBytes(byte[] data, uint offset)
        {
            if (data.Length > MaximumTransferSize)
                WriteBytesLarge(data, offset);
            lock (_sync)
            {
                SendInternal(SwitchCommand.PokeRaw(offset, data));

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