using NGnono.Tools.ViewTemplate.Core;

namespace NGnono.Tools.ViewTemplate
{
    /// <summary>
    /// 创建视图引擎 工厂
    /// </summary>
    public interface IViewEngineFactory
    {
        /// <summary>
        /// 创建视图引擎解析器
        /// </summary>
        /// <param name="type">具体视图引擎参数</param>
        /// <returns>IViewEngine</returns>
        IViewEngine Create(string type);
    }
}