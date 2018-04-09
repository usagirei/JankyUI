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

        private JankyNodeContext ResolveContext(XmlDocument xmlDoc, object dc, IDictionary<string, object> resources)
        {
            var ctx = new JankyNodeContext(dc);
            JankyUIManager.Register(ctx);
            ctx.RootNode = NodeHelper<Node>.Instance.Activate(ctx);
            var settings = xmlDoc.SelectSingleNode("/*[local-name() = 'JankyUI']/*[local-name() = 'Resources']");
            if (settings != null)
            {
                foreach (XmlNode setting in settings.ChildNodes)
                {
                    var key = setting.Attributes["key"].Value;
                    var value = setting.Attributes["value"].Value;
                    if (ctx.Resources.ContainsKey(key))
                    {
                        ctx.Resources[key] = ctx.Resources[key] + "|" + value;
                    }
                    else
                    {
                        ctx.Resources[key] = value;
                    }
                }

                if (resources != null)
                {
                    // Replace with Overrides
                    foreach (var kvp in resources)
                    {
                        if (ctx.Resources.ContainsKey(kvp.Key))
                            ctx.Resources.Remove(kvp.Key);
                    }

                    foreach (var kvp in resources)
                    {
                        var key = kvp.Key;
                        var value = kvp.Value;
                        if (ctx.Resources.ContainsKey(key) && ctx.Resources[key] is string && value is string)
                        {
                            ctx.Resources[key] = ctx.Resources[key] + "|" + value;
                        }
                        else
                        {
                            ctx.Resources[key] = value;
                        }
                    }
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
                if (childNode.NodeType == XmlNodeType.Comment)
                    continue;

                var child = ResolveNodeRecursive(context, childNode);
                node.Children.Add(child);
            }

            return node;
        }


        public IJankyContext CreateJankyUI(string xml, object viewModel, IDictionary<string, object> resources)
        {
            using (var xmlStream = new MemoryStream())
            using (var sw = new StreamWriter(xmlStream))
            {
                sw.Write(xml);
                sw.Flush();
                xmlStream.Position = 0;
                return CreateJankyUI(xmlStream, viewModel, resources);
            }
        }

        public IJankyContext CreateJankyUI(Stream xmlStream, object viewModel, IDictionary<string, object> resources)
        {
            using (var xmlReader = XmlReader.Create(xmlStream))
            {
                var doc = new XmlDocument();
                doc.Load(xmlReader);

                return ResolveContext(doc, viewModel, resources);
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
