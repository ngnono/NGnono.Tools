namespace NGnono.Tools.ViewTemplate.Core
{
    /// <summary>
    /// 处理器接口，模板和数据结合产生HTML片段
    /// </summary>
    public interface IViewEngine
    {
        /// <summary>
        /// 处理模板类型
        /// </summary>
        string ProcessTemplateType { get; }

        /// <summary>
        /// 视图解析
        /// </summary>
        /// <param name="template">模板</param>
        /// <param name="model">数据</param>
        /// <returns>解析后</returns>
        string Parse(string template, dynamic model);
    }
}