using System;
using System.Collections.Generic;
using UnityEngine;
using SSM.Grid;

namespace SSM.GraphDrawing
{
    [RequireComponent(typeof(GraphCanvas))]
    public class GraphPSOSource : MonoBehaviour
    {
        private GraphCanvas graphCanvas;
        private PSOTimeline timelineController;

        protected void Awake()
        {
            timelineController = timelineController ?? FindObjectOfType<PSOTimeline>();
            graphCanvas = GetComponent<GraphCanvas>();
            timelineController.PSOTimelineChanged += SetDataToGraph;
        }

        protected void OnEnable()
        {
            SetDataToGraph(this, false);
        }

        protected void OnDestroy()
        {
            timelineController.PSOTimelineChanged -= SetDataToGraph;
        }

        private void SetDataToGraph(object sender, bool isRunning)
        {
            if (timelineController.ExecutionID < 0 || isRunning)
            {
                return;
            }

            var iterCount = timelineController.CurrentIter;

            var v = new List<Vector2>(iterCount + 1);
            for (int iIter = 0; iIter <= iterCount; iIter++)
            {
                var p = timelineController.GetProgressAtIteration(iIter);
                v.Add(new Vector2(iIter, p.GBestFitness));
            }

            if (graphCanvas.view.graphs.Count <= 0)
            {
                graphCanvas.view.AddGraph(new Graph("Convergence", v, false));
            }
            else
            {
                graphCanvas.view.graphs[0].SetRawCoords(v);
            }
        }
    }
}
