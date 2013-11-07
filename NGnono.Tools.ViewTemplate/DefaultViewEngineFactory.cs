using System.Collections.Generic;
using System.Linq;
using NGnono.Tools.ViewTemplate.Core;

namespace NGnono.Tools.ViewTemplate
{
    /// <summary>
    /// DefaultViewEngineFactory
    /// </summary>
    public class DefaultViewEngineFactory : IViewEngineFactory
    {
        /// <summary>
        /// viewEngines
        /// </summary>
        private readonly IEnumerable<IViewEngine> _viewEngines;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewEngineFactory"/> class.
        /// </summary>
        public DefaultViewEngineFactory()
        {
            _viewEngines = new List<IViewEngine> { new NVelocityViewEngine(), new RazorViewEngine() };
        }

        /// <summary>
        /// 生产视图引擎
        /// </summary>
        /// <param name="viewEnginetype">视图引擎名称</param>
        /// <returns>IViewEngine</returns>
        public IViewEngine Create(string viewEnginetype)
        {
            if (_viewEngines == null)
            {
                return new RazorViewEngine();
            }

            return
                _viewEngines.FirstOrDefault(ve => System.String.Compare(ve.ProcessTemplateType, viewEnginetype, System.StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
