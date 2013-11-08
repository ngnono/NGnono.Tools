using System;
using NGnono.Framework.ServiceLocation;
using Quartz;
using Quartz.Spi;

namespace NGnono.Tools.Scheduler.Core
{
    public class IocJobFactory : IJobFactory
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly IJobFactory _bottomJobFactory;

        protected IJobFactory BottomJobFactory
        {
            get
            {
                return _bottomJobFactory;
            }
        }

        public IocJobFactory(IServiceLocator serviceLocator, IJobFactory bottomFactory)
        {
            _serviceLocator = serviceLocator;
            _bottomJobFactory = bottomFactory;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (_serviceLocator.IsRegistered(bundle.JobDetail.JobType))
            {
                return (IJob)_serviceLocator.Resolve(bundle.JobDetail.JobType);
            }

            if (BottomJobFactory != null)
            {
                return BottomJobFactory.NewJob(bundle, scheduler);
            }

            throw new ArgumentException(String.Format("不能构建该类型{0}，请检查配置文件", bundle.JobDetail.JobType.Name));
        }

        public void ReturnJob(IJob job)
        {
            //throw new NotImplementedException();
        }
    }
}