using System;
using System.IO;
using WORD = System.UInt16;
using DWORD = System.UInt32;


namespace PmdFile.Pmd
{
    internal class ModelBone
    {
        public ModelBone(BinaryReader reader, float CoordZ, float scale)
        {
            Read(reader, CoordZ, scale);
        }

        /// <summary>
        ///     ボーン名
        /// </summary>
        public string BoneName { get; set; }

        /// <summary>
        ///     親ボーン番号
        /// </summary>
        public ushort ParentBoneIndex { get; set; }

        public ushort TailPosBoneIndex { get; set; }

        /// <summary>
        ///     ボーンのタイプ
        /// </summary>
        /// <remarks>0:回転 1:回転・移動 2:IK 3:不明 4:IK影響下 5:IK接続先 6:非表示 7:唸り 9:回転運動 </remarks>
        public byte BoneType { get; set; }

        /// <summary>
        ///     IKボーン番号
        /// </summary>
        public ushort IKParentBoneIndex { get; set; }

        /// <summary>
        ///     ボーンのヘッドの位置(x, y, z)
        /// </summary>
        public float[] BoneHeadPos { get; private set; }

        /// <summary>
        ///     ボーン名
        /// </summary>
        public string BoneNameEnglish { get; set; }

        public void Read(BinaryReader reader, float CoordZ, float scale)
        {
            BoneHeadPos = new float[3];
            BoneName = MMDUtils.GetString(reader.ReadBytes(20));
            ParentBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            TailPosBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            BoneType = reader.ReadByte();
            IKParentBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            for (var i = 0; i < BoneHeadPos.Length; i++)
                BoneHeadPos[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            BoneNameEnglish = null; // MMDModel.EnglishExpantionがtrueなら後で設定される
            // 座標系の調整
            BoneHeadPos[2] *= CoordZ;
        }

        public void ReadEnglishExpantion(BinaryReader reader)
        {
            BoneNameEnglish = MMDUtils.GetString(reader.ReadBytes(20));
        }

        public void Write(StreamWriter writer)
        {
            writer.Write(BoneName + ",");
            writer.Write(ParentBoneIndex + ",");
            writer.Write(TailPosBoneIndex + ",");
            writer.Write(BoneType + ",");
            writer.Write(IKParentBoneIndex + ",");
            foreach (var e in BoneHeadPos) writer.Write(e + ",");
            writer.Write(BoneNameEnglish + "\n");
        }
    }
}