using JankyUI.Nodes;
using UnityEngine;

namespace JankyUI
{
    public interface IJankyContext
    {
        object DataContext { get; set; }

        GUISkin Skin { get; set; }

        int WindowID { get; }

        void OnGUI();

        bool Active { get; set; }
    }
}
