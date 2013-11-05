using System;
using System.Configuration;

namespace NGnono.Tools.Dispatcher.Core.Cfg
{
    internal class CfgDefine
    {
        public const string Section = "dispatcher";
        public const string Task = "task";
        public const string Target = "target";
        public const string Source = "source";
        public const string Server = "server";
        public const string EndHandle = "endHandle";
        public const string Action = "action";
    }

    internal class DispatcherCfgSection : ConfigurationSection
    {
        #region fields

        private static readonly ConfigurationProperty _tasks = new ConfigurationProperty(null, typeof(TaskElementCollection), new TaskElementCollection(), ConfigurationPropertyOptions.IsDefaultCollection);

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        #endregion

        static DispatcherCfgSection()
        {
            _properties.Add(_tasks);
        }

        #region properties

        [ConfigurationCollection(typeof(TaskElement), AddItemName = CfgDefine.Task, CollectionType = ConfigurationElementCollectionType.BasicMap)]
        public TaskElementCollection Tasks
        {
            get { return (TaskElementCollection)base[_tasks]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion
    }

    internal class TaskElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _sources = new ConfigurationProperty(CfgDefine.Source, typeof(SourceElement), new SourceElement(), ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _targets = new ConfigurationProperty(CfgDefine.Target, typeof(TargetElement), new TargetElement(), ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _handler = new ConfigurationProperty(CfgDefine.EndHandle, typeof(EndHandleElement), new EndHandleElement(), ConfigurationPropertyOptions.None);

        static TaskElement()
        {
            _properties.Add(_name);
            _properties.Add(_sources);
            _properties.Add(_targets);
            _properties.Add(_handler);
        }

        public string Name { get { return (string)base[_name]; } }
        public SourceElement Source { get { return (SourceElement)base[_sources]; } }
        public TargetElement Target { get { return (TargetElement)base[_targets]; } }
        public EndHandleElement EndHandler { get { return (EndHandleElement)base[_handler]; } }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }

    internal class TaskElementCollection : ConfigurationElementCollection
    {
        #region fields

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        #endregion

        public TaskElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        #region Properties

        protected override ConfigurationElement CreateNewElement()
        {
            return new TaskElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TaskElement)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return CfgDefine.Task;
            }
        }

        public TaskElement Get(int index)
        {
            return (TaskElement)base.BaseGet(index);
        }

        public TaskElement Get(string key)
        {
            return (TaskElement)base.BaseGet(key);
        }

        public TaskElement this[int index]
        {
            get
            {
                return (TaskElement)base.BaseGet(index);
            }
        }

        public object[] AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public new TaskElement this[string key]
        {
            get
            {
                return (TaskElement)base.BaseGet(key);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion
    }

    internal class SourceElementCollection : ConfigurationElementCollection
    {
        #region fields

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        #endregion

        public SourceElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        #region Properties

        protected override ConfigurationElement CreateNewElement()
        {
            return new SourceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SourceElement)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return CfgDefine.Source;
            }
        }

        public SourceElement Get(int index)
        {
            return (SourceElement)base.BaseGet(index);
        }

        public SourceElement Get(string key)
        {
            return (SourceElement)base.BaseGet(key);
        }

        public SourceElement this[int index]
        {
            get
            {
                return (SourceElement)base.BaseGet(index);
            }
        }

        public object[] AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public new SourceElement this[string key]
        {
            get
            {
                return (SourceElement)base.BaseGet(key);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion
    }

    internal class SourceElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _path = new ConfigurationProperty("path", typeof(string), String.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static SourceElement()
        {
            _properties.Add(_name);
            _properties.Add(_path);
        }

        public string Name { get { return (string)base[_name]; } }
        public string Path { get { return (string)base[_path]; } }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }

    internal class TargetElementCollection : ConfigurationElementCollection
    {
        #region fields

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        #endregion

        public TargetElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        #region Properties

        protected override ConfigurationElement CreateNewElement()
        {
            return new TargetElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TargetElement)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return CfgDefine.Target;
            }
        }

        public TargetElement Get(int index)
        {
            return (TargetElement)base.BaseGet(index);
        }

        public TargetElement Get(string key)
        {
            return (TargetElement)base.BaseGet(key);
        }

        public TargetElement this[int index]
        {
            get
            {
                return (TargetElement)base.BaseGet(index);
            }
        }

        public object[] AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public new TargetElement this[string key]
        {
            get
            {
                return (TargetElement)base.BaseGet(key);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion
    }

    internal class TargetElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _path = new ConfigurationProperty("path", typeof(string), String.Empty);
        private static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _type = new ConfigurationProperty("type", typeof(string), String.Empty);
        private static readonly ConfigurationProperty _server = new ConfigurationProperty(null, typeof(ServerElementCollection), new ServerElementCollection(), ConfigurationPropertyOptions.IsDefaultCollection);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static TargetElement()
        {
            _properties.Add(_path);
            _properties.Add(_name);
            _properties.Add(_type);
            _properties.Add(_server);
        }

        public string Name { get { return (string)base[_name]; } }
        public string Path { get { return (string)base[_path]; } }
        public string Type { get { return (string)base[_type]; } }
        [ConfigurationCollection(typeof(ServerElement), AddItemName = CfgDefine.Server, CollectionType = ConfigurationElementCollectionType.BasicMap)]
        public ServerElementCollection Server { get { return (ServerElementCollection)base[_server]; } }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }

    internal class ServerElementCollection : ConfigurationElementCollection
    {
        #region fields

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        #endregion

        public ServerElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        #region Properties

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return CfgDefine.Server;
            }
        }

        public ServerElement Get(int index)
        {
            return (ServerElement)base.BaseGet(index);
        }

        public ServerElement Get(string key)
        {
            return (ServerElement)base.BaseGet(key);
        }

        public ServerElement this[int index]
        {
            get
            {
                return (ServerElement)base.BaseGet(index);
            }
        }

        public object[] AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public new ServerElement this[string key]
        {
            get
            {
                return (ServerElement)base.BaseGet(key);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion
    }

    internal class ServerElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _path = new ConfigurationProperty("path", typeof(string), String.Empty);
        private static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _type = new ConfigurationProperty("type", typeof(string), String.Empty);

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static ServerElement()
        {
            _properties.Add(_path);
            _properties.Add(_name);
            _properties.Add(_type);
        }

        public string Name { get { return (string)base[_name]; } }
        public string Path { get { return (string)base[_path]; } }
        public string Type { get { return (string)base[_type]; } }


        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }

    internal class EndHandleElementCollection : ConfigurationElementCollection
    {
        #region fields

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        #endregion

        public EndHandleElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        #region Properties

        protected override ConfigurationElement CreateNewElement()
        {
            return new EndHandleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EndHandleElement)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return CfgDefine.EndHandle;
            }
        }

        public EndHandleElement Get(int index)
        {
            return (EndHandleElement)base.BaseGet(index);
        }

        public EndHandleElement Get(string key)
        {
            return (EndHandleElement)base.BaseGet(key);
        }

        public EndHandleElement this[int index]
        {
            get
            {
                return (EndHandleElement)base.BaseGet(index);
            }
        }

        public object[] AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public new EndHandleElement this[string key]
        {
            get
            {
                return (EndHandleElement)base.BaseGet(key);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion
    }

    internal class EndHandleElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _type = new ConfigurationProperty("type", typeof(string), String.Empty);
        private static readonly ConfigurationProperty _action = new ConfigurationProperty(null, typeof(ActionElementCollection), new ActionElementCollection(), ConfigurationPropertyOptions.IsDefaultCollection);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static EndHandleElement()
        {
            _properties.Add(_name);
            _properties.Add(_type);
            _properties.Add(_action);
        }

        public string Name { get { return (string)base[_name]; } }
        public string Type { get { return (string)base[_type]; } }
        public ActionElementCollection Actions { get { return (ActionElementCollection)base[_action]; } }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }

    internal class ActionElementCollection : ConfigurationElementCollection
    {
        #region fields

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        #endregion

        public ActionElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        #region Properties

        protected override ConfigurationElement CreateNewElement()
        {
            return new ActionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ActionElement)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return CfgDefine.Action;
            }
        }

        public ActionElement Get(int index)
        {
            return (ActionElement)base.BaseGet(index);
        }

        public ActionElement Get(string key)
        {
            return (ActionElement)base.BaseGet(key);
        }

        public ActionElement this[int index]
        {
            get
            {
                return (ActionElement)base.BaseGet(index);
            }
        }

        public object[] AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public new ActionElement this[string key]
        {
            get
            {
                return (ActionElement)base.BaseGet(key);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion
    }

    internal class ActionElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _path = new ConfigurationProperty("path", typeof(CdataElement), new CdataElement());

        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static ActionElement()
        {
            _properties.Add(_name);
            _properties.Add(_path);
        }

        public string Name { get { return (string)base[_name]; } }
        public string Path
        {
            get
            {
                var cdata = (CdataElement)base[_path];
                return cdata == null ? null : cdata.Text;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }

    internal class CdataElement : ConfigurationElement
    {
        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            Text = reader.ReadElementContentAs(typeof(string), null) as string;
        }

        protected override bool SerializeElement(System.Xml.XmlWriter writer, bool serializeCollectionKey)
        {
            if (writer != null)
                writer.WriteCData(Text);

            return true;
        }

        [ConfigurationProperty("data", IsRequired = false)]
        public string Text
        {
            get { return this["data"].ToString(); }
            set { this["data"] = value; }
        }
    }
}
