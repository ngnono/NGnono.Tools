using System.Configuration;

namespace NGnono.Tools.Multimedia.Core.Cfg
{
    internal static class Define
    {
        public const string AudioCompressionPath = "audiocompression_exepath";

        /// <summary>
        ///  -i {0} -acodec libmp3lame -ac 2 -ab 17k -ar 16000 -vol 200 {1}
        /// </summary>
        public const string AudioCompressionParams = "audiocompression_params";
    }

    internal static class CfgManager
    {
        /// <summary>
        /// 获取 音频压缩软件物理地址
        /// </summary>
        public static string GetAudioCompressionPathRaw
        {
            get
            {
                return ConfigurationManager.AppSettings[Define.AudioCompressionPath];
            }
        }

        public static string GetAudioCompressionParamsRaw
        {
            get
            {
                return ConfigurationManager.AppSettings[Define.AudioCompressionParams];
            }
        }
    
    }
}
