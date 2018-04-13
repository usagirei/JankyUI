using System;
using JankyUI.Attributes;
using JankyUI.Nodes.Binding;
using JankyUI.EventArgs;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Window")]
    [JankyProperty("on-mouse-over", nameof(MouseOver))]
    [JankyProperty("title", nameof(Title))]
    internal class WindowNode : AreaNode
    {
        public JankyProperty<string> Title;
        public JankyMethod<Action<JankyEventArgs<bool>>> MouseOver;

        public int ID { get { return Context.WindowID; } }

        private bool _mouseIsOver = false;

        protected override void OnGUI()
        {
#if MOCK
            WndProc(-1);
#else
            AreaRect = GUI.Window(ID, AreaRect, WndProc, Title);

            var oldMouseState = _mouseIsOver;
            var newMouseState = AreaRect.Contains(Event.current.mousePosition);
            if (newMouseState != oldMouseState)
            {
                _mouseIsOver = newMouseState;
                MouseOver.Invoke(new JankyEventArgs<bool>(ID, Name, oldMouseState, newMouseState));
            }
#endif
        }

        public void WndProc(int id)
        {
#if MOCK
            Console.WriteLine("Begin Window: {0} {1} {2} {3}", X, Y, Width, Height);
            foreach (var child in Children)
                child.Execute();
            Console.WriteLine("End Window");
#else
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
#endif
        }
    }
}
