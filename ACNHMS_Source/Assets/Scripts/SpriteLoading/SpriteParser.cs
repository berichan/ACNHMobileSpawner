using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace NH_CreationEngine
{
    // You don't need to use this, this just makes my life with Unity slighty better when loading Images
    public class CachedTextureEntry
    {
        public string ID { get; set; }
        public Texture2D Tex { get; set; }

        public CachedTextureEntry(string id, Texture2D tx)
        {
            ID = id; Tex = tx;
            Tex.hideFlags = HideFlags.HideAndDontSave;
        }

        public void Clean()
        {
            // IDisposable is terrible for this
            UnityEngine.Object.Destroy(Tex);
        }
    }

    public class SpriteParser
    {
        private const int MaxTextures = 1000;

        private string filePathSprites;
        private IDictionary<string, ByteBoundary> spritePointerHeader;
        private IDictionary<string, string> spritePointerTable = null;

        public IDictionary<string, ByteBoundary> SpritePointerHeader { get => spritePointerHeader; }
        public IDictionary<string, string> SpritePointerTable { get => spritePointerTable; }

        public static SpriteParser CurrentInstance = null;

        private SortedList<long, CachedTextureEntry> cachedIdTextureMap = new SortedList<long, CachedTextureEntry>();

        private long lastLong = 0;

        public SpriteParser(string filePathDmp, string filePathHeader)
        {
            filePathSprites = filePathDmp;

            spritePointerHeader = new Dictionary<string, ByteBoundary>();
            var lines = File.ReadLines(filePathHeader);
            foreach (var line in lines)
                if (line != null)
                    spritePointerHeader.Add(processHeaderLine(line));
        }

        public SpriteParser(string filePathDmp, string filePathHeader, string filePathPointer)
            : this(filePathDmp, filePathHeader)
        {
            spritePointerTable = new Dictionary<string, string>();
            var lines = File.ReadLines(filePathPointer);
            foreach (var line in lines)
                if (line != null)
                    spritePointerTable.Add(processPointerLine(line));
        }

        private void updateEntry(long id, CachedTextureEntry cte)
        {
            cachedIdTextureMap.Remove(id);
            initEntry(cte);
        }

        private void initEntry(CachedTextureEntry cte)
        {
            lastLong++;
            cachedIdTextureMap.Add(lastLong, cte);
        }

        private void initEntry(string id, Texture2D tx)
        {
            var ncte = new CachedTextureEntry(id, tx);
            initEntry(ncte);
        }

        private Texture2D tryGetViaID(string id)
        {
            var ctePair = cachedIdTextureMap.FirstOrDefault(x => x.Value.ID == id);
            if (!ctePair.Equals(default(KeyValuePair<long, CachedTextureEntry>)))
            {
                updateEntry(ctePair.Key, ctePair.Value);
                return ctePair.Value.Tex;
            }

            return null;
        }

        private Texture2D getAndUpdateMap(string id, byte[] bytes)
        {
            if (bytes == null)
                return null;
            var toAssignImage = new Texture2D(2, 2);
            toAssignImage.LoadImage(bytes);
            initEntry(id, toAssignImage);

            while (cachedIdTextureMap.Count > MaxTextures)
            {
                cachedIdTextureMap.ElementAt(0).Value.Clean();
                cachedIdTextureMap.RemoveAt(0);
            }

            return toAssignImage;
        }

        public Texture2D GetTexture(string itemId, ushort count)
        {
            string id = $"{itemId}-{count}";
            Texture2D exists = tryGetViaID(id);
            if (exists != null)
                return exists;

            var bytes = GetPng(itemId, count);
            return getAndUpdateMap(id, bytes);
        }

        private byte[] GetPng(string itemId, ushort count)
        {
            if (spritePointerTable == null)
                throw new Exception("Not pointer table loaded.");
            if (itemId == null)
                return null;
            string sItemdId = itemId;
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

        public Texture2D GetTexture(ushort itemId, ushort count)
        {
            string id = $"{itemId}-{count}";
            Texture2D exists = tryGetViaID(id);
            if (exists != null)
                return exists;

            var bytes = GetPng(itemId, count);
            return getAndUpdateMap(id, bytes);
        }

        private byte[] GetPng(ushort itemId, ushort count)
        {
            if (spritePointerTable == null)
                throw new Exception("Not pointer table loaded.");
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

        public Texture2D GetTexture(string itemName)
        {
            string id = $"{itemName}-0"; // map 0 to fix issues with unloaded maps 
            Texture2D exists = tryGetViaID(id);
            if (exists != null)
                return exists;

            var bytes = GetPng(itemName);
            return getAndUpdateMap(id, bytes);
        }

        private byte[] GetPng(string itemName)
        {
            if (!spritePointerHeader.ContainsKey(itemName))
                return null;
            ByteBoundary bb = spritePointerHeader[itemName];
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

                Console.WriteLine("Loaded: {0}/{1}. Bytes: {2}", counter, allFiles.Length, byteCounter / 1000);
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


