using System;
using Common.Logging;
using Quartz;
using Quartz.Impl;
using System.ServiceProcess;

namespace NGnono.Tools.Scheduler
{
    partial class QuartzService : ServiceBase
    {
        private readonly ILog _logger;
        private readonly IScheduler _scheduler;

        public QuartzService()
        {
            InitializeComponent();
            _logger = LogManager.GetLogger(GetType());
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler();
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
            _scheduler.Start();
            _logger.Info("Quartz服务成功启动");
        }

        protected override void OnStop()
        {
            _scheduler.Shutdown();
            _logger.Info("Quartz服务成功终止");
        }

        protected override void OnPause()
        {
            _scheduler.PauseAll();
        }

        protected override void OnContinue()
        {
            _scheduler.ResumeAll();
        }
    }
}
