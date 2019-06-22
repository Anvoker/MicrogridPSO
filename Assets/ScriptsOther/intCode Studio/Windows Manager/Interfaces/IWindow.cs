using UnityEngine;
using System.Collections;

namespace WindowManager
{
    public interface IWindow
    {
        void Unhide();
        void Hide();
        void Show();
        void Close();
    }
}