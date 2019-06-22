using System;
using UnityEngine;

namespace SSM.GraphDrawing
{
    [Serializable]
    public class TextStyle
    {
        public int size = 8;
        public Font font;
        public Color color = Color.black;
        public TextAnchor alignment = TextAnchor.MiddleCenter;
    }
}