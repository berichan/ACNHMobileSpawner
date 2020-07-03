using System;
using System.Collections.Generic;
using System.IO;

namespace NH_CreationEngine
{
    // You don't need to use this, this just makes my life with Unity slighty better when loading Images
    public class SpriteParser
    {
        private string filePathSprites;
        private IDictionary<string, string> spritePointerTable;
        private IDictionary<string, ByteBoundary> spritePointerHeader;

        public static SpriteParser CurrentInstance = null;

        public SpriteParser(string filePathDmp, string filePathHeader, string filePathPointer)
        {
            filePathSprites = filePathDmp;

            spritePointerHeader = new Dictionary<string, ByteBoundary>();
            var lines = File.ReadLines(filePathHeader);
            foreach (var line in lines)
                if (line != null)
                    spritePointerHeader.Add(processHeaderLine(line));

            spritePointerTable = new Dictionary<string, string>();
            lines = File.ReadLines(filePathPointer);
            foreach (var line in lines)
                if (line != null)
                    spritePointerTable.Add(processPointerLine(line));
        }

        public byte[] GetPng(ushort itemId, byte count)
        {
            string sItemdId = itemId.ToString("X");
            string bodyVal = (count & 0xF).ToString();
            string fabricVal = (((count & 0xFF) - (count & 0xF)) / 32u).ToString();
            string fileToGet = string.Empty;
            // try full
            string check = string.Format("{0}_{1}_{2}", sItemdId, bodyVal, fabricVal);
            string check2 = string.Format("{0}_{1}", sItemdId, bodyVal);
            if (spritePointerTable.ContainsKey(check))
                fileToGet = spritePointerTable[check];
            else if (spritePointerTable.ContainsKey(check2))
                fileToGet = spritePointerTable[check2];
            else if (spritePointerTable.ContainsKey(sItemdId))
                fileToGet = spritePointerTable[sItemdId];
            else
                return null;

            ByteBoundary bb = spritePointerHeader[fileToGet];
            ulong bytesReq = bb.end - bb.start;
            List<byte> readlist = new List<byte>();
            using (BinaryReader b = new BinaryReader(File.Open(filePathSprites, FileMode.Open)))
            {
                b.BaseStream.Seek((long)bb.start, SeekOrigin.Begin);
                readlist.AddRange(b.ReadBytes((int)bytesReq));
            }

            return readlist.ToArray();
        }

        private KeyValuePair<string, ByteBoundary> processHeaderLine(string line)
        {
            string[] lineSplit = line.Split(',');
            return new KeyValuePair<string, ByteBoundary>(lineSplit[0], new ByteBoundary(lineSplit[1], lineSplit[2]));
        }

        private KeyValuePair<string, string> processPointerLine(string line)
        {
            string[] lineSplit = line.Split(',');
            return new KeyValuePair<string, string>(lineSplit[0], lineSplit[1]);
        }

        public static void DumpImagesToSingleFile(string folderPath, string singleFilePath)
        {
            string[] allFiles = Directory.GetFiles(folderPath, "*.png");
            List<byte> encodedPngs = new List<byte>();
            Dictionary<string, ByteBoundary> fileByteIndexes = new Dictionary<string, ByteBoundary>();

            int counter = 0;
            ulong byteCounter = 0;
            foreach (string file in allFiles)
            {
                byte[] png = File.ReadAllBytes(file);
                encodedPngs.AddRange(png);
                fileByteIndexes.Add(Path.GetFileNameWithoutExtension(file), new ByteBoundary(byteCounter, byteCounter + (ulong)png.Length));
                byteCounter += (ulong)png.Length; //\r\n

                Console.WriteLine("Loaded: {0}/{1}. Bytes: {2}", counter, allFiles.Length, byteCounter);
                counter++;
            }

            File.WriteAllBytes(singleFilePath, encodedPngs.ToArray());

            using (StreamWriter file = new StreamWriter(singleFilePath + ".header", false))
                foreach (var entry in fileByteIndexes)
                    file.WriteLine("{0},{1:X}", entry.Key, entry.Value);
        }

        public static void SetCurrentInstance(SpriteParser sp) => CurrentInstance = sp;

    }

    public class ByteBoundary
    {
        public ulong start; public ulong end;
        public ByteBoundary(ulong start, ulong end) { this.start = start; this.end = end; }
        public ByteBoundary(string start, string end) { this.start = ulong.Parse(start, System.Globalization.NumberStyles.HexNumber); this.end = ulong.Parse(end, System.Globalization.NumberStyles.HexNumber); }
        public override string ToString() => string.Format("{0:X},{1:X}", start, end);
    }
}


