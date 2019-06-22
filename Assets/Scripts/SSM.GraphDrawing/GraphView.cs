using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SSM.GraphDrawing
{
    [System.Serializable]
    public class GraphView
    {
        public List<Graph> graphs = new List<Graph>();
        public GraphViewStyle style;

        public void RemoveGraph(Graph graph)
        {
            if (graphs.Contains(graph))
            {
                graphs.Remove(graph);
                graph.DestroyLine();
            }
        }

        public void AddGraph(Graph graph, LineStyle lineStyle = null)
        {
            if (graphs.Contains(graph))
            {
                return;
            }

            if (style.lineStyleDefaults.Count < graphs.Count)
            {
                for (int i = style.lineStyleDefaults.Count; i < graphs.Count; i++)
                {
                    style.lineStyleDefaults.Add(new LineStyle(style.lineStyleDefaults[0]));
                }
            }

            graphs.Add(graph);
            if (style.lineStyleDefaults.Count >= graphs.Count)
            {
                if (lineStyle != null)
                {
                    style.lineStyleDefaults[graphs.Count - 1] = lineStyle;
                }
            }
            else
            {
                style.lineStyleDefaults.Add(lineStyle ?? new LineStyle(style.lineStyleDefaults[0]));
            }
        }
    }
}