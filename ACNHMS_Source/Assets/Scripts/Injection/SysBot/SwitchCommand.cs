using System.Linq;
using System.Text;

namespace NHSE.Injection
{
    /// <summary>
    /// Encodes commands for a <see cref="SysBot"/> to be sent as a <see cref="byte"/> array.
    /// </summary>
    public static class SwitchCommand
    {
        private static readonly Encoding Encoder = Encoding.UTF8;
        private static byte[] Encode(string command, bool addrn = true) => Encoder.GetBytes(addrn ? command + "\r\n" : command);

        /// <summary>
        /// Removes the virtual controller from the bot. Allows physical controllers to control manually.
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] DetachController() => Encode("detachController");

        /// <summary>
        /// Presses and releases a <see cref="SwitchButton"/> for 50ms.
        /// </summary>
        /// <param name="button">Button to click.</param>
        /// <remarks>Press &amp; Release timing is performed by the console automatically.</remarks>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Click(SwitchButton button) => Encode($"click {button}");

        /// <summary>
        /// Presses and does NOT release a <see cref="SwitchButton"/>.
        /// </summary>
        /// <param name="button">Button to hold.</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Hold(SwitchButton button) => Encode($"press {button}");

        /// <summary>
        /// Releases the held <see cref="SwitchButton"/>.
        /// </summary>
        /// <param name="button">Button to release.</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Release(SwitchButton button) => Encode($"release {button}");

        /// <summary>
        /// Sets the specified <see cref="stick"/> to the desired <see cref="x"/> and <see cref="y"/> positions.
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] SetStick(SwitchStick stick, int x, int y) => Encode($"setStick {stick} {x} {y}");

        /// <summary>
        /// Resets the specified <see cref="stick"/> to (0,0)
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] ResetStick(SwitchStick stick) => SetStick(stick, 0, 0);

        /// <summary>
        /// Requests the Bot to send <see cref="count"/> bytes from <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <param name="count">Amount of bytes</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Peek(uint offset, int count) => Encode($"peek 0x{offset:X8} {count}");

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be written to <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <param name="data">Data to write</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Poke(uint offset, byte[] data) => Encode($"poke 0x{offset:X8} 0x{string.Concat(data.Select(z => $"{z:X2}"))}");

        /// <summary>
        /// (Without return) Requests the Bot to send <see cref="count"/> bytes from <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <param name="count">Amount of bytes</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekRaw(uint offset, int count) => Encode($"peek 0x{offset:X8} {count}", false);

        /// <summary>
        /// (Without return) Sends the Bot <see cref="data"/> to be written to <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <param name="data">Data to write</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PokeRaw(uint offset, byte[] data) => Encode($"poke 0x{offset:X8} 0x{string.Concat(data.Select(z => $"{z:X2}"))}", false);

        /// <summary>
        /// Requests the Bot to send <see cref="count"/> bytes from absolute <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Absolute address of the data</param>
        /// <param name="count">Amount of bytes</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekAbsolute(ulong offset, int count) => Encode($"peekAbsolute 0x{offset:X16} {count}");

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be written to absolute <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Absolute address of the data</param>
        /// <param name="data">Data to write</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PokeAbsolute(ulong offset, byte[] data) => Encode($"pokeAbsolute 0x{offset:X16} 0x{string.Concat(data.Select(z => $"{z:X2}"))}");

        /// <summary>
        /// Requests the Bot to send <see cref="count"/> bytes from main <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data relative to main</param>
        /// <param name="count">Amount of bytes</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekMain(ulong offset, int count) => Encode($"peekMain 0x{offset:X16} {count}");

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be written to main <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data relative to main</param>
        /// <param name="data">Data to write</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PokeMain(ulong offset, byte[] data) => Encode($"pokeMain 0x{offset:X16} 0x{string.Concat(data.Select(z => $"{z:X2}"))}");

        /// <summary>
        /// Requests the Bot to send the current count of frozen offsets.
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Version() => Encode("getVersion");

        /// <summary>
        /// Requests the Bot to follow the main pointer and return the (absolute) offset at the end. The first value should be the main+jump
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] FollowMainPointer(long[] jumps) => Encode($"pointer{string.Concat(jumps.Select(z => $" {z}"))}");

        /// <summary>
        /// Requests the Bot to follow the main pointer and return the bytes read at the end. The first value should be the main+jump
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekMainPointer(long[] jumps, int count) => Encode($"pointerPeek {count}{string.Concat(jumps.Select(z => $" {z}"))}");

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be constantly written to <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <param name="data">Data to write</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Freeze(uint offset, byte[] data) => Encode($"freeze 0x{offset:X8} 0x{string.Concat(data.Select(z => $"{z:X2}"))}");

        /// <summary>
        /// Sends the Bot a command to unfreeze <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] UnFreeze(uint offset) => Encode($"unFreeze 0x{offset:X8}");

        /// <summary>
        /// Requests the Bot to send the current count of frozen offsets.
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] FreezeCount() => Encode("freezeCount");

        /// <summary>
        /// Requests the Bot to clear all current frozen values
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] FreezeClear() => Encode("freezeClear");

        /// <summary>
        /// Requests the Bot to pause all freezes.
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] FreezePause() => Encode("freezePause");

        /// <summary>
        /// Requests the Bot to unpause freezes
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] FreezeUnpause() => Encode("freezeUnpause");

        /// <summary>
        /// Requests the Bot to clear all current frozen values
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Configure(string name, string value) => Encode($"configure {name} {value}");

        /// <summary>
        /// Presses and releases a <see cref="SwitchButton"/> for 50ms.
        /// </summary>
        /// <remarks>Press &amp; Release timing is performed by the console automatically.</remarks>
        /// <param name="button">Button to click.</param>
        /// <param name="crlf">Line terminator (unused by USB's protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Click(SwitchButton button, bool crlf = true) => Encode($"click {button}", crlf);

        /// <summary>
        /// Presses and does NOT release a <see cref="SwitchButton"/>.
        /// </summary>
        /// <param name="button">Button to hold.</param>
        /// <param name="crlf">Line terminator (unused by USB's protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Hold(SwitchButton button, bool crlf = true) => Encode($"press {button}", crlf);

        /// <summary>
        /// Releases the held <see cref="SwitchButton"/>.
        /// </summary>
        /// <param name="button">Button to release.</param>
        /// <param name="crlf">Line terminator (unused by USB's protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Release(SwitchButton button, bool crlf = true) => Encode($"release {button}", crlf);

        public static byte[] SetScreen(bool on, bool crlf = true) => Encode($"screen{(on ? "On" : "Off")}", crlf);

        public static byte[] BatteryCharge() => Encode("charge");
    }

    public static class SwitchCommandMethodHelper
    {
        public static byte[] GetPeekCommand(ulong offset, int count, RWMethod method, bool usb)
        {
            switch (method)
            {
                case RWMethod.Heap when !usb: return SwitchCommand.Peek((uint)offset, count);
                case RWMethod.Heap when usb: return SwitchCommand.PeekRaw((uint)offset, count);
                case RWMethod.Main: return SwitchCommand.PeekMain(offset, count);
                case RWMethod.Absolute: return SwitchCommand.PeekAbsolute(offset, count);
                default: return SwitchCommand.Peek((uint)offset, count);
            }
        }

        public static byte[] GetPokeCommand(ulong offset, byte[] data, RWMethod method, bool usb)
        {
            switch (method)
            {
                case RWMethod.Heap when !usb: return SwitchCommand.Poke((uint)offset, data);
                case RWMethod.Heap when usb: return SwitchCommand.PokeRaw((uint)offset, data);
                case RWMethod.Main: return SwitchCommand.PokeMain(offset, data);
                case RWMethod.Absolute: return SwitchCommand.PokeAbsolute(offset, data);
                default: return SwitchCommand.Poke((uint)offset, data);
            }
        }
    }
}