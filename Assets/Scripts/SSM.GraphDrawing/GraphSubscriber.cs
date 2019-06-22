using SSM.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections.ObjectModel;

namespace SSM.GraphDrawing
{
    [Serializable]
    public class GraphSubscriber : MonoBehaviour
    {
        [System.Serializable]
        public struct MicrogridVarAndIndex
        {
            public MicrogridVar mvar;
            public int index;
        }

        public GraphCanvas graphCanvas;
        public Microgrid microgrid;
        public GraphInput graphInput;
        public List<MicrogridVarAndIndex> loadOnSetup;

        public ReadOnlyCollection<GraphMicrogridLink> Links
        {
            get;
            protected set;
        }

        [SerializeField]
        private List<GraphMicrogridLink> linksMutable
            = new List<GraphMicrogridLink>();

        private bool isSubscribedToMicrogrid;
        private bool isSubscribedToGraphInput;

        public void AddLinks(IList<GraphMicrogridLink> links)
        {
            linksMutable.AddRange(links);
            for (int i = 0; i < links.Count; i++)
            {
                UpdateLink(links[i]);
            }
        }

        public void AddLink(GraphMicrogridLink link)
        {
            linksMutable.Add(link);
            UpdateLink(link);
        }

        public void RemoveLink(GraphMicrogridLink link)
        {
            foreach (Graph graph in link.graphs)
            {
                graphCanvas.view.graphs.Remove(graph);
            }
            linksMutable.Remove(link);
        }

        public void RemoveAllLinks()
        {
            foreach (GraphMicrogridLink link in Links.ToList())
            {
                foreach (Graph graph in link.graphs)
                {
                    graphCanvas.view.graphs.Remove(graph);
                }
                linksMutable.Remove(link);
            }
        }

        public void LoadFromMicrogrid() => LoadVarsFromMG(this, null);

        private void Awake()
        {
            Links = new ReadOnlyCollection<GraphMicrogridLink>(linksMutable);
        }

        private void Start()
        {
            microgrid = microgrid ?? FindObjectOfType<Microgrid>();
            Setup();
            SubscribeToMicrogrid();
            SubscribeToGraphInput();
        }

        public void Setup()
        {
            RemoveAllLinks();
            LoadVarsFromMG(this, null);

            foreach (MicrogridVarAndIndex d in loadOnSetup)
            {
                var link = GraphMicrogridLink.Make(d.mvar, microgrid, d.index);
                AddLink(link);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromMicrogrid();
            UnsubscribeFromGraphInput();
        }

        private void UpdateLink(GraphMicrogridLink link)
        {
            UpdateLinkFromMG(link, microgrid, graphCanvas);
        }

        private static void UpdateLinkFromMG(
            GraphMicrogridLink link,
            Microgrid microgrid, 
            GraphCanvas canvas)
        {
            if (link.dimensions == 0)
            {
                Debug.LogWarning("Graph can't have zero dimensions.");
                return;
            }
            else if (link.dimensions == 1)
            {
                Update1DFromMG(link, microgrid, canvas);
            }
            else if (link.dimensions == 2)
            {
                Update2DFromMG(link, microgrid, canvas);
            }
            else
            {
                throw new ArgumentException("Trying to get an " +
                    "unhandled dimension.", nameof(link.dimensions));
            }
        }

        private static void SetToMG(
            Microgrid microgrid,
            GraphMicrogridLink link)
        {
            if (MGMisc.accessorLists.ContainsKey(link.mvar))
            {
                if (link.graphs.Count > 0)
                {
                    var values = GetYValues(link.graphs[0].RawCoords);
                    MGMisc.accessorLists[link.mvar].Set(microgrid, values);
                }
            }
            else if (MGMisc.accessor2DArray.ContainsKey(link.mvar))
            {
                float[,] values = MGMisc.accessor2DArray[link.mvar].Get(microgrid);

                int length = values.GetLength(link.targetDimension);

                if (length != link.graphs.Count)
                {
                    return;
                }

                if (link.targetDimension == 0)
                {
                    if (link.index >= 0)
                    {
                        var value = GetYValues(link.graphs[0].RawCoords);
                        MGHelper.SetRow(value, values, link.index);
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            var value = GetYValues(link.graphs[i].RawCoords);
                            MGHelper.SetRow(value, values, i);
                        }
                    }
                }
                else if (link.targetDimension == 1)
                {
                    if (link.index >= 0)
                    {
                        var value = GetYValues(link.graphs[0].RawCoords);
                        MGHelper.SetCol(value, values, link.index);
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            var value = GetYValues(link.graphs[i].RawCoords);
                            MGHelper.SetCol(value, values, i);
                        }
                    }
                }
                else if (link.targetDimension < 0)
                {
                    throw new ArgumentException("Trying to set to a " +
                        "negative dimension.", nameof(link.targetDimension));
                }
                else
                {
                    throw new ArgumentException("Trying to set to an " +
                        "unhandled dimension.", nameof(link.targetDimension));
                }

                MGMisc.accessor2DArray[link.mvar].Set(microgrid, values);
            }
        }

        private static void Update1DFromMG(
            GraphMicrogridLink link,
            Microgrid microgrid, 
            GraphCanvas canvas)
        {
            int tCount = microgrid.Input.tCount;
            float tSize = microgrid.Input.tIncrementSize;

            if (MGMisc.accessorLists.ContainsKey(link.mvar))
            {
                var values = MGMisc.accessorLists[link.mvar]
                    .Get(microgrid);

                if (link.graphs.Count > 1)
                {
                    for (int i = link.graphs.Count - 1; i > 0; i--)
                    {
                        canvas.view.RemoveGraph(link.graphs[i]);
                        link.graphs.RemoveAt(i);
                    }

                    UpdateGraph(link, microgrid, values, 0);
                }
                else if (link.graphs.Count == 1)
                {
                    UpdateGraph(link, microgrid, values, 0);
                }
                else
                {
                    AddGraph(link, canvas, values, tCount, tSize);
                }
            }
        }

        private static void Update2DFromMG(
            GraphMicrogridLink link,
            Microgrid microgrid, 
            GraphCanvas canvas)
        {
            int tCount = microgrid.Input.tCount;
            float tSize = microgrid.Input.tIncrementSize;

            if (MGMisc.accessor2DArray.ContainsKey(link.mvar))
            {
                if (link.index >= 0)
                {
                    float[,] values = MGMisc.accessor2DArray[link.mvar]
                        .Get(microgrid);

                    int length = values.GetLength(link.targetDimension);

                    float[][] _values = Arr2DToJaggedArr(values, length,
                        link.targetDimension);

                    float[] value;

                    if (_values.GetLength(0) >= link.index + 1)
                    {
                        value = _values[link.index];
                    }
                    else
                    {
                        value = new float[0];
                    }

                    if (link.graphs.Count > 1)
                    {
                        for (int i = link.graphs.Count - 1; i > 0; i--)
                        {
                            canvas.view.RemoveGraph(link.graphs[i]);
                            link.graphs.RemoveAt(i);
                        }

                        UpdateGraph(link, microgrid, value, 0);
                    }
                    else if (link.graphs.Count == 1)
                    {
                        UpdateGraph(link, microgrid, value, 0);
                    }
                    else
                    {
                        AddGraph(link, canvas, value, tCount, tSize);
                    }

                    return;
                }
                else
                {

                    float[,] values = MGMisc.accessor2DArray[link.mvar]
                            .Get(microgrid);

                    int length = values.GetLength(link.targetDimension);

                    float[][] _values = Arr2DToJaggedArr(values, length,
                        link.targetDimension);

                    int graphCount = link.graphs.Count;
                    if (link.graphs.Count > length)
                    {
                        for (int i = graphCount - 1; i >= length; i--)
                        {
                            canvas.view.RemoveGraph(link.graphs[i]);
                            link.graphs.RemoveAt(i);
                        }
                    }
                    else if (link.graphs.Count < length)
                    {
                        for (int i = 0; i < length - graphCount; i++)
                        {
                            AddGraph(link, canvas);
                        }
                    }

                    for (int i = 0; i < length; i++)
                    {
                        UpdateGraph(link, microgrid, _values[i], i);
                    }
                    return;
                }
            }
        }

        private static float[][] Arr2DToJaggedArr(
            float[,] values, 
            int length,
            int targetDimension)
        {
            float[][] _values = new float[length][];

            if (targetDimension == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    _values[i] = MGHelper.GetRow(values, i);
                }
            }
            else if (targetDimension == 1)
            {
                for (int i = 0; i < length; i++)
                {
                    _values[i] = MGHelper.GetCol(values, i);
                }
            }

            return _values;
        }

        private static void UpdateGraph(
            GraphMicrogridLink link,
            Microgrid microgrid, 
            IList<float> values, 
            int index)
        {
            var coords = ValuesToTimeAndValues(values,
                microgrid.Input.tCount,
                microgrid.Input.tIncrementSize);

            UpdateLinkExistingGraph(link, link.graphs[index], coords);
        }


        private static void AddGraph(
            GraphMicrogridLink link,
            GraphCanvas canvas, 
            IList<float> values, 
            int tCount, 
            float tSize)
        {
            var coords = ValuesToTimeAndValues(values, tCount, tSize);
            AddGraph(link, canvas, coords);
        }

        private static void AddGraph(
            GraphMicrogridLink link,
            GraphCanvas canvas)
        {
            MGMisc.enumToColor.TryGetValue(link.mvar, out Color? colorOverride);
            MGMisc.enumToMetadata.TryGetValue(link.mvar, out string[] s);

            if (colorOverride.HasValue)
            {
                colorOverride = new Color(
                colorOverride.Value.r,
                colorOverride.Value.g,
                colorOverride.Value.b,
                0.8f);
            }

            string label;
            if (link.graphs.Count > 1)
            {
                int i = link.graphs.Count;
                label = link.mvar.GetVarDescription() + " " + i;
            }
            else
            {
                label = link.mvar.GetVarDescription();
            }

            var graph = new Graph(label, new List<Vector2>())
            {
                colorOverride = colorOverride,
                editable = link.mvar.IsEditable(),
                valueMetadata = new ValueMetadata(s)
            };
            link.graphs.Add(graph);
            canvas.view.AddGraph(graph);
        }


        private static void AddGraph(
            GraphMicrogridLink link,
            GraphCanvas canvas,
            List<Vector2> coords)
        {
            if (coords == null)
            {
                AddGraph(link, canvas);
                return;
            }

            MGMisc.enumToColor.TryGetValue(link.mvar, out Color? colorOverride);
            MGMisc.enumToMetadata.TryGetValue(link.mvar, out string[] s);

            if (colorOverride.HasValue)
            {
                colorOverride = new Color(
                    colorOverride.Value.r,
                    colorOverride.Value.g,
                    colorOverride.Value.b,
                    0.8f);
            }

            string label;
            if (link.graphs.Count > 1)
            {
                int i = link.graphs.Count;
                label = link.mvar.GetVarDescription() + " " + i;
            }
            else
            {
                label = link.mvar.GetVarDescription();
            }

            var graph = new Graph(label, coords)
            {
                colorOverride = colorOverride,
                editable = link.mvar.IsEditable(),
                valueMetadata = new ValueMetadata(s)
            };
            link.graphs.Add(graph);
            canvas.view.AddGraph(graph);
        }

        private static void UpdateLinkExistingGraph(
            GraphMicrogridLink link,
            Graph graph, 
            List<Vector2> coords)
        {
            MGMisc.enumToColor.TryGetValue(link.mvar, out Color? colorOverride);
            MGMisc.enumToMetadata.TryGetValue(link.mvar, out string[] s);

            if (colorOverride.HasValue)
            {
                colorOverride = new Color(
                colorOverride.Value.r,
                colorOverride.Value.g,
                colorOverride.Value.b,
                0.8f);
            }

            if (link.graphs.Count > 1)
            {
                int i = link.graphs.FindIndex(x => x == graph) + 1;
                graph.label = link.mvar.GetVarDescription() + " " + i;
            }
            else
            {
                graph.label = link.mvar.GetVarDescription();
            }

            if (coords == null)
            {
                coords = new List<Vector2>();
            }

            graph.colorOverride = colorOverride;
            graph.editable = link.mvar.IsEditable();
            graph.valueMetadata = new ValueMetadata(s);
            graph.SetRawCoords(coords);
            graph.isDirty = true;
        }

        private void LoadVarsFromMG(object sender, EventArgs e)
        {
            if (Links == null) { return; }

            foreach (GraphMicrogridLink link in Links)
            {
                if (link.linkType == LinkType.GraphToMicrogrid)
                {
                    continue;
                }
                UpdateLink(link);
            }
        }

        private void LoadInputVarsFromMG(object sender, EventArgs e)
        {
            if (Links == null) { return; }

            foreach (GraphMicrogridLink link in Links)
            {
                if (link.linkType == LinkType.GraphToMicrogrid
                    || !MGMisc.GetInputVars().Contains(link.mvar))
                {
                    continue;
                }
                UpdateLink(link);
            }
        }

        private void SubscribeToMicrogrid()
        {
            if (microgrid != null && !isSubscribedToMicrogrid)
            {
                isSubscribedToMicrogrid = true;
                microgrid.OnLoaded += LoadInputVarsFromMG;
                microgrid.OnCalculated += LoadVarsFromMG;
            }
        }

        private void UnsubscribeFromMicrogrid()
        {
            if (microgrid != null && isSubscribedToMicrogrid)
            {
                isSubscribedToMicrogrid = false;
                microgrid.OnLoaded -= LoadInputVarsFromMG;
                microgrid.OnCalculated -= LoadVarsFromMG;
            }
        }

        private void SubscribeToGraphInput()
        {
            if (!isSubscribedToGraphInput && graphInput != null)
            {
                isSubscribedToGraphInput = true;
                graphInput.onMove += SaveToMicrogrid;
            }
        }

        private void UnsubscribeFromGraphInput()
        {
            if (isSubscribedToGraphInput && graphInput != null)
            {
                isSubscribedToGraphInput = false;
                graphInput.onMove -= SaveToMicrogrid;
            }
        }

        private void SaveToMicrogrid(object sender, EventArgs e)
        {
            UnsubscribeFromMicrogrid();
            if (Links == null) { return; }

            microgrid.Input.SuspendEvent();
            SaveToMicrogridSilent(sender, e);
            microgrid.Input.ResumeEvent();
            graphCanvas.isDirty = true;
            graphCanvas.Update();
            LoadFromMicrogrid();
            SubscribeToMicrogrid();
        }

        private void SaveToMicrogridSilent(object sender, EventArgs e)
        {
            microgrid.Input.dirty = true;
            for (int i = 0; i < Links.Count; i++)
            {
                if (Links[i].linkType == LinkType.MicrogridToGraph)
                {
                    continue;
                }

                SetToMG(microgrid, Links[i]);
            }
        }

        private static List<float> GetYValues(IEnumerable<Vector2> coords)
            => coords.Select(coord => coord.y).ToList();

        private static List<Vector2> ValuesToTimeAndValues(
            IList<float> values, int tCount, float tSize)
        {
            if(values == null || values.Count <= 0 || tCount == 0 || tSize == 0)
            {
                return null;
            }

            var coords = new List<Vector2>(tCount);
            float t = 0.0f;
            for (int i = 0; i < tCount; i++)
            {
                coords.Add(new Vector2(t, values[i]));
                t += tSize / (tSize * tCount);
            }

            return coords;
        }
    }

    public enum LinkType
    {
        GraphToMicrogrid,
        MicrogridToGraph,
        Bidirectional
    }

    [System.Serializable]
    public struct MicrogridVarStruct
    {
        public MicrogridVar mvar;
        public int dimensionToGet;
        public int[] arrayIndex;
    }

    [System.Serializable]
    public class GraphMicrogridLink
    {
        public List<Graph> graphs = new List<Graph>();
        public MicrogridVar mvar;
        public LinkType linkType;
        public int dimensions;
        public int targetDimension;
        public int index = -1;

        public GraphMicrogridLink() { }

        public static GraphMicrogridLink Make(MicrogridVar mvar, Microgrid m, int index)
        {
            int dimensions;
            int targetDimension;

            var arr = mvar.GetArray();
            if (arr.Length == 0)
            {
                Debug.LogWarning("Shouldn't try to graph non-array values.");
                return null;
            }
            else if (arr.Length == 1)
            {
                dimensions = 1;
                targetDimension = 0;
                return new GraphMicrogridLink(mvar, dimensions,
                    targetDimension, index);
            }
            else if (arr.Length == 2)
            {
                dimensions = 2;
                int tIndex
                    = Array.FindIndex(arr, x => x == MicrogridVar.TCount);

                if (tIndex < 0)
                {
                    throw new InvalidProgramException();
                }
                else if (tIndex == 0)
                {
                    targetDimension = 1;
                }
                else if (tIndex == 1)
                {
                    targetDimension = 0;
                }
                else
                {
                    throw new NotImplementedException();
                }
                return new GraphMicrogridLink(mvar, dimensions,
                    targetDimension, index);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public GraphMicrogridLink(MicrogridVar mvar, int dimensions,
            int targetDimension, int index)
        {
            this.mvar = mvar;
            this.dimensions = dimensions;
            this.targetDimension = targetDimension;
            this.index = index;

            if (mvar.IsEditable())
            {
                linkType = LinkType.Bidirectional;
            }
            else
            {
                linkType = LinkType.MicrogridToGraph;
            }
        }
    }
}