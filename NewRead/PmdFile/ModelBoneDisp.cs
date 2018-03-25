using System;
using System.IO;
using WORD = System.UInt16;
using DWORD = System.UInt32;

namespace PmdFile.Pmd
{
    /// <summary>
    ///     ボーン枠用表示データ
    /// </summary>
    internal class ModelBoneDisp
    {
        public ModelBoneDisp(BinaryReader reader)
        {
            Read(reader);
        }

        /// <summary>
        ///     枠用ボーン番号
        /// </summary>
        public ushort BoneIndex { get; set; }

        /// <summary>
        ///     表示枠用番号
        /// </summary>
        public byte BoneDispFrameIndex { get; set; }

        public void Read(BinaryReader reader)
        {
            BoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            BoneDispFrameIndex = reader.ReadByte();
        }

        public void Write(StreamWriter writer)
        {
            writer.Write(BoneIndex + ",");
            writer.Write(BoneDispFrameIndex + "\n");
        }
    }
}