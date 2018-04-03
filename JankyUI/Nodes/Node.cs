using System;
using System.Collections.Generic;
using UnityEngine;

namespace JankyUI.Nodes
{
    internal abstract class Node
    {
        public List<Node> Children { get; set; }

        public Node ParentNode { get; set; }

        private object _dataContext;
        public virtual object DataContext
        {
            get
            {
                // Try Caching DataContext if Invalidated
                if (_dataContext == null || !object.ReferenceEquals(_dataContext, ParentNode?.DataContext))
                {
                    _dataContext = ParentNode?.DataContext;
                }
                return _dataContext;
            }
        }

        private GUISkin _guiSkin;
        public virtual GUISkin Skin
        {
            get
            {
                // Try Caching Skin if Invalidated
                if (_guiSkin == null || !object.ReferenceEquals(_guiSkin, ParentNode?.Skin))
                {
                    _guiSkin = ParentNode?.Skin;
                }
                // Return Default if none set;
                return _guiSkin ?? GUI.skin;
            }
        }

        protected virtual void OnGUI()
        {
        }
    }
}
