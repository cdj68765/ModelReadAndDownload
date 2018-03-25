using System.Text;

namespace PmdFile.Pmd
{
    /// <summary>
    ///     座標系
    /// </summary>
    public enum CoordinateType
    {
        /// <summary>
        ///     左手座標系（MMDの標準座標系）
        /// </summary>
        LeftHandedCoordinate = 1,

        /// <summary>
        ///     右手座標系（XNAの標準座標系）
        /// </summary>
        RightHandedCoordinate = -1
    }

    internal class MMDUtils
    {
        /// <summary>
        ///     pmdファイルはshift-jis
        /// </summary>
        public static Encoding encoder = Encoding.GetEncoding("shift-jis");

        public static string GetString(byte[] bytes)
        {
            var i = 0;
            foreach (var v in bytes)
            {
                if (v == 0) break;
                i++;
            }
            if (i < bytes.Length)
                return encoder.GetString(bytes, 0, i);
            return encoder.GetString(bytes);
        }
    }
}