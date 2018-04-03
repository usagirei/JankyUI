using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
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

        private JankyNodeContext ResolveContext(XmlDocument xmlDoc, object dc = null)
        {
            var ctx = new JankyNodeContext(dc);
            ctx.RootNode = NodeHelper<Node>.Instance.Activate(ctx);
            var first = ResolveNodeRecursive(ctx, xmlDoc.FirstChild);
            ctx.RootNode.Children.Add(first);
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
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            if (doc.FirstChild.LocalName != "Window")
                throw new Exception("First Child must be of Type Window");

            return ResolveContext(doc, viewModel);
        }

        public void LoadJankyNodes(Assembly ass)
        {
            var nodes = ass.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Node)))
                .Select(t =>
                {
                    return (NodeHelper) typeof(NodeHelper<>).MakeGenericType(t)
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
