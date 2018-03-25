using System.Collections.Generic;
using System.IO;
using PMDEditor;

namespace Pmxfile
{
    public class PmxFile
    {
        // PMDEditor.Pmx
        internal static Pmx FromStream(Stream s, PmxElementFormat f = null)
        {
            var Ret = new Pmx();
            var pmxHeader = new PmxHeader(2f);
            pmxHeader.FromStreamEx(s, null);
            Ret.Header = pmxHeader;
            if (pmxHeader.Ver <= 1f)
            {
                var mMD_Pmd = new MMD_Pmd();
                s.Seek(0L, SeekOrigin.Begin);
                mMD_Pmd.FromStreamEx(s, null);
                Ret.FromPmx(PmxConvert.PmdToPmx(mMD_Pmd));
                return Ret;
            }
            Ret.ModelInfo = new PmxModelInfo();
            Ret.ModelInfo.FromStreamEx(s, pmxHeader.ElementFormat);
            var num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.VertexList = new List<PmxVertex>();
            Ret.VertexList.Clear();
            Ret.VertexList.Capacity = num;
            for (var i = 0; i < num; i++)
            {
                var pmxVertex = new PmxVertex();
                pmxVertex.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.VertexList.Add(pmxVertex);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.FaceList = new List<int>();
            Ret.FaceList.Clear();
            Ret.FaceList.Capacity = num;
            for (var j = 0; j < num; j++)
            {
                var item = PmxStreamHelper.ReadElement_Int32(s, pmxHeader.ElementFormat.VertexSize, false);
                Ret.FaceList.Add(item);
            }
            var pmxTextureTable = new PmxTextureTable();
            pmxTextureTable.FromStreamEx(s, pmxHeader.ElementFormat);
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.MaterialList = new List<PmxMaterial>();
            Ret.MaterialList.Clear();
            Ret.MaterialList.Capacity = num;
            for (var k = 0; k < num; k++)
            {
                var pmxMaterial = new PmxMaterial();
                pmxMaterial.FromStreamEx_TexTable(s, pmxTextureTable, pmxHeader.ElementFormat);
                Ret.MaterialList.Add(pmxMaterial);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.BoneList = new List<PmxBone>();
            Ret.BoneList.Clear();
            Ret.BoneList.Capacity = num;

            for (var l = 0; l < num; l++)
            {
                var pmxBone = new PmxBone();
                pmxBone.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.BoneList.Add(pmxBone);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.MorphList = new List<PmxMorph>();
            Ret.MorphList.Clear();
            Ret.MorphList.Capacity = num;
            for (var m = 0; m < num; m++)
            {
                var pmxMorph = new PmxMorph();
                pmxMorph.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.MorphList.Add(pmxMorph);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.NodeList = new List<PmxNode>();
            Ret.NodeList.Clear();
            Ret.NodeList.Capacity = num;
            for (var n = 0; n < num; n++)
            {
                var pmxNode = new PmxNode();
                pmxNode.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.NodeList.Add(pmxNode);
                if (Ret.NodeList[n].SystemNode)
                    if (Ret.NodeList[n].Name == "Root")
                        Ret.RootNode = Ret.NodeList[n];
                    else if (Ret.NodeList[n].Name == "表情")
                        Ret.ExpNode = Ret.NodeList[n];
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.BodyList = new List<PmxBody>();
            Ret.BodyList.Clear();
            Ret.BodyList.Capacity = num;
            for (var num2 = 0; num2 < num; num2++)
            {
                var pmxBody = new PmxBody();
                pmxBody.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.BodyList.Add(pmxBody);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.JointList = new List<PmxJoint>();
            Ret.JointList.Clear();
            Ret.JointList.Capacity = num;
            for (var num3 = 0; num3 < num; num3++)
            {
                var pmxJoint = new PmxJoint();
                pmxJoint.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.JointList.Add(pmxJoint);
            }
            return Ret;
        }
    }
}