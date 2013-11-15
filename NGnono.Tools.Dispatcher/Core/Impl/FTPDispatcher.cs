using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NGnono.Tools.Dispatcher.Core.Contract;
using NGnono.Tools.Dispatcher.Core.Model;

namespace NGnono.Tools.Dispatcher.Core.Impl
{
    /// <summary>
    /// FTP分发器
    /// </summary>
    public class FTPDispatcher : IDispatcher
    {
        /// <summary>
        /// FTP请求对象
        /// </summary>
        private FtpWebRequest ftpRequest = null;

        /// <summary>
        /// FTP登录密码
        /// </summary>
        private string ftpPassword;

        /// <summary>
        /// FTP登录名
        /// </summary>
        private string ftpUserName;

        /// <summary>
        /// Uri
        /// </summary>
        private Uri uri;

        /// <summary>
        /// 当前工作目录
        /// </summary>
        private string directoryPath;

        /// <summary>
        ///  Initializes a new instance of the <see cref="FTPDispatcher"/> class. 
        ///  初始化 
        /// </summary>
        /// <param name="ftpUri">ftp地址</param>
        /// <param name="ftpUserName">ftp登录名</param>
        /// <param name="ftpPassword">ftp登录密码</param>
        public FTPDispatcher(List<Uri> ftpUri, string ftpUserName, string ftpPassword)
        {
            this.Uris = ftpUri;

            this.ftpUserName = ftpUserName;
            this.ftpPassword = ftpPassword;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="FTPDispatcher"/> class. 
        ///  初始化  
        /// </summary>
        /// <param name="ftpUri">ftp地址</param>
        /// <param name="ftpUserName">ftp登录名</param>
        /// <param name="ftpPassword">ftp登录密码</param>
        public FTPDispatcher(Uri ftpUri, string ftpUserName, string ftpPassword)
        {
            this.uri = new Uri(ftpUri.GetLeftPart(UriPartial.Authority));
            this.directoryPath = ftpUri.AbsolutePath;

            if (!this.directoryPath.EndsWith("/"))
            {
                this.directoryPath += "/";
            }

            this.ftpUserName = ftpUserName;
            this.ftpPassword = ftpPassword;
        }

        /// <summary>
        /// 分发完成后事件
        /// </summary>
        public event DispatcherEndHandle OnPublishedEventHandle;

        /// <summary>
        /// 分发后数据
        /// </summary>
        public object DispatcherEndData
        {
            get;
            set;
        }

        /// <summary>
        /// uri
        /// </summary>
        public Uri Uri
        {
            get
            {
                if (this.directoryPath == "/")
                {
                    return this.uri;
                }
                else
                {
                    string strUri = this.uri.ToString();
                    if (strUri.EndsWith("/"))
                    {
                        strUri = strUri.Substring(0, strUri.Length - 1);
                    }

                    return new Uri(strUri + this.DirectoryPath);
                }
            }

            set
            {
                if (value.Scheme != Uri.UriSchemeFtp)
                {
                    throw new Exception("Ftp 地址格式错误!");
                }

                this.uri = new Uri(value.GetLeftPart(UriPartial.Authority));
                this.directoryPath = value.AbsolutePath;

                if (!this.directoryPath.EndsWith("/"))
                {
                    this.directoryPath += "/";
                }
            }
        }

        /// <summary>
        /// FTP服务器地址
        /// </summary>
        public List<Uri> Uris { get; set; }

        /// <summary>
        /// 当前工作目录
        /// </summary>
        public string DirectoryPath
        {
            get { return this.directoryPath; }
            set { this.directoryPath = value; }
        }

        /// <summary>
        /// Gets the dispatcher scheme.
        /// </summary>
        public DispatcherSchemes DispatcherScheme
        {
            get
            {
                return new DispatcherSchemes() { UriScheme = UriSchemes.FTP };
            }
        }

        /// <summary>
        /// 分发
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="uris">目标地址</param>
        public void Dispatch(string content, IEnumerable<Uri> uris)
        {
            Dispatch(content, uris, Encoding.UTF8);
        }

        /// <summary>
        /// 分发
        /// </summary>
        /// <param name="content">要分发的内容</param>
        /// <param name="uris">分发目标uri</param>
        /// <param name="encoding"></param>
        public void Dispatch(string content, IEnumerable<Uri> uris, Encoding encoding)
        {
            if (string.IsNullOrEmpty(content)) return;

            byte[] contentBytes = encoding.GetBytes(content);

            if (this.Uris != null && this.Uris.Count > 0)
            {
                foreach (var u in this.Uris)
                {
                    this.UploadFile(contentBytes, u);
                }
            }
        }

        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="fileBytes">文件二进制内容</param>
        /// <param name="tergetFileName">要在FTP服务器上面保存文件名</param>
        /// <returns>bool</returns>
        public bool UploadFile(byte[] fileBytes, string tergetFileName)
        {
            this.ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(this.Uri.ToString() + tergetFileName));
            this.ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            this.ftpRequest.UseBinary = true;
            this.ftpRequest.Credentials = new NetworkCredential(this.ftpUserName, this.ftpPassword);

            Stream requestStream = this.ftpRequest.GetRequestStream();
            MemoryStream mem = new MemoryStream(fileBytes);
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            int totalRead = 0;

            while (true)
            {
                bytesRead = mem.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                totalRead += bytesRead;
                requestStream.Write(buffer, 0, bytesRead);
            }

            requestStream.Close();
            mem.Dispose();
            return true;
        }

        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="fileBytes">文件二进制内容</param>
        /// <param name="uri">要在FTP服务器上面保存文件名</param>
        /// <returns>bool</returns>
        public bool UploadFile(byte[] fileBytes, Uri uri)
        {
            Uri tempuri = new Uri(uri.GetLeftPart(UriPartial.Authority));
            string directoryPath = uri.AbsolutePath;
            string directoryPathNoFileName = uri.ToString().Substring(0, directoryPath.LastIndexOf("/"));

            this.ftpRequest =
                (FtpWebRequest)
                WebRequest.Create(tempuri.ToString().Remove(tempuri.ToString().Length - 1) +
                                  directoryPath.Replace("/", "\\"));
            this.ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            this.ftpRequest.UseBinary = true;
            this.ftpRequest.Credentials = new NetworkCredential(this.ftpUserName, this.ftpPassword);

            Stream requestStream = this.ftpRequest.GetRequestStream();
            MemoryStream mem = new MemoryStream(fileBytes);
            byte[] buffer = new byte[1024];
            int bytesRead;
            int totalRead = 0;
            while (true)
            {
                bytesRead = mem.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                totalRead += bytesRead;
                requestStream.Write(buffer, 0, bytesRead);
            }

            requestStream.Close();
            this.ftpRequest.GetResponse();
            mem.Dispose();
            return true;
        }

        /// <summary>
        /// 上传整个目录
        /// </summary>
        /// <param name="fileBytes">要上传文件byte流</param>
        /// <param name="ftpPath">FTP路径</param>
        /// <param name="dirName">要上传的目录名</param>
        /// <param name="filename">文件名</param>
        /// <param name="ftpUser">FTP用户名（匿名为空）</param>
        /// <param name="ftpPassword">FTP登录密码（匿名为空）</param>
        public void UploadDirectory(byte[] fileBytes, string ftpPath, string dirName, string filename, string ftpUser, string ftpPassword)
        {
            if (ftpUser == null)
            {
                ftpUser = string.Empty;
            }

            if (ftpPassword == null)
            {
                ftpPassword = string.Empty;
            }

            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new Exception(string.Format("要上传的文件内容为空"));
            }

            ////检查FTP服务器是否存在该目录,不存在则创建
            if (!this.CheckDirectoryExist(ftpPath, dirName))
            {
                this.MakeDir(ftpPath, dirName);
            }

            this.UpLoadFile(fileBytes, ftpPath + dirName + @"/" + filename, ftpUser, ftpPassword);
        }

        /// <summary>
        /// 新建目录
        /// </summary>
        /// <param name="ftpPath"></param>
        /// <param name="dirName"></param>
        public void MakeDir(string ftpPath, string dirName)
        {
            try
            {
                ////实例化FTP连接
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + dirName));
                ////指定FTP操作类型为创建目录
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                ////获取FTP服务器的响应
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileBytes">要上传的内容</param>
        /// <param name="ftpUri">FTP地址</param>
        /// <param name="ftpUser">登录名</param>
        /// <param name="ftppd">密码</param>
        public void UpLoadFile(byte[] fileBytes, string ftpUri, string ftpUser, string ftppd)
        {
            if (ftpUser == null)
            {
                ftpUser = string.Empty;
            }

            if (ftppd == null)
            {
                ftppd = string.Empty;
            }

            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new Exception(string.Format("要上传的文件内容为空"));
            }

            FtpWebRequest ftpWebRequest = null;
            Stream requestStream = null;

            try
            {
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpUri));
                ftpWebRequest.Credentials = new NetworkCredential(ftpUser, ftppd);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;

                int buffLength = 4096;
                byte[] buff = new byte[buffLength];
                int bytesRead;
                int totalRead = 0;
                requestStream = ftpWebRequest.GetRequestStream();

                MemoryStream mem = new MemoryStream(fileBytes);
                while (true)
                {
                    bytesRead = mem.Read(buff, 0, buff.Length);
                    if (bytesRead == 0)
                        break;
                    totalRead += bytesRead;
                    requestStream.Write(buff, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
            }
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="ftpPath">要检查的目录的上一级目录</param>
        /// <param name="dirName">要检查的目录名</param>
        /// <returns>存在返回true，否则false</returns>
        public bool CheckDirectoryExist(string ftpPath, string dirName)
        {
            bool result = false;

            try
            {
                dirName = dirName.Replace("\\", "/");

                dirName.Split('/');

                ////实例化FTP连接
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath));
                ////指定FTP操作类型为创建目录
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                ////获取FTP服务器的响应
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                StringBuilder str = new StringBuilder();
                string line = sr.ReadLine();
                while (line != null)
                {
                    str.Append(line);
                    str.Append("|");
                    line = sr.ReadLine();
                }

                string[] datas = str.ToString().Split('|');

                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].Contains("<DIR>"))
                    {
                        int index = datas[i].IndexOf("<DIR>");
                        string name = datas[i].Substring(index + 5).Trim();
                        if (name == dirName)
                        {
                            result = true;
                            break;
                        }
                    }
                }

                sr.Dispose();
                response.Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// 发布完成后
        /// </summary>
        public void PublishEnd()
        {
            if (this.OnPublishedEventHandle != null && this.DispatcherEndData != null)
            {
                DispatcherEndEventaArgs ev = new DispatcherEndEventaArgs(this.DispatcherEndData);

                this.OnPublishedEventHandle(this, ev);
            }
        }

        /// <summary>
        /// 获取目录下的详细信息
        /// </summary>
        /// <param name="localDir">本机目录</param>
        /// <returns></returns>
        private static List<List<string>> GetDirDetails(string localDir)
        {
            List<List<string>> infos = new List<List<string>>();

            try
            {
                infos.Add(Directory.GetFiles(localDir).ToList()); // 获取当前目录的文件
                infos.Add(Directory.GetDirectories(localDir).ToList()); // 获取当前目录的目录
                for (int i = 0; i < infos[0].Count; i++)
                {
                    int index = infos[0][i].LastIndexOf(@"\");
                    infos[0][i] = infos[0][i].Substring(index + 1);
                }

                for (int i = 0; i < infos[1].Count; i++)
                {
                    int index = infos[1][i].LastIndexOf(@"\");
                    infos[1][i] = infos[1][i].Substring(index + 1);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return infos;
        }
    }
}
