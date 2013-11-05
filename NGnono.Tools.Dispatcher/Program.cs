using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NGnono.Tools.Dispatcher.Core;
using NGnono.Tools.Dispatcher.Core.Cfg;

namespace NGnono.Tools.Dispatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("begin: " + DateTime.Now.ToString("HH:mm:ss.fff"));

            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            webClient.Headers.Add("Accept", "N/A");
            Func<Stream, Encoding, string> getResponse = (stream, encoding) =>
            {
                var reader = new StreamReader(stream, encoding);

                var readbuffer = new char[256];
                var n = reader.Read(readbuffer, 0, 256);
                var sb = new StringBuilder();

                while (n > 0)
                {
                    var str = new String(readbuffer, 0, n);
                    sb.Append(str);
                    n = reader.Read(readbuffer, 0, 256);
                }

                return sb.ToString();
            };

            Func<string, string> replace = s => s.Replace("\r", String.Empty).Replace("\n", String.Empty).Trim();

            try
            {
                var cfg = (DispatcherCfgSection)ConfigurationManager.GetSection(CfgDefine.Section);

                var dispatcher = new DefaultDispatcherFactory().Create(null);

                foreach (TaskElement t in cfg.Tasks)
                {
                    Console.WriteLine("----------------------------------- ");
                    Console.WriteLine("source file: " + t.Source.Path);
                    var c = File.ReadAllText(t.Source.Path, Encoding.Default);
                    var serverList = new List<Uri>();
                    foreach (ServerElement s in t.Target.Server)
                    {
                        Console.WriteLine("target file: " + s.Path);
                        serverList.Add(new Uri(s.Path));
                    }

                    if (t.EndHandler != null)
                    {
                        foreach (ActionElement a in t.EndHandler.Actions)
                        {
                            dispatcher.DispatcherEndData = "OK";
                            ActionElement a1 = a;
                            dispatcher.OnPublishedEventHandle += (s, e) =>
                            {
                                //请求一个地址
                                if (webClient != null)
                                {
                                    Console.WriteLine(a1.Path);

                                    var strm = webClient.OpenRead(replace(a1.Path));

                                    Console.WriteLine(getResponse(strm, Encoding.UTF8));
                                }
                            };
                        }
                    }

                    Console.WriteLine("begin dispatcher: " + DateTime.Now.ToString("HH:mm:ss.fff"));
                    dispatcher.Dispatch(c, serverList, Encoding.Default);
                    Console.WriteLine("end dispatcher: " + DateTime.Now.ToString("HH:mm:ss.fff"));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {

                webClient.Dispose();

            }


            Console.WriteLine("end: " + DateTime.Now.ToString("HH:mm:ss.fff"));
            Console.ReadLine();
        }
    }
}
