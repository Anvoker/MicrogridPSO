using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vectrosity;
using TMPro;

namespace SSM.GraphDrawing
{
    public static class Factory
    {
        /// <summary>
        /// Shorthand method for drawing a line, initializing it if it doesn't exist
        /// </summary>
        /// <param name="line"></param>
        /// <param name="coords"></param>
        /// <param name="canvas"></param>
        /// <param name="name"></param>
        /// <param name="style"></param>
        public static VectorLine Line(
            VectorLine line, 
            List<Vector2> coords, 
            Canvas canvas,
            Transform parent,
            LineStyle style = null, 
            string name = "",
            Color? colorOverride = null)
        {
            if (style == null) { style = new LineStyle(); }

            if (line == null)
            {
               line = new VectorLine(name, coords, style.lineThickness);
            }
            else
            {
                line.name = name == "" ? line.name : name;
                line.points2 = new List<Vector2>(coords);
            }

            line.SetCanvas(canvas, false);
            line.rectTransform.SetParent(parent, false);
            line.lineWidth = style.lineThickness;
            line.color = colorOverride ?? style.lineColor;
            line.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            line.lineType = style.lineType;
            line.SetWidth(style.lineThickness);
            line.joins = style.joinType;
            line.Draw();
            return line;
        }

        public static RectTransform Anchor(
            RectTransform anchor, 
            Transform parent, 
            string name,                    
            Vector2 pos, 
            Vector2 size)
        {
            if (anchor == null)
            {
                var gameObj = new GameObject(name, typeof(RectTransform));
                gameObj.transform.SetParent(parent, false);
                anchor = gameObj.GetComponent<RectTransform>();
            }
            else
            {
                anchor.name = name;
                anchor.transform.SetParent(parent, false);
            }

            anchor.anchorMin = Vector2.zero;
            anchor.anchorMax = Vector2.zero;
            anchor.pivot = Vector2.zero;
            anchor.sizeDelta = size;
            anchor.anchoredPosition = pos;

            return anchor;
        }

        public static void UseAnchor(
            RectTransform rt, 
            RectTransform anchor, 
            Vector2 offset,
            AnchorPresets anchorPreset, 
            PivotPresets pivotPreset)
        {
            if (rt == null || anchor == null) { return; }
            rt.SetParent(anchor, false);
            rt.SetAnchor(anchorPreset);
            rt.SetPivot(pivotPreset);
            rt.anchoredPosition = offset;
        }

        public static TMP_Text Label(
            TMP_Text textObj, 
            TMP_Text prototype,
            string name, 
            string content,
            Transform parent,
            Vector2 pos, 
            TextStyle style = null)
        {
            textObj = textObj ?? UnityEngine.Object.Instantiate(prototype);

            textObj.name = name;
            textObj.transform.SetParent(parent, false);
            var rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = pos;

            if (style != null)
            {
                textObj.color = style.color;
                textObj.fontSize = style.size;
            }

            textObj.text = content;

            return textObj;
        }

        public static string GetRoundingFormatBuffer(int digits)
        {
            return "{0:F" + digits + "}";
        }

        public static string GetRoundingFormat(int digits)
        {
            string format = "0.";
            for (int j = 0; j < digits; j++)
            {
                format += "0";
            }
            return format;
        }
    }
}