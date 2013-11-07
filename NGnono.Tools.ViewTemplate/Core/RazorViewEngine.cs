using RazorEngine;

namespace NGnono.Tools.ViewTemplate.Core
{
    /// <summary>
    /// RazorViewEngine
    /// </summary>
    public class RazorViewEngine : IViewEngine
    {
        /// <summary>
        /// 处理模板类型
        /// </summary>
        public string ProcessTemplateType
        {
            get
            {
                return TemplateType.Razor;
            }
        }

        /// <summary>
        /// 将模板和数据解析成string
        /// </summary>
        /// <param name="template">模板</param>
        /// <param name="model">数据</param>
        /// <returns></returns>
        public string Parse(string template, dynamic model)
        {
            return Razor.Parse(template, model);
        }
    }
}
