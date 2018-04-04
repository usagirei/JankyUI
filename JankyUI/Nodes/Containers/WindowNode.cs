using System;
using JankyUI.Attributes;
using JankyUI.Binding;
using UnityEngine;

namespace JankyUI.Nodes
{
    [JankyTag("Window")]
    [JankyProperty("mouse-enter", nameof(MouseEnter))]
    [JankyProperty("mouse-leave", nameof(MouseLeave))]
    [JankyProperty("title", nameof(Title))]
    internal class WindowNode : AreaNode
    {
        public readonly JankyProperty<string> Title;
        public readonly JankyMethod<Action<int, bool>> MouseEnter;
        public readonly JankyMethod<Action<int, bool>> MouseLeave;

        public int ID { get { return Context.WindowID; } }

        private bool _mouseState = false;

        protected override void OnGUI()
        {
#if MOCK
            WndProc(-1);
#else
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
