using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//1.
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace NGnono.Tools.Logging
{
    class Program
    {
        
        static Program()
        {
            //2.log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));
        }

        static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Program));
        
        static void Main(string[] args)
        {
            _log.Debug("debug");
            _log.Info("Info");
            _log.Warn("Warn");
            _log.Error("Error");
            _log.Fatal("Fatal");

            Console.ReadLine();
        }
    }
}
