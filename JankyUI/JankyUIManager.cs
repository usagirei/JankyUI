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
    public static class JankyUIManager
    {
        internal static int WindowIDCounter = 0;

        public static Dictionary<int, IJankyContext> Windows { get; }

        static JankyUIManager()
        {
            Windows = new Dictionary<int, IJankyContext>();
        }

        internal static void Register(JankyNodeContext ctx)
        {
            ctx.WindowID = WindowIDCounter++;
            Windows[ctx.WindowID] = ctx;
        }
    }
}
