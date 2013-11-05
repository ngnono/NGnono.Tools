using System;

namespace NGnono.Tools.Dispatcher.Core.Model
{
    /// <summary>
    /// The uri schemes.
    /// </summary>
    public class UriSchemes
    {
        /// <summary>
        /// The file system.
        /// </summary>
        public const string FileSystem = "filesystem";

        /// <summary>
        /// The ftp.
        /// </summary>
        public const string FTP = "ftp";
    }

    public class FileOpt
    {
        /// <summary>
        /// 覆盖 不存在时，创建新的
        /// </summary>
        public const string Cover = "Cover";

        /// <summary>
        /// 追加，如果不存在时报异常
        /// </summary>
        public const string Appent = "Appent";

        /// <summary>
        /// 优先追加，当不存在时覆盖
        /// </summary>
        public const string AppentOrCover = Appent + Cover;

        /// <summary>
        /// 是否追加，如果不存在时报异常
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static bool IsAppent(string opt)
        {
            return StringCompare(Appent, opt);
        }

        public static bool IsCover(string opt)
        {
            return StringCompare(Cover, opt);
        }

        public static bool IsAppentOrCover(string opt)
        {
            return StringCompare(AppentOrCover, opt);
        }

        private static bool StringCompare(string o, string s)
        {
            return String.Compare(o, s, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}