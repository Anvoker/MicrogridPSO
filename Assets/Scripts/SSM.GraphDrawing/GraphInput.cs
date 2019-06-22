using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SSM.GraphDrawing
{
    public class GraphInput : MonoBehaviour
    {
        public GraphCanvas graphCanvas;
        public RectTransform cursorAnchorRT;
        public RectTransform cursorRT;
        public delegate void GraphInputMoveEventHandler(object sender, MovedEventArgs e);
        public GraphInputMoveEventHandler onMoveStart;
        public GraphInputMoveEventHandler onMove;
        public GraphInputMoveEventHandler onMoveEnd;

        [SerializeField] private Vector2 uiPosition;
        //[SerializeField] private Vector2 positionStart;
        [SerializeField] private static float brushSize = 64.0f;
        //[SerializeField] private bool resampleAfterFirstClick = true;
        [SerializeField] private bool started;
        [SerializeField] private bool withinRect;

        private Vector2 positionStart;

        [SerializeField] private List<ModifiedCoord> modifiedCoords = new List<ModifiedCoord>();

        private RectTransform rt;

        private void Update() => Process();

        private void Start()
        {
            rt = graphCanvas.DrawSurface;
            cursorAnchorRT.SetParent(rt, false);
        }

        private void Process()
        {
            if (graphCanvas.view.graphs == null || 
                graphCanvas.view.graphs.Count <= 0)
            {
                if (cursorRT.gameObject.activeSelf)
                {
                    cursorRT.gameObject.SetActive(false);
                }

                return;
            }

            var mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            IEnumerable<Graph> graphs = graphCanvas.view.graphs.Where(x => x.editable);
            uiPosition = GetUIPosition(rt, mousePosition);
            withinRect = RectTransformUtility.RectangleContainsScreenPoint(rt, mousePosition, Camera.main);

            if (withinRect)
            {
                if (!cursorRT.gameObject.activeSelf)
                {
                    cursorRT.gameObject.SetActive(true);
                }
            }
            else if (cursorRT.gameObject.activeSelf)
            {
                cursorRT.gameObject.SetActive(false);
            }

            cursorAnchorRT.anchoredPosition = uiPosition;
            cursorRT.sizeDelta = new Vector2(brushSize, brushSize);


            foreach (Graph graph in graphs)
            {
                if (Input.GetMouseButton(0) && withinRect)
                {
                    if (!started)
                    {
                        modifiedCoords = new List<ModifiedCoord>();
                        modifiedCoords = GetAffectedCoords(graph, uiPosition, brushSize);
                        onMoveStart?.Invoke(this, new MovedEventArgs(graph));
                        positionStart = uiPosition;
                    }

                    Vector2 delta = uiPosition - positionStart;
                    positionStart = uiPosition;
                    MoveCoords(graph, graphCanvas, delta, modifiedCoords);
                    onMove?.Invoke(this, new MovedEventArgs(graph));
                }
                else if (started)
                {
                    modifiedCoords.Clear();
                    onMoveEnd?.Invoke(this, new MovedEventArgs(graph));
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadPlus) || 
                Input.GetKeyDown(KeyCode.Plus))
            {
                brushSize = Mathf.Clamp(brushSize + 5.0f, 0.0f, 512.0f);
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus) || 
                Input.GetKeyDown(KeyCode.Minus))
            {
                brushSize = Mathf.Clamp(brushSize - 5.0f, 0.0f, 512.0f);
            }

            started = Input.GetMouseButton(0) && withinRect; 
        }

        private static Vector2 GetUIPosition(RectTransform rt, Vector2 mousePosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, mousePosition, Camera.main, out Vector2 uiPosition);

            uiPosition += new Vector2(
                rt.rect.width / 2.0f, 
                rt.rect.height / 2.0f);

            return uiPosition;
        }

        private static List<ModifiedCoord> GetAffectedCoords(
            Graph graph, 
            Vector2 uiPosition,
            float brushSize)
        {
            var modCoords = new List<ModifiedCoord>();

            for (int i = 0; i < graph.Coords.Count; i++)
            {
                float magnitude = (graph.Coords[i] - uiPosition).magnitude;

                if (magnitude < brushSize / 2.0f)
                {
                    modCoords.Add(new ModifiedCoord(i, (graph.Coords[i] - uiPosition).y));
                }
            }
            return modCoords;
        }

        private static void MoveCoords(
            Graph graph,
            GraphCanvas graphCanvas,
            Vector2 delta,
            List<ModifiedCoord> changedCoords)
        {
            for (int i = 0; i < changedCoords.Count; i++)
            {
                int iCoord   = changedCoords[i].coordIndex;
                var newCoord = new Vector2(graph.Coords[iCoord].x, graph.Coords[iCoord].y + delta.y);
                graph.SetRawCoord(graphCanvas.PxToRaw(newCoord), iCoord);
            }

            graph.isDirty = true;
        }

        private static Vector2 GetNewCoord(
            Vector2 rawCoords, 
            Vector2 delta, 
            Vector2 ratio)
        {
            float newX = rawCoords.x;
            float newY = rawCoords.y + delta.y / ratio.y;
            return new Vector2(newX, newY);
        }

        public class MovedEventArgs : EventArgs
        {
            public Graph modifiedGraph;

            public MovedEventArgs(Graph graph)
            {
                modifiedGraph = graph;
            }
        }
    }
}
