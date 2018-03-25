using System;
using System.IO;
using WORD = System.UInt16;
using DWORD = System.UInt32;

namespace PmdFile.Pmd
{
    internal class ModelIK
    {
        public ModelIK(BinaryReader reader)
        {
            Read(reader);
        }

        public ushort IKBoneIndex { get; set; }
        public ushort IKTargetBoneIndex { get; set; }
        public ushort Iterations { get; set; }
        public float AngleLimit { get; set; }
        public ushort[] IKChildBoneIndex { get; set; }

        public void Read(BinaryReader reader)
        {
            IKBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            IKTargetBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            var chainLength = reader.ReadByte();
            Iterations = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            AngleLimit = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            IKChildBoneIndex = new ushort[chainLength];
            for (var i = 0; i < chainLength; i++)
                IKChildBoneIndex[i] = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        }

        public void Write(StreamWriter writer)
        {
            writer.Write(IKBoneIndex + ",");
            writer.Write(IKTargetBoneIndex + ",");
            writer.Write(Iterations + ",");
            writer.Write(AngleLimit + ",");
            foreach (var e in IKChildBoneIndex) writer.Write(e + ",");
            writer.WriteLine();
        }
    }
}