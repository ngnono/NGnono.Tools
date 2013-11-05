using NGnono.Tools.Dispatcher.Core.Contract;
using NGnono.Tools.Dispatcher.Core.Impl;

namespace NGnono.Tools.Dispatcher.Core
{
    /// <summary>
    /// 默认分发器工厂
    /// </summary>
    public class DefaultDispatcherFactory : IDispatcherFactory
    {
        /// <summary>
        /// The dispatcher.
        /// </summary>
        private readonly IDispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDispatcherFactory"/> class.
        /// </summary>
        public DefaultDispatcherFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDispatcherFactory"/> class.
        /// </summary>
        /// <param name="dispatcher">
        /// The dispatcher.
        /// </param>
        public DefaultDispatcherFactory(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// 生产分发器
        /// </summary>
        /// <param name="type">分发器类型</param>
        /// <returns>IDispatcher</returns>
        public IDispatcher Create(string type)
        {
            return _dispatcher ?? new FileShareDispatcher();
        }
    }
}