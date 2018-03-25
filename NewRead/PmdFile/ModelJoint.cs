using System;
using System.IO;
using WORD = System.UInt16;
using DWORD = System.UInt32;


namespace PmdFile.Pmd
{
    internal class ModelJoint
    {
        public ModelJoint(BinaryReader reader, float CoordZ, float scale)
        {
            Read(reader, CoordZ, scale);
        }

        public string Name { get; set; }
        public uint RigidBodyA { get; set; }
        public uint RigidBodyB { get; set; }
        public float[] Position { get; private set; }
        public float[] Rotation { get; private set; }
        public float[] ConstrainPosition1 { get; private set; }
        public float[] ConstrainPosition2 { get; private set; }
        public float[] ConstrainRotation1 { get; private set; }
        public float[] ConstrainRotation2 { get; private set; }
        public float[] SpringPosition { get; private set; }
        public float[] SpringRotation { get; private set; }

        public void Read(BinaryReader reader, float CoordZ, float scale)
        {
            Name = MMDUtils.GetString(reader.ReadBytes(20));
            RigidBodyA = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            RigidBodyB = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            Position = new float[3];
            for (var i = 0; i < Position.Length; i++)
                Position[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            Rotation = new float[3];
            for (var i = 0; i < Rotation.Length; i++)
                Rotation[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            ConstrainPosition1 = new float[3];
            for (var i = 0; i < ConstrainPosition1.Length; i++)
                ConstrainPosition1[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            ConstrainPosition2 = new float[3];
            for (var i = 0; i < ConstrainPosition2.Length; i++)
                ConstrainPosition2[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            ConstrainRotation1 = new float[3];
            for (var i = 0; i < ConstrainRotation1.Length; i++)
                ConstrainRotation1[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            ConstrainRotation2 = new float[3];
            for (var i = 0; i < ConstrainRotation2.Length; i++)
                ConstrainRotation2[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            SpringPosition = new float[3];
            for (var i = 0; i < SpringPosition.Length; i++)
                SpringPosition[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            SpringRotation = new float[3];
            for (var i = 0; i < SpringRotation.Length; i++)
                SpringRotation[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            // 必要？
            ConstrainRotation1[0] *= CoordZ;
            ConstrainRotation1[1] *= CoordZ;
            ConstrainRotation2[0] *= CoordZ;
            ConstrainRotation2[1] *= CoordZ;
            // これは必要
            ConstrainPosition1[2] *= CoordZ;
            ConstrainPosition2[2] *= CoordZ;
        }


        public void Write(StreamWriter writer)
        {
            writer.Write(Name + ",");
            writer.Write(RigidBodyA + ",");
            writer.Write(RigidBodyB + ",");
            foreach (var e in Position) writer.Write(e + ",");
            foreach (var e in Rotation) writer.Write(e + ",");
            foreach (var e in ConstrainPosition1) writer.Write(e + ",");
            foreach (var e in ConstrainPosition2) writer.Write(e + ",");
            foreach (var e in ConstrainRotation1) writer.Write(e + ",");
            foreach (var e in ConstrainRotation2) writer.Write(e + ",");
            foreach (var e in SpringPosition) writer.Write(e + ",");
            foreach (var e in SpringRotation) writer.Write(e);
        }
    }
}