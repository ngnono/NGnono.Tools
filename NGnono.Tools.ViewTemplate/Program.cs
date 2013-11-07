using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using NGnono.Tools.ViewTemplate.Core;

namespace NGnono.Tools.ViewTemplate
{
    /// <summary>
    /// 默认ViewModel
    /// </summary>
    public class DefaultViewModel
    {
        /// <summary>
        /// HTML元素
        /// </summary>
        public IList<HtmlUnit> HtmlUnits { get; set; }
    }

    /// <summary>
    /// HTML单元
    /// </summary>
    public class HtmlUnit
    {
        /// <summary>
        /// 图片区域
        /// </summary>
        public HtmlElement Image { get; set; }

        /// <summary>
        /// 标题文本区域
        /// </summary>
        public HtmlElement Title { get; set; }

        /// <summary>
        /// 副标题区域
        /// </summary>
        public HtmlElement SubTitle { get; set; }

        /// <summary>
        /// 扩展元素
        /// </summary>
        public IDictionary<string, string> ExpandElement { get; set; }

        /// <summary>
        /// 子HTML单元
        /// 用于子节点是列表
        /// </summary>
        public IList<HtmlElement> ChildHtmlUnit { get; set; }
    }

    /// <summary>
    /// HtmlUnit单元
    /// </summary>
    public class HtmlElement
    {
        /// <summary>
        /// 初始化HtmlElement
        /// </summary>
        public HtmlElement()
        {
            this.Target = "_blank";
        }

        /// <summary>
        /// 打开方式
        /// 链接打开方式
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// 标题
        /// 页面显示文字
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 链接地址
        /// URL链接地址
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// 图片地址
        /// 用来存放图片地址
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 市场价
        /// </summary>
        public string MacketPrice { get; set; }

        /// <summary>
        /// 销售价
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 渠道价
        /// </summary>
        public string ChannelPrice { get; set; }

        /// <summary>
        /// 其它价格(备用)
        /// </summary>
        public string OtherPrice { get; set; }

        /// <summary>
        /// 加粗
        /// 文字是否加粗
        /// </summary>
        public bool IsBold { get; set; }

        /// <summary>
        /// 斜体
        /// 文字是否斜体
        /// </summary>
        public bool IsItalic { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// 图片提示
        /// 用于图片加载前提示文字
        /// </summary>
        public string Alt { get; set; }

        /// <summary>
        /// 上
        /// </summary>
        public string Top { get; set; }

        /// <summary>
        /// 左
        /// </summary>
        public string Left { get; set; }

        /// <summary>
        /// 下
        /// </summary>
        public string Bottom { get; set; }

        /// <summary>
        /// 右
        /// </summary>
        public string Right { get; set; }

        /// <summary>
        /// 宽
        /// </summary>
        public string Width { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        public string Height { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 页面ID
        /// </summary>
        public string PageID { get; set; }

        /// <summary>
        /// 区块ID
        /// </summary>
        public string TableID { get; set; }

        /// <summary>
        /// 子元素
        /// </summary>
        public HtmlElement SubInfo { get; set; }

        /// <summary>
        /// 扩展元素
        /// </summary>
        public IDictionary<string, string> ExpandElement { get; set; }
    }

    /// <summary>
    /// 公共扩展类
    /// </summary>
    public static class CommonUtily
    {
        /// <summary>
        /// 转换到JSON
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public static string ToJson(this object @object)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(@object);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var viewEngineFactory = new DefaultViewEngineFactory();
            var viewEngine = viewEngineFactory.Create(TemplateType.Razor);

            var templates = GetTemplates();
            var model = GetModel();

            var c = viewEngine.Parse(templates.FirstOrDefault().Value, model);

            Write("abcd.html", c);
        }

        private static void Write(string n, string c)
        {
            var p = AppDomain.CurrentDomain.BaseDirectory + "/" + n;

            File.WriteAllText(p, c, Encoding.Default);
        }

        private static Dictionary<string, string> GetTemplates()
        {
            var tPath = ConfigurationManager.AppSettings["defTemplates"];

            var files = new List<string>();
            //foreach (var f in tPath)
            //{
            files.AddRange(System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + tPath));
            //}

            var dic = new Dictionary<string, string>(files.Count);

            Func<string, string> getName = s =>
                {
                    if (String.IsNullOrWhiteSpace(s))
                    {
                        return null;
                    }

                    var t = s.LastIndexOf("/", System.StringComparison.Ordinal);

                    return s.Substring(t, s.Length - t);
                };

            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                dic.Add(getName(file), content);
            }

            return dic;
        }

        private static DefaultViewModel GetModel()
        {
            DefaultViewModel model = new DefaultViewModel();

            model.HtmlUnits = new List<HtmlUnit>();

            #region[unitA]
            HtmlUnit unitA = new HtmlUnit()
            {
                ChildHtmlUnit = new List<HtmlElement>(),
                Image = new HtmlElement(),
                Title = new HtmlElement(),
                SubTitle = new HtmlElement(),
                ExpandElement = new Dictionary<string, string>()
            };

            unitA.Title.Title = "运动服装";
            #region[CMS 第二版]
            unitA.ExpandElement.Add("begintime", DateTime.Now.ToString());
            unitA.ExpandElement.Add("endtime", DateTime.Now.ToString());
            #endregion
            unitA.ChildHtmlUnit.Add(new HtmlElement
            {
                Title = "T恤", //标题
                Href = "http://www.baidu.com", //连接
            });
            unitA.ChildHtmlUnit.Add(new HtmlElement
            {
                Title = "Polo衫",
                Href = "http://www.baidu.com",
            });
            unitA.ChildHtmlUnit.Add(new HtmlElement
            {
                Title = "卫衣",
                Href = "http://www.baidu.com",
            });
            unitA.ChildHtmlUnit.Add(new HtmlElement
            {
                Title = "衬衫",
                Href = "http://www.baidu.com",
            });
            unitA.ChildHtmlUnit.Add(new HtmlElement
            {
                Title = "背心/吊带",
                Href = "http://www.baidu.com",
            });
            unitA.ChildHtmlUnit.Add(new HtmlElement
            {
                Title = "长裤",
                Href = "http://www.baidu.com",
            });
            #endregion
            model.HtmlUnits.Add(unitA);

            return model;
        }


    }
}
