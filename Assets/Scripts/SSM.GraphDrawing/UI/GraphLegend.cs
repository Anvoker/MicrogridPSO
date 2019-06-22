using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using UnityEngine.UI;

namespace SSM.GraphDrawing.UI
{
    public class GraphLegend : MonoBehaviour
    {
        public GraphCanvas graphCanvas;
        public Canvas canvas;
        public GraphLegendItem template;
        private Dictionary<Graph, GraphLegendItem> items;

        public void Generate()
        {
            if (graphCanvas.view == null || graphCanvas.view.graphs == null)
            {
                return;
            }

            items = new Dictionary<Graph, GraphLegendItem>();
            template.gameObject.SetActive(false);
            int i = 0;

            foreach (Graph graph in graphCanvas.view.graphs)
            {
                LineStyle lineStyle = graphCanvas.view.style.lineStyleDefaults[0];
                if (graphCanvas.view.style.lineStyleDefaults.Count > i)
                {
                    lineStyle = graphCanvas.view.style.lineStyleDefaults[i];
                }
                var newItem = Instantiate(template, transform, false);
                newItem.gameObject.SetActive(true);
                newItem.label.text = graph.label;
                var width = newItem.lineDrawRT.GetComponent<LayoutElement>().minWidth;
                var middleLeft = new Vector2(0.0f, 0.0f);
                var middleRight = new Vector2(-width, 0.0f);
                var coords = new List<Vector2> { middleLeft, middleRight };
                var newLine = Factory.Line(null, coords, canvas, canvas.transform, lineStyle, "line", graph.colorOverride);
                newLine.rectTransform.SetParent(newItem.lineDrawRT, false);
                newLine.rectTransform.SetAnchor(AnchorPresets.MiddleRight);
                newLine.rectTransform.SetPivot(PivotPresets.MiddleRight);
                items.Add(graph, newItem);
                i++;
            }
        }

        private void OnEnable()
        {
            if (graphCanvas == null)
            {
                graphCanvas = GetComponentInParent<GraphCanvas>();
            }

            if (graphCanvas == null)
            {
                graphCanvas = transform.parent
                    .GetComponentInChildren<GraphCanvas>();
            }

            if (graphCanvas == null)
            {
                graphCanvas = transform.parent.parent
                    .GetComponentInChildren<GraphCanvas>();
            }

            if (graphCanvas != null)
            {
                Generate();
            }
        }

        private void OnDisable()
        {
            if (items != null)
            {
                foreach (Graph graph in items.Keys)
                {
                    Destroy(items[graph].gameObject);
                }

                items.Clear();
            }
        }
    }
}
