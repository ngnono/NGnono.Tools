using System;
using System.Collections.Generic;
using System.Text;
using NGnono.Tools.Dispatcher.Core.Model;

namespace NGnono.Tools.Dispatcher.Core.Contract
{
    /// <summary>
    /// 分发器
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// 分发完成后的参数
        /// </summary>
        object DispatcherEndData { get; set; }

        /// <summary>
        /// 发布完成后执行的事件
        /// </summary>
        event DispatcherEndHandle OnPublishedEventHandle;

        /// <summary>
        /// 发布完成后
        /// </summary>
        void PublishEnd();

        /// <summary>
        /// Gets the dispatcher scheme.
        /// </summary>
        DispatcherSchemes DispatcherScheme { get; }

        /// <summary>
        /// 分发
        /// </summary>
        /// <param name="content">要分发的内容</param>
        /// <param name="uris">分发目标uri</param>
        void Dispatch(string content, IEnumerable<Uri> uris);

        /// <summary>
        /// 分发
        /// </summary>
        /// <param name="content">要分发的内容</param>
        /// <param name="uris">分发目标uri</param>
        /// <param name="encoding"></param>
        void Dispatch(string content, IEnumerable<Uri> uris, Encoding encoding);
    }
}