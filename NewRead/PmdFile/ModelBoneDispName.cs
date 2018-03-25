using System.IO;

namespace PmdFile.Pmd
{
    internal class ModelBoneDispName
    {
        public ModelBoneDispName(BinaryReader reader)
        {
            Read(reader);
        }

        public string BoneDispName { get; set; }
        public string BoneDispNameEnglish { get; set; }

        public void Read(BinaryReader reader)
        {
            BoneDispName = MMDUtils.GetString(reader.ReadBytes(50));
            BoneDispNameEnglish = null;
        }

        public void ReadEnglishExpantion(BinaryReader reader)
        {
            BoneDispNameEnglish = MMDUtils.GetString(reader.ReadBytes(50));
        }

        public void Write(StreamWriter writer)
        {
            writer.Write(BoneDispName + ",");
            writer.Write(BoneDispNameEnglish + "\n");
        }
    }
}