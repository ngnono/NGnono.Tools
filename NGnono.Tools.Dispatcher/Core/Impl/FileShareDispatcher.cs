using NGnono.Tools.Dispatcher.Core.Contract;
using NGnono.Tools.Dispatcher.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NGnono.Tools.Dispatcher.Core.Impl
{
    /// <summary>
    /// FileShareDispatcher
    /// </summary>
    public class FileShareDispatcher : IDispatcher
    {
        /// <summary>
        /// 分发完成后事件
        /// </summary>
        public event DispatcherEndHandle OnPublishedEventHandle;

        /// <summary>
        /// 发布完成后数据
        /// </summary>
        public object DispatcherEndData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the dispatcher scheme.
        /// </summary>
        public DispatcherSchemes DispatcherScheme
        {
            get
            {
                return new DispatcherSchemes { UriScheme = UriSchemes.FileSystem };
            }
        }

        /// <summary>
        /// 分发内容
        /// </summary>
        /// <param name="content">要分发的内容</param>
        /// <param name="uris">要分发的地址</param>
        public void Dispatch(string content, IEnumerable<Uri> uris)
        {
            Dispatch(content, uris, Encoding.UTF8);
            //if (uris == null)
            //{
            //    throw new Exception("分发目标不能为空");
            //}

            //byte[] contentBytes = Encoding.UTF8.GetBytes(content);

            //foreach (var fp in uris)
            //{
            //    string tempPath = fp.LocalPath.Substring(0, fp.LocalPath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase));

            //    if (!Directory.Exists(tempPath))
            //    {
            //        Directory.CreateDirectory(tempPath);
            //    }

            //    using (Stream s = File.Create(fp.LocalPath))
            //    {
            //        s.Write(contentBytes, 0, contentBytes.Length);
            //    }

            //    using (var stream = new StreamWriter(fp.LocalPath, false, Encoding.UTF8))
            //    {
            //        stream.Write(content);
            //    }
            //}

            //this.PublishEnd();
        }

        /// <summary>
        /// 分发内容
        /// </summary>
        /// <param name="content">要分发的内容</param>
        /// <param name="uris">要分发的地址</param>
        /// <param name="encoding">编码格式</param>
        public void Dispatch(string content, IEnumerable<Uri> uris, Encoding encoding)
        {
            if (uris == null)
            {
                throw new Exception("分发目标不能为空");
            }

            foreach (var fp in uris)
            {
                var tempPath = fp.LocalPath.Substring(
                    0, fp.LocalPath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase));

                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                using (var stream = new StreamWriter(fp.LocalPath, false, encoding, 1024))
                {
                    stream.Write(content);
                }
            }

            this.PublishEnd();
        }

        /// <summary>
        /// 执行事发布完成事件
        /// </summary>
        public void PublishEnd()
        {
            if (OnPublishedEventHandle == null || DispatcherEndData == null) return;

            var ev = new DispatcherEndEventaArgs(DispatcherEndData);

            OnPublishedEventHandle(this, ev);
        }

        /// <summary>
        /// The copy file.
        /// </summary>
        /// <param name="srcPath">
        /// The src path.
        /// </param>
        /// <param name="aimPath">
        /// The aim path.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        private void CopyFile(string srcPath, string aimPath, string fileName)
        {
            // 检查目标目录是否以目录分割字符结束如果不是则添加之
            if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                aimPath += Path.DirectorySeparatorChar;
            }

            // 判断目标目录是否存在如果不存在则新建之
            if (!Directory.Exists(aimPath))
            {
                Directory.CreateDirectory(aimPath);
            }

            File.Copy(srcPath + "\\" + fileName, aimPath + fileName, true);
        }
    }
}
