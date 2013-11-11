using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using NGnono.Framework.ServiceLocation;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace NGnono.Tools.Scheduler.Core
{
    public class Run
    {
        public Run()
        {
            var log = LogManager.GetLogger(GetType());
            this.Action = (c) =>
                {
                    log.Info("RUN.run.run.run.run");

                    log.Info(c.ToString());
                };
        }

        public Action<IJobExecutionContext> Action;
    }


    public class StdQuartzJob : IJob
    {
        private readonly Action<IJobExecutionContext> _action;

        public StdQuartzJob(Run run)
        {
            _action = run.Action;
        }

        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.Info("StdQuartzJob 任务开始运行");

                if (_action != null)
                {
                    _action(context);
                }

                Logger.Info("StdQuartzJob任务运行结束");
            }
            catch (Exception ex)
            {
                Logger.Error("StdQuartzJob 运行异常", ex);
            }
        }
    }

    /// <summary>
    /// 计划任务类型
    /// </summary>
    [DataContract]
    public enum TaskType
    {
        /// <summary>
        /// 一次的
        /// </summary>
        [EnumMember]
        [Description("一次性计划")]
        Once = 1,

        /// <summary>
        /// 周期性的
        /// </summary>
        [EnumMember]
        [Description("周期性计划")]
        Periodic = 2
    }

    /// <summary>
    /// Job名称
    /// </summary>
    [DataContract]
    public enum ScheduleJobAlias
    {
        [EnumMember]
        None = 0,
    }

    /// <summary>
    /// 请求信息
    /// </summary>
    [DataContract]
    public class SchedulerRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerRequest"/> class. 
        /// 计划任务请求信息
        /// </summary>
        public SchedulerRequest()
        {
            this.TaskType = TaskType.Periodic;
            this.Job = ScheduleJobAlias.None;
            this.HandleInfo = new HandleInfo
            {
                HttpMethod = "POST",
                QueryString = new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// 计划任务名称
        /// </summary>
        [DataMember]
        public string SchedulerName { get; set; }

        /// <summary>
        /// 第二名称
        /// </summary>
        [DataMember]
        public string SecondName { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Descrtion { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [DataMember]
        public object Data { get; set; }

        /// <summary>
        /// 时间表达式
        /// </summary>
        [DataMember]
        public string TimeExpression { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [DataMember]
        public TaskType TaskType { get; set; }

        /// <summary>
        /// 执行的Job
        /// </summary>
        [DataMember]
        public ScheduleJobAlias Job { get; set; }

        /// <summary>
        /// 处理信息
        /// </summary>
        [DataMember]
        public HandleInfo HandleInfo { get; set; }

        /// <summary>
        /// 扩展数据
        /// </summary>
        [DataMember]
        public IDictionary<object, object> ExpendData { get; set; }
    }

    /// <summary>
    /// 执行信息
    /// </summary>
    [DataContract]
    public class HandleInfo
    {
        /// <summary>
        /// 请求的Uri
        /// </summary>
        [DataMember]
        public Uri RequestUri { get; set; }

        /// <summary>
        /// 请求方式默认为Post
        /// </summary>
        [DataMember]
        public string HttpMethod { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        [DataMember]
        public IDictionary<string, object> QueryString { get; set; }
    }

    /// <summary>
    /// 计划任务返回结果
    /// </summary>
    [DataContract]
    public class SchedulerResponse
    {
        /// <summary>
        /// 执行时间
        /// </summary>
        [DataMember]
        public DateTime ExecuteTime { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置数据
        /// </summary>
        [DataMember]
        public string Data { get; set; }

        /// <summary>
        /// 返回请求状态
        /// </summary>
        [DataMember]
        public bool Success { get; set; }

        /// <summary>
        /// 获取Json数据
        /// </summary>
        /// <returns></returns>
        public string Json()
        {
            return this.Message;
        }
    }

    #region wcf

    /// <summary>
    /// ISchedulerService
    /// </summary>
    [ServiceContract]
    public interface ISchedulerService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        [OperationContract]
        SchedulerResponse Add(SchedulerRequest request);

        /// <summary>
        ///  修改
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        [OperationContract]
        SchedulerResponse Modify(SchedulerRequest request);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        [OperationContract]
        SchedulerResponse Remove(SchedulerRequest request);

        /// <summary>
        /// 获取下次执行时间
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        [OperationContract]
        SchedulerResponse GetNextExecuteTime(SchedulerRequest request);
    }

    public class SchedulerService : ISchedulerService
    {
        private readonly ISchedulerContract _scheduler;

        public SchedulerService(ISchedulerContract scheduler)
        {
            _scheduler = scheduler;
        }

        private static SchedulerResponse Convert(SchedulerResult result)
        {
            var response = new SchedulerResponse
                {
                    Data = result.Data,
                    ExecuteTime = result.ExecuteTime,
                    Message = result.Message,
                    Success = result.Success
                };

            return response;
        }

        public SchedulerResponse Add(SchedulerRequest request)
        {
            return Convert(_scheduler.Add(request));
        }

        public SchedulerResponse Modify(SchedulerRequest request)
        {
            return Convert(_scheduler.Modify(request));
        }

        public SchedulerResponse Remove(SchedulerRequest request)
        {
            return Convert(_scheduler.Remove(request));
        }

        public SchedulerResponse GetNextExecuteTime(SchedulerRequest request)
        {
            return Convert(_scheduler.GetNextExecuteTime(request));
        }
    }

    #endregion

    /// <summary>
    /// 执行后返回结果
    /// </summary>
    public class SchedulerResult
    {
        /// <summary>
        /// 执行时间
        /// </summary>
        [DataMember]
        public DateTime ExecuteTime { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置数据
        /// </summary>
        [DataMember]
        public string Data { get; set; }

        /// <summary>
        /// 返回请求状态
        /// </summary>
        [DataMember]
        public bool Success { get; set; }
    }

    public interface ISchedulerContract
    {
        /// <summary>
        /// 添加计划
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        SchedulerResult Add(SchedulerRequest request);

        /// <summary>
        /// 修改计划
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        SchedulerResult Modify(SchedulerRequest request);

        /// <summary>
        /// 称除计划
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        SchedulerResult Remove(SchedulerRequest request);

        /// <summary>
        ///  获取下次执行时间
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>返回结果</returns>
        SchedulerResult GetNextExecuteTime(SchedulerRequest request);
    }

    public abstract class SchedulerBase : ISchedulerContract
    {
        /// <summary>
        /// 组名格式
        /// </summary>
        protected const string DefaultGroupName = "GroupName_Ngnono.Tools.Scheduler.Tasks";

        /// <summary>
        /// 作业名称格式
        /// </summary>
        protected const string JobNameFormat = "JobName_{0}_{1}";

        /// <summary>
        /// 获取jobname
        /// </summary>
        /// <param name="primaryName"></param>
        /// <param name="secondaryName"></param>
        /// <returns></returns>
        protected string GetJobName(string primaryName, string secondaryName)
        {
            return String.Format(JobNameFormat, primaryName, secondaryName);
        }

        public abstract SchedulerResult Add(SchedulerRequest request);
        public abstract SchedulerResult Modify(SchedulerRequest request);
        public abstract SchedulerResult Remove(SchedulerRequest request);
        public abstract SchedulerResult GetNextExecuteTime(SchedulerRequest request);
    }

    public class Quartz4NetSchedulerImpl : SchedulerBase
    {
        private readonly QuartzManager _manager;

        public Quartz4NetSchedulerImpl(QuartzManager manager)
        {
            _manager = manager;
        }

        public override SchedulerResult Add(SchedulerRequest request)
        {
            throw new NotImplementedException();
        }

        public override SchedulerResult Modify(SchedulerRequest request)
        {
            throw new NotImplementedException();
        }

        public override SchedulerResult Remove(SchedulerRequest request)
        {
            throw new NotImplementedException();
        }

        public override SchedulerResult GetNextExecuteTime(SchedulerRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
