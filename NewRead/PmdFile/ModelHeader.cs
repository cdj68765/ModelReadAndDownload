using System.IO;

namespace PmdFile.Pmd
{
    internal class ModelHeader
    {
        public ModelHeader(BinaryReader reader)
        {
            Read(reader);
        }

        public string ModelName { get; set; }
        public string Comment { get; set; }
        public string ModelNameEnglish { get; set; }
        public string CommentEnglish { get; set; }

        public void Read(BinaryReader reader)
        {
            ModelName = MMDUtils.GetString(reader.ReadBytes(20));
            Comment = MMDUtils.GetString(reader.ReadBytes(256));
            ModelNameEnglish = CommentEnglish = null;
        }

        public void ReadEnglishExpantion(BinaryReader reader)
        {
            ModelNameEnglish = MMDUtils.GetString(reader.ReadBytes(20));
            CommentEnglish = MMDUtils.GetString(reader.ReadBytes(256));
        }

        public void Write(StreamWriter writer)
        {
            writer.WriteLine("モデル名," + ModelName);
            writer.WriteLine("コメント," + Comment.Replace("\n", "\n,"));
            writer.WriteLine("モデル（英語）," + ModelNameEnglish);
            writer.WriteLine("コメント（英語）," + (CommentEnglish == null ? "" : CommentEnglish.Replace("\n", "\n,")));
        }
    }
}