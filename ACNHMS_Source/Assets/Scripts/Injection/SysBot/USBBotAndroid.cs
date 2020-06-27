using System;
using System.Threading;


namespace NHSE.Injection
{
    public class USBBotAndroid : IRAMReadWriter
    {
        public bool Connected { get; private set; }
        private readonly object _sync = new object();
        public int MaximumTransferSize { get { return 536; } }

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
           lock (_sync)
            {
                byte[] cmd = SwitchCommand.PeekRaw(offset, length);
                AndroidUSBUtils.CurrentInstance.WriteToEndpoint(cmd);

                // give it time to push data back
                Thread.Sleep((length / 256) + 100);

                byte[] buffer = AndroidUSBUtils.CurrentInstance.ReadEndpoint(length);
                return buffer;
            }
        }

        public void WriteBytes(byte[] data, uint offset)
        {
            lock (_sync)
            {
                AndroidUSBUtils.CurrentInstance.WriteToEndpoint(SwitchCommand.PokeRaw(offset, data));

                // give it time to push data back
                Thread.Sleep((data.Length / 256) + 100);
            }
        }
    }
}
