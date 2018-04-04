using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using JankyUI.Nodes;

namespace JankyUI
{
    public class JankyUIGenerator
    {
        private static JankyUIGenerator _default;
        private Dictionary<string, NodeHelper> _nodes;

        public static JankyUIGenerator Default
        {
            get
            {
                return _default ?? (_default = new JankyUIGenerator(typeof(JankyUIGenerator).Assembly));
            }
        }

        public JankyUIGenerator(params Assembly[] nodeSource)
        {
            _nodes = new Dictionary<string, NodeHelper>();
            foreach (var ass in nodeSource)
                LoadJankyNodes(ass);
        }

        static int FindElementIndex(XmlElement element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlElement && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

        static string FindXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        builder.Insert(0, "/" + node.Name + "[" + index + "]");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString();
                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        private JankyNodeContext ResolveContext(XmlDocument xmlDoc, object dc = null)
        {
            var ctx = new JankyNodeContext(dc);
            ctx.RootNode = NodeHelper<Node>.Instance.Activate(ctx);
            var settings = xmlDoc.SelectSingleNode("/*[local-name() = 'JankyUI']/*[local-name() = 'Resources']");
            if (settings != null)
            {
                foreach (XmlNode setting in settings.ChildNodes)
                {
                    var key = setting.Attributes["key"].Value;
                    var value = setting.Attributes["value"].Value;
                    ctx.Resources[key] = value;
                }
            }
            var window = xmlDoc.SelectSingleNode("/*[local-name() = 'JankyUI']/*[local-name() = 'Window']");
            var node = ResolveNodeRecursive(ctx, window);
            ctx.RootNode.Children.Add(node);
            return ctx;
        }

        private Node ResolveNodeRecursive(JankyNodeContext context, XmlNode xmlNode)
        {
            var nodeName = xmlNode.LocalName;
            var nodeInfo = _nodes[nodeName];
            var props = xmlNode.Attributes
                .OfType<XmlAttribute>()
                .ToDictionary(x => x.LocalName, x => x.Value);
            var node = nodeInfo.Activate(context, props);

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                var child = ResolveNodeRecursive(context, childNode);
                node.Children.Add(child);
            }

            return node;
        }


        public IJankyContext CreateJankyUI(string xml, object viewModel)
        {
            using (var xmlStream = new MemoryStream())
            using (var sw = new StreamWriter(xmlStream))
            {
                sw.Write(xml);
                sw.Flush();
                xmlStream.Position = 0;
                return CreateJankyUI(xmlStream, viewModel);
            }
        }

        public IJankyContext CreateJankyUI(Stream xmlStream, object viewModel)
        {
            using (var xmlReader = XmlReader.Create(xmlStream))
            {
                var doc = new XmlDocument();
                doc.Load(xmlReader);

                return ResolveContext(doc, viewModel);
            }

        }

        public void ValidateJankyXml(string xml)
        {
            using (var xmlStream = new MemoryStream())
            using (var sw = new StreamWriter(xmlStream))
            {
                sw.Write(xml);
                sw.Flush();
                xmlStream.Position = 0;
                ValidateJankyXml(xmlStream);
            }
        }

        // TODO: Broken in some Mono Version
        public void ValidateJankyXml(Stream xmlStream)
        {
            using (var schemaStream = typeof(JankyUIGenerator).Assembly.GetManifestResourceStream("JankyUI.Schema.xsd"))
            using (var tr = new StreamReader(schemaStream, Encoding.UTF8))
            {
                XmlReader schemaDocument = XmlReader.Create(schemaStream);

                var settings = new XmlReaderSettings();
                settings.Schemas.Add("janky://Schema/v1", schemaDocument);
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationType = ValidationType.Schema;

                using (var xmlReader = XmlReader.Create(xmlStream, settings))
                {
                    var doc = new XmlDocument();
                    doc.Load(xmlReader);
                }
            }
        }

        public void LoadJankyNodes(Assembly ass)
        {
            var nodes = ass.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Node)))
                .Select(t =>
                {
                    return (NodeHelper)typeof(NodeHelper<>).MakeGenericType(t)
                                                            .GetProperty("Instance")
                                                            .GetValue(null, null);
                });

            foreach (var node in nodes)
            {
                foreach (var alias in node.Tags)
                {
                    _nodes.Add(alias.Name, node);
                }
            }
        }
    }
}
