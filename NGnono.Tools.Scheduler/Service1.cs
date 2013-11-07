using Common.Logging;
using NGnono.Tools.Scheduler.Core;
using Quartz;
using System;
using System.ServiceProcess;

namespace NGnono.Tools.Scheduler
{
    partial class QuartzService : ServiceBase
    {
        private readonly ILog _logger;
        private readonly QuartzManager _quartzManager;

        public QuartzService()
        {
            InitializeComponent();
            _logger = LogManager.GetLogger(GetType());
            _quartzManager = QuartzManager.Current;
        }

        public void ConsoleDebug()
        {
            _logger.Info("已经启动");
            _logger.Info("...");
            OnStart(null);
            Console.Read();
        }

        protected override void OnStart(string[] args)
        {
            _quartzManager.Start();
            _logger.Info("Quartz服务成功启动");
        }

        protected override void OnStop()
        {
            _quartzManager.Stop();
            _logger.Info("Quartz服务成功终止");
        }

        protected override void OnPause()
        {
            _quartzManager.Pause();
            _logger.Info("Quartz服务成功暂停");
        }

        protected override void OnContinue()
        {
            _quartzManager.Continue();
            _logger.Info("Quartz服务成功恢复");
        }
    }
}
