using Common.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;

namespace NGnono.Tools.Scheduler.Core
{
    /// <summary>
    /// 计划任务管理
    /// </summary>
    internal class QuartzManager
    {
        /// <summary>
        /// 计划任务
        /// </summary>
        private readonly IScheduler _scheduler;

        private readonly ILog _logger;

        private static QuartzManager _current;
        private static readonly object ObjAsync = new object();

        private QuartzManager()
        {
            _logger = LogManager.GetLogger(GetType());
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler();
        }

        public static QuartzManager Current
        {
            get
            {
                if (_current == null)
                {
                    lock (ObjAsync)
                    {
                        if (_current == null)
                        {
// ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
                            _current = new QuartzManager();
// ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
                        }
                    }
                }

                return _current;
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            _scheduler.Start();
        }

        /// <summary>
        /// 停止
        /// 停止所有的Job执行
        /// </summary>
        public void Stop()
        {
            _scheduler.Shutdown();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            _scheduler.PauseAll();
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Continue()
        {
            _scheduler.ResumeAll();
        }

        /// <summary>
        /// 添加到计划任务
        /// </summary>
        /// <param name="job">执行的Job</param>
        /// <param name="trigger">触发器</param>
        /// <returns>返回执行时间</returns>
        public DateTimeOffset AddScheduleJob(IJobDetail job, ITrigger trigger)
        {
            var startTime = _scheduler.ScheduleJob(job, trigger);
            Start();
            return startTime;
        }

        /// <summary>
        /// 移除Job
        /// </summary>
        /// <param name="jobName">Job名称</param>
        /// <param name="groupName">组名称</param>
        /// <returns>返回执行结果</returns>
        public bool RemoveJob(string jobName, string groupName)
        {
            try
            {
                // 删除job
                var success = _scheduler.DeleteJob(new JobKey(jobName, groupName));
                if (success)
                {
                    _logger.Info(string.Format("计划任务:{0}被成功移除", jobName));
                }

                return success;
            }
            catch (Exception e)
            {
                _logger.Info(string.Format("计划任务:{0}取消失败", jobName));
                _logger.Error(e);

                return false;
            }
        }

        /// <summary>
        /// 获取Job详情
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public IList<ITrigger> GetListJobTrigger(string jobName, string groupName)
        {
            if (IsExitsJob(jobName, groupName))
            {
                return _scheduler.GetTriggersOfJob(new JobKey(jobName, groupName));
            }

            return new List<ITrigger>(0);
        }

        /// <summary>
        /// 验证是否存在Job
        /// </summary>
        /// <param name="jobName">Job名称</param>
        /// <param name="groupName">组名称</param>
        /// <returns>返回结果</returns>
        public bool IsExitsJob(string jobName, string groupName)
        {
            var jobNameList = _scheduler.GetJobDetail(new JobKey(jobName, groupName));

            return jobNameList != null;
        }
    }
}
