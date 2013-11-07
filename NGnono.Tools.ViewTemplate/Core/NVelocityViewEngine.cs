using System.IO;
using Commons.Collections;
using NVelocity;
using NVelocity.App;

namespace NGnono.Tools.ViewTemplate.Core
{
    /// <summary>
    /// The n velocity view engine.
    /// </summary>
    public class NVelocityViewEngine : IViewEngine
    {
        /// <summary>
        /// The velocity engine.
        /// </summary>
        private static readonly VelocityEngine VelocityEngine = new VelocityEngine();

        /// <summary>
        /// Initializes a new instance of the <see cref="NVelocityViewEngine"/> class.
        /// </summary>
        public NVelocityViewEngine()
        {
            var props = new ExtendedProperties();
            VelocityEngine.Init(props);
        }

        /// <summary>
        /// 处理模板类型
        /// </summary>
        public string ProcessTemplateType
        {
            get
            {
                return TemplateType.Velocity;
            }
        }

        /// <summary>
        /// The parse.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public string Parse(string template, dynamic model)
        {
            var ctx = new VelocityContext();
            ctx.Put("Model", model);
            var writer = new StringWriter();
            VelocityEngine.Evaluate(ctx, writer, null, template);

            return writer.GetStringBuilder().ToString();
        }
    }
}