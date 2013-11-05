using NGnono.Tools.Dispatcher.Core.Contract;

namespace NGnono.Tools.Dispatcher.Core
{
    /// <summary>
    /// 分发器 
    /// </summary>
    public interface IDispatcherFactory
    {
        /// <summary>
        /// 生产分发器
        /// </summary>
        /// <param name="type">分发器类型</param>
        /// <returns>IDispatcher</returns>
        IDispatcher Create(string type);
    }
}
