using System;
using JankyUI.Attributes;
using JankyUI.Nodes.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.EventArgs
{
    public class JankyEventArgs : System.EventArgs
    {
        public int WindowID { get; }
        public string Control { get; }

        public JankyEventArgs(int windowID, string controlName)
        {
            WindowID = windowID;
            Control = controlName;
        }
    }
}
