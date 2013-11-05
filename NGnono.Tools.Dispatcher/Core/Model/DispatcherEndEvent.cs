using System;

namespace NGnono.Tools.Dispatcher.Core.Model
{
    /// <summary>
    /// 发布完成后
    /// </summary>
    public class DispatcherEndEventaArgs : EventArgs
    {
        private readonly object _value;

        /// <summary>
        /// 发布完成后执行的事件
        /// </summary>
        /// <param name="value"></param>
        public DispatcherEndEventaArgs(object value)
        {
            _value = value;
        }

        /// <summary>
        /// URL地址
        /// </summary>
        public object QueryString
        {
            get
            {
                return _value;
            }
        }
    }
}