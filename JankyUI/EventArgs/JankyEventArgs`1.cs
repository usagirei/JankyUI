using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using JankyUI.Enums;
using UnityEngine;

namespace JankyUI.EventArgs
{

    public class JankyEventArgs<T> : JankyEventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public JankyEventArgs(int windowID, string name, T oldValue, T newValue) : base(windowID, name)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
