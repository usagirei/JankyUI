using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Window")]
    [JankyProperty("id", nameof(ID))]
    [JankyProperty("mouseEnter", nameof(MouseEnter))]
    [JankyProperty("mouseLeave", nameof(MouseLeave))]
    [JankyProperty("title", nameof(Title))]
    internal class WindowNode : AreaNode
    {
        public int ID { get; set; }
        public readonly DataContextProperty<string> Title;
        public readonly DataContextMethod<Action<int, bool>> MouseEnter;
        public readonly DataContextMethod<Action<int, bool>> MouseLeave;

        private bool _mouseState = false;

        protected override void OnGUI()
        {
            AreaRect = GUI.Window(ID, AreaRect, WndProc, Title);

            var newMouseState = AreaRect.Contains(Event.current.mousePosition);
            if (newMouseState != _mouseState)
            {
                _mouseState = newMouseState;
                if (newMouseState)
                    MouseEnter.Invoke(ID, true);
                else
                    MouseLeave.Invoke(ID, false);
            }
        }

        public void WndProc(int id)
        {
            try
            {
                var clientArea = new Rect();
                var border = GUI.skin.window.padding;

                clientArea.Set(border.left, border.top, Width - (border.left + border.right), Height - (border.top + border.bottom));
                
                GUILayout.BeginArea(clientArea);

                foreach (var child in Children)
                    child.Execute();

                GUILayout.EndArea();

                GUI.DragWindow(new Rect(0, 0, 10000, 20));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
