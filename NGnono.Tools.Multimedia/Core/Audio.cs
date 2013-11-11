using NGnono.Framework.Process;
using NGnono.Tools.Multimedia.Core.Cfg;
using System;
using System.IO;
using System.Text;

namespace NGnono.Tools.Multimedia.Core
{
    public interface IAudioHandler
    {
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="orgFullName"></param>
        /// <param name="tgtFullName"></param>
        void Compression(string orgFullName, string tgtFullName);

        /// <summary>
        /// 获取时间
        /// </summary>
        /// <param name="orgFullName"></param>
        /// <returns></returns>
        TimeSpan GetDuration(string orgFullName);
    }

    public abstract class AudioHandlerBase : IAudioHandler
    {
        public abstract void Compression(string orgFullName, string tgtFullName);
        public abstract TimeSpan GetDuration(string orgFullName);
    }

    internal class FfmpegAudioHandler : AudioHandlerBase
    {

        //        What you actually need to know to use ffmpeg:

        //  # convert "yoursourcefile" to "youroutputfile", using 4mbit/s xvid and 192kbit/s mp3
        //  ffmpeg -i yoursourcefile -vcodec libxvid -b 4000000 -acodec libmp3lame -ab 192000 youroutputfile

        //  # as above, but rescale it
        //  ffmpeg -i yoursourcefile -s 640x480 -vcodec libxvid -b 4000000 -acodec libmp3lame -ab 192000 youroutputfile

        //  # same as first, but use VBR with quality '8' (fixed quality setting, so bitrate varies to meet that quality setting)
        //  ffmpeg -i yoursourcefile -vcodec libxvid -qscale 8 -acodec libmp3lame -ab 192000 youroutputfile

        //  # same as first, but pass through the audio unchanged
        //  ffmpeg -i yoursourcefile -vcodec libxvid -b 4000000 -acodec copy youroutputfile

        //  # as above, but no audio
        //  ffmpeg -i yoursourcefile -vcodec libxvid -b 4000000 -an youroutputfile

        //  # similarly, audio but no video
        //  ffmpeg -i yoursourcefile -vn -acodec libmp3lame -ab 192000 youroutputfile

        //  # get a list of compression formats and output file types
        //  ffmpeg -formats


        //Breaking it down with each option / set of options separately (this is what the ffmpeg usage should actually look like):

        //Command:
        //    ffmpeg

        //Input file:
        //    -i yoursourcefile

        //Optional rescaling
        //    -s 640x480

        //Video conversion (choose one)
        //    -vcodec libxvid -b 4000000         (CBR bitrate=4000Mb/s)
        //    -vcodec libxvid -qscale 8          (VBR quality=8 -- arbitrary number, try different values)
        //    -vcodec copy
        //    -vn

        //Audio conversion (choose one)
        //    -acodec libmp3lame -ab 192000
        //    -acodec copy
        //    -an

        //Output file (this name appears on the line without an option before it)
        //    youroutputfile

        private readonly string _compressExePath;
        private static FfmpegAudioHandler _instance;
        private static readonly object SyncObject = new object();
        private readonly IProcessExecuteProvider _processExecuteProvider;

        #region .ctor

        private FfmpegAudioHandler()
            : this(Path.GetFullPath(CfgManager.GetAudioCompressionPathRaw), ProcessExecuteManager.Current)
        {
        }

        private FfmpegAudioHandler(string exePath, IProcessExecuteProvider processExecuteProvider)
        {
            _compressExePath = exePath;
            _processExecuteProvider = processExecuteProvider;
        }

        #endregion

        #region properties

        public static FfmpegAudioHandler Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncObject)
                    {
                        if (_instance == null)
                        {
                            // ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
                            _instance = new FfmpegAudioHandler();
                            // ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region methods

        private void CallProcess(string exePath, string fileArgs)
        {
            _processExecuteProvider.Exectue(exePath, fileArgs);
        }

        private string CallProcessAndReturn(string exePath, string fileArgs)
        {
            return _processExecuteProvider.ExectueReturn(exePath, fileArgs);
        }

        private static string ConvertDuration(string str)
        {
            return !String.IsNullOrEmpty(str) ? str.Substring(str.IndexOf("Duration: ", StringComparison.Ordinal) + ("Duration: ").Length, ("00:00:00").Length) : String.Empty;
        }

        #endregion

        public override void Compression(string originalFullName, string targetFullName)
        {
            var sbFileArgs = new StringBuilder()
                //.AppendFormat(" -i {0} -acodec libmp3lame -ac 2 -ab 17k -ar 16000 -vol 200 {1}", originalFullName, targetFullName);
               .AppendFormat(CfgManager.GetAudioCompressionParamsRaw, Path.GetFullPath(originalFullName), targetFullName);
            var fileArgs = sbFileArgs.ToString();

            CallProcess(_compressExePath, fileArgs);
        }

        public override TimeSpan GetDuration(string originalFullName)
        {
            var sbFileArgs = new StringBuilder()
   .AppendFormat(" -i {0}", Path.GetFullPath(originalFullName));
            var fileArgs = sbFileArgs.ToString();

            var result = ConvertDuration(CallProcessAndReturn(_compressExePath, fileArgs));

            return !String.IsNullOrEmpty(result) ? TimeSpan.Parse(result) : TimeSpan.Zero;
        }
    }
}
