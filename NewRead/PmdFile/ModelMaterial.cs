using System;
using System.IO;
using WORD = System.UInt16;
using DWORD = System.UInt32;

namespace PmdFile.Pmd
{
    internal class ModelMaterial
    {
        public ModelMaterial(BinaryReader reader)
        {
            Read(reader);
        }

        public float[] DiffuseColor { get; private set; }
        public float Alpha { get; set; }
        public float Specularity { get; set; }
        public float[] SpecularColor { get; private set; }
        public float[] AmbientColor { get; private set; }
        public byte ToonIndex { get; set; }
        public byte EdgeFlag { get; set; }
        public uint FaceVertCount { get; set; }
        public string TextureFileName { get; set; }
        public string SphereTextureFileName { get; set; }

        public void Read(BinaryReader reader)
        {
            DiffuseColor = new float[3];
            for (var i = 0; i < DiffuseColor.Length; i++)
                DiffuseColor[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Alpha = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Specularity = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            SpecularColor = new float[3];
            for (var i = 0; i < SpecularColor.Length; i++)
                SpecularColor[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            AmbientColor = new float[3];
            for (var i = 0; i < AmbientColor.Length; i++)
                AmbientColor[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            ToonIndex = reader.ReadByte();
            EdgeFlag = reader.ReadByte();
            FaceVertCount = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            var FileName = MMDUtils.GetString(reader.ReadBytes(20));
            var FileNames = FileName.Split('*');
            TextureFileName = SphereTextureFileName = "";
            foreach (var s in FileNames)
            {
                var ext = Path.GetExtension(s).ToLower();
                if (ext == ".sph" || ext == ".spa")
                    SphereTextureFileName = s.Trim();
                else
                    TextureFileName = s.Trim();
            }
        }

        public void Write(StreamWriter writer)
        {
            foreach (var v in DiffuseColor) writer.Write(v + ",");
            writer.Write(Alpha + ",");
            writer.Write(Specularity + ",");
            foreach (var v in SpecularColor) writer.Write(v + ",");
            foreach (var v in AmbientColor) writer.Write(v + ",");
            writer.Write(ToonIndex + ",");
            writer.Write(EdgeFlag + ",");
            writer.Write(FaceVertCount + ",");
            writer.Write(TextureFileName + ",");
            writer.Write(SphereTextureFileName + "\n");
        }
    }
}