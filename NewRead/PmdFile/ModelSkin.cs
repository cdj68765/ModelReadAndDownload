using System;
using System.IO;
using WORD = System.UInt16;
using DWORD = System.UInt32;

namespace PmdFile.Pmd
{
    internal class ModelSkinVertexData
    {
        public ModelSkinVertexData(BinaryReader reader, float CoordZ, float scale)
        {
            Read(reader, CoordZ, scale);
        }

        public uint SkinVertIndex { get; set; }
        public float[] SkinVertPos { get; set; }

        public void Read(BinaryReader reader, float CoordZ, float scale)
        {
            SkinVertIndex = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            SkinVertPos = new float[3];
            for (var i = 0; i < SkinVertPos.Length; i++)
                SkinVertPos[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            SkinVertPos[2] *= CoordZ;
        }

        public void Write(StreamWriter writer)
        {
            writer.Write(SkinVertIndex + ",");
            foreach (var v in SkinVertPos) writer.Write(v + ",");
            writer.WriteLine();
        }
    }

    internal class ModelSkin
    {
        public ModelSkin(BinaryReader reader, float CoordZ, float scale)
        {
            Read(reader, CoordZ, scale);
        }

        public string SkinName { get; set; }
        public byte SkinType { get; set; }
        public ModelSkinVertexData[] SkinVertDatas { get; set; }
        public string SkinNameEnglish { get; set; }

        public void Read(BinaryReader reader, float CoordZ, float scale)
        {
            SkinName = MMDUtils.GetString(reader.ReadBytes(20));
            var skinVertCount = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            SkinType = reader.ReadByte();
            SkinVertDatas = new ModelSkinVertexData[skinVertCount];
            for (var i = 0; i < SkinVertDatas.Length; i++)
                SkinVertDatas[i] = new ModelSkinVertexData(reader, CoordZ, scale);
            SkinNameEnglish = null;
        }

        public void ReadEnglishExpantion(BinaryReader reader)
        {
            SkinNameEnglish = MMDUtils.GetString(reader.ReadBytes(20));
        }

        public void Write(StreamWriter writer)
        {
            writer.Write(SkinName + ",");
            writer.Write(SkinType + ",");
            writer.Write(SkinNameEnglish + "\n");
            foreach (var e in SkinVertDatas) e.Write(writer);
        }
    }
}