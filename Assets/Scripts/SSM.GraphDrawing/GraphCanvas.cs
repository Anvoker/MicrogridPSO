using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Vectrosity;
using VectorExtensions;
using System.Text.Formatting;
using TMPro;
using UnityEngine.Profiling;

namespace SSM.GraphDrawing
{
    [RequireComponent(typeof(Canvas))]
    public class GraphCanvas : MonoBehaviour
    {
        public Canvas canvas;
        public RectTransform canvasRT;
        public GraphView view;
        public GraphStrings strings;
        public RectTransform DrawSurface => drawSurface;

        public TMP_Text labelAxisPrototype;
        public TMP_Text labelPrototypeX;
        public TMP_Text labelPrototypeY;
        public TMP_Text titlePrototype;

        public Vector2 RawMin => rawMin;
        public Vector2 RawMax => rawMax;
        public Vector2 Increment => increment;
        public Vector2 IncrementPx => incrementPx;
        public Vector2 AxisLengthPx => axisLengthPx;
        public Vector2 AxisLength => axisLength;

        public bool isDirty;

        private RectTransform drawSurface;
        private RectTransform labelXSurface;
        private RectTransform labelYSurface;
        private RectTransform titleContainer;
        private HorizontalLayoutGroup layoutX;
        private VerticalLayoutGroup layoutY;

        private RectTransform gridContainer;
        private RectTransform lineContainer;
        private RectTransform axisContainer;
        private RectTransform axisLabelContainer;

        private VectorLine zeroLine;
        private VectorLine axisLine;
        private List<VectorLine> markersX = new List<VectorLine>();
        private List<VectorLine> markersY = new List<VectorLine>();
        private List<VectorLine> gridX    = new List<VectorLine>();
        private List<VectorLine> gridY    = new List<VectorLine>();

        //The minimum and maximum values in rawCoords for all graphs
        private Vector2 rawMin  = new Vector2(0.0f, 0.0f);
        private Vector2 rawMax  = new Vector2(0.0f, 0.0f);
        private float yOffsetBottom;
        private float yOffsetTop;
        private float xOffsetLeft;
        private float xOffsetRight;

        private List<TMP_Text> markerTextsX = new List<TMP_Text>();
        private List<TMP_Text> markerTextsY = new List<TMP_Text>();
        private TMP_Text labelX;
        private TMP_Text labelY;
        private TMP_Text title;
        private const int ObjLimit = 100;

        private RectTransform anchorRightBottom;
        private RectTransform anchorLeftTop;
        private RectTransform anchorMiddleTop;

        public bool negativeY;

        private static Axis[] axes = (Axis[])Enum.GetValues(typeof(Axis));
        private Vector2 increment;
        private Vector2 incrementPx;
        private Vector2 axisLength;
        private Vector2 axisLengthPx;

        public void Update()
        {
            if (view.graphs.Count <= 0)
            {
                return;
            }

            bool hasCoords = false;

            for (int i = 0; i < view.graphs.Count; i++)
            {
                if (view.graphs[i].RawCoords.Count > 0)
                {
                    hasCoords = true;
                    break;
                }
            }

            if (hasCoords)
            {
                Draw();
            }
        }

        protected void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvasRT = GetComponent<RectTransform>();
            view.style = view.style ?? GetComponentInChildren<GraphViewStyle>();

            var topVLayoutGO = new GameObject("VLayout");
            var topVLayoutRT = topVLayoutGO.AddComponent<RectTransform>();
            var topVLayoutGroup = topVLayoutGO.AddComponent<VerticalLayoutGroup>();
            topVLayoutGroup.childControlWidth = true;
            topVLayoutGroup.childControlHeight = true;
            topVLayoutGroup.childForceExpandWidth = true;
            topVLayoutGroup.childForceExpandHeight = false;
            topVLayoutGroup.spacing = 16;
            topVLayoutRT.SetParent(canvasRT, false);
            topVLayoutRT.SetAnchor(AnchorPresets.StretchAll);
            topVLayoutRT.offsetMin = Vector2.zero;
            topVLayoutRT.offsetMax = Vector2.zero;

            var titleContainerGO = new GameObject("TitleContainer");
            var titleContainerRT = titleContainerGO.AddComponent<RectTransform>();
            var titleContainerGroup = titleContainerGO.AddComponent<VerticalLayoutGroup>();
            var titleContainerLE = titleContainerGO.AddComponent<LayoutElement>();
            titleContainerLE.flexibleWidth = 1;
            titleContainerLE.flexibleHeight = 0;
            titleContainerGroup.childControlWidth = true;
            titleContainerGroup.childControlHeight = true;
            titleContainerGroup.childForceExpandWidth = true;
            titleContainerGroup.childForceExpandHeight = true;
            titleContainerRT.SetParent(topVLayoutRT, false);
            titleContainer = titleContainerRT;

            var hLayoutGO = new GameObject("HLayout");
            var hLayoutRT = hLayoutGO.AddComponent<RectTransform>();
            var hLayoutGroup = hLayoutGO.AddComponent<HorizontalLayoutGroup>();
            var hLayoutLE = hLayoutGO.AddComponent<LayoutElement>();
            hLayoutLE.flexibleWidth = 1;
            hLayoutLE.flexibleHeight = 1;
            hLayoutGroup.spacing = 8;
            hLayoutGroup.childControlWidth = true;
            hLayoutGroup.childControlHeight = true;
            hLayoutGroup.childForceExpandWidth = false;
            hLayoutGroup.childForceExpandHeight = true;
            hLayoutRT.SetParent(topVLayoutRT, false);

            var labelYContainerGO = new GameObject("LabelYRect");
            var labelYContainerRT = labelYContainerGO.AddComponent<RectTransform>();
            var labelYContainerGroup = labelYContainerGO.AddComponent<VerticalLayoutGroup>();
            var labelYContainerLE = labelYContainerGO.AddComponent<LayoutElement>();
            labelYContainerLE.flexibleWidth = 0;
            labelYContainerLE.flexibleHeight = 1;
            labelYContainerGroup.childAlignment = TextAnchor.LowerRight;
            labelYContainerGroup.childControlWidth = true;
            labelYContainerGroup.childControlHeight = true;
            labelYContainerGroup.childForceExpandWidth = false;
            labelYContainerGroup.childForceExpandHeight = true;
            labelYContainerRT.SetParent(hLayoutRT, false);
            layoutY = labelYContainerGroup;
            labelYSurface = labelYContainerRT;

            var vLayoutGO = new GameObject("VLayout");
            var vLayoutRT = vLayoutGO.AddComponent<RectTransform>();
            var vLayoutGroup = vLayoutGO.AddComponent<VerticalLayoutGroup>();
            var vLayoutLE = vLayoutGO.AddComponent<LayoutElement>();
            vLayoutLE.flexibleWidth = 1;
            vLayoutLE.flexibleHeight = 1;
            vLayoutGroup.childControlWidth = true;
            vLayoutGroup.childControlHeight = true;
            vLayoutGroup.childForceExpandWidth = true;
            vLayoutGroup.childForceExpandHeight = false;
            vLayoutRT.SetParent(hLayoutRT, false);

            var drawSurfaceGO = new GameObject("DrawSurface");
            var drawSurfaceRT = drawSurfaceGO.AddComponent<RectTransform>();
            var drawSurfaceLE = drawSurfaceGO.AddComponent<LayoutElement>();
            drawSurfaceLE.flexibleWidth = 1;
            drawSurfaceLE.flexibleHeight = 1;
            drawSurfaceRT.SetParent(vLayoutRT, false);
            drawSurface = drawSurfaceRT;

            var labelXContainerGO = new GameObject("LabelXRect");
            var labelXContainerRT = labelXContainerGO.AddComponent<RectTransform>();
            var labelXContainerGroup = labelXContainerGO.AddComponent<HorizontalLayoutGroup>();
            var labelXContainerLE = labelXContainerGO.AddComponent<LayoutElement>();
            labelXContainerLE.flexibleWidth = 1;
            labelXContainerLE.flexibleHeight = 0;
            labelXContainerGroup.childControlWidth = true;
            labelXContainerGroup.childControlHeight = true;
            labelXContainerGroup.childForceExpandWidth = true;
            labelXContainerGroup.childForceExpandHeight = false;
            labelXContainerRT.SetParent(vLayoutRT, false);
            layoutX = labelXContainerGroup;
            labelXSurface = labelXContainerRT;

            gridContainer = new GameObject("GridContainer").AddComponent<RectTransform>();
            lineContainer = new GameObject("LineContainer").AddComponent<RectTransform>();
            axisContainer = new GameObject("AxisContainer").AddComponent<RectTransform>();
            axisLabelContainer = new GameObject("AxisLabelContainer").AddComponent<RectTransform>();

            gridContainer.SetParent(drawSurfaceRT, false);
            gridContainer.SetAnchor(AnchorPresets.StretchAll);
            gridContainer.offsetMin = Vector2.zero;
            gridContainer.offsetMax = Vector2.zero;

            lineContainer.SetParent(drawSurfaceRT, false);
            lineContainer.SetAnchor(AnchorPresets.StretchAll);
            lineContainer.offsetMin = Vector2.zero;
            lineContainer.offsetMax = Vector2.zero;

            axisContainer.SetParent(drawSurfaceRT, false);
            axisContainer.SetAnchor(AnchorPresets.StretchAll);
            axisContainer.offsetMin = Vector2.zero;
            axisContainer.offsetMax = Vector2.zero;

            axisLabelContainer.SetParent(drawSurfaceRT, false);
            axisLabelContainer.SetAnchor(AnchorPresets.StretchAll);
            axisLabelContainer.offsetMin = Vector2.zero;
            axisLabelContainer.offsetMax = Vector2.zero;

            gridContainer.SetSiblingIndex(4);
            lineContainer.SetSiblingIndex(3);
            axisLabelContainer.SetSiblingIndex(2);
            axisContainer.SetSiblingIndex(1);
        }

        public Vector2 RawToPx(Vector2 v)
            => MathHelper.NormalizeToRange(v, rawMin, rawMax, Vector2.zero, AxisLengthPx);

        public Vector2 PxToRaw(Vector2 v)
            => MathHelper.DenormalizeToRange(v, rawMin, rawMax, Vector2.zero, AxisLengthPx);

        public float RawToPx(float r, Axis axis)
        {
            switch (axis)
            {
                case Axis.x:
                    return MathHelper.NormalizeToRange(r, rawMin.x, rawMax.x, 0.0f, AxisLengthPx.x);
                case Axis.y:
                    return MathHelper.NormalizeToRange(r, rawMin.y, rawMax.y, 0.0f, AxisLengthPx.y);
                default:
                    throw new InvalidOperationException();
            }
        }

        public float PxToRaw(float p, Axis axis)
        {
            switch (axis)
            {
                case Axis.x:
                    return MathHelper.DenormalizeToRange(p, rawMin.x, rawMax.x, 0.0f, AxisLengthPx.x);
                case Axis.y:
                    return MathHelper.DenormalizeToRange(p, rawMin.y, rawMax.y, 0.0f, AxisLengthPx.y);
                default:
                    throw new InvalidOperationException();
            }
        }

        public float RawToPxX(float x)
            => MathHelper.NormalizeToRange(x, rawMin.x, rawMax.x, 0.0f, AxisLengthPx.x);

        public float PxToRawX(float x)
            => MathHelper.DenormalizeToRange(x, rawMin.x, rawMax.x, 0.0f, AxisLengthPx.x);

        public float RawToPxY(float y)
            => MathHelper.NormalizeToRange(y, rawMin.y, rawMax.y, 0.0f, AxisLengthPx.y);

        public float PxToRawY(float y)
            => MathHelper.DenormalizeToRange(y, rawMin.y, rawMax.y, 0.0f, AxisLengthPx.y);

        private void GetRawMinMax()
        {
            var graphs = view.graphs;
            var style = view.style;

            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float maxX = Mathf.NegativeInfinity;
            float maxY = Mathf.NegativeInfinity;

            for (int i = 0; i < graphs.Count; i++)
            {
                if (graphs[i].RawMin.x < minX)
                {
                    minX = graphs[i].RawMin.x;
                }

                if (graphs[i].RawMin.y < minY)
                {
                    minY = graphs[i].RawMin.y;
                }

                if (graphs[i].RawMax.x > maxX)
                {
                    maxX = graphs[i].RawMax.x;
                }

                if (graphs[i].RawMax.y > maxY)
                {
                    maxY = graphs[i].RawMax.y;
                }
            }

            minX = style.minAxisXOverride ? view.style.minAxis.x : minX;
            minY = style.minAxisYOverride ? view.style.minAxis.y : minY;
            maxX = style.maxAxisXOverride ? view.style.maxAxis.x : maxX;
            maxY = style.maxAxisYOverride ? view.style.maxAxis.y : maxY;

            if (!Mathf.Approximately(rawMin.x, minX) ||
                !Mathf.Approximately(rawMin.y, minY) ||
                !Mathf.Approximately(rawMax.x, maxX) ||
                !Mathf.Approximately(rawMax.y, maxY))
            {
                rawMin = new Vector2(minX, minY);
                rawMax = new Vector2(maxX, maxY);
            }

            var oldAxisLength = axisLength;
            var oldAxisLengthPx = axisLengthPx;

            axisLength = new Vector2(rawMax.x - rawMin.x, rawMax.y - rawMin.y);
            axisLengthPx = new Vector2(drawSurface.rect.size.x, drawSurface.rect.size.y);

            if (!Mathf.Approximately(oldAxisLength.x, axisLength.x)     ||
                !Mathf.Approximately(oldAxisLength.y, axisLength.y)     ||
                !Mathf.Approximately(oldAxisLengthPx.x, axisLengthPx.x) ||
                !Mathf.Approximately(oldAxisLengthPx.y, axisLengthPx.y))
            {
                isDirty = true;
            }

            float xCount = AxisLengthPx.x / 20.0f;
            float yCount = AxisLengthPx.y / 20.0f;
            float xIncr = AxisLength.x / xCount;
            float yIncr = AxisLength.y / yCount;

            increment = new Vector2(xIncr, yIncr);
            incrementPx = new Vector2(20.0f, 20.0f);
        }

        private void ScaleCoordinates()
        {
            for (int i = 0; i < view.graphs.Count; i++)
            {
                view.graphs[i].ScaleCoordinates(rawMin, rawMax, AxisLengthPx);
            }
        }

        private void Draw()
        {
            GetRawMinMax();

            bool areGraphsDirty = false;

            for (int i = 0; i < view.graphs.Count; i++)
            {
                areGraphsDirty |= view.graphs[i].isDirty;
            }

            if (isDirty)
            {
                SetAnchors();
                DrawTitle();
            }

            if (isDirty || areGraphsDirty)
            {
                ScaleCoordinates();
                DrawGraphLines();
                DrawZeroLine();
            }

            foreach (Axis axis in axes)
            {
                if (isDirty)
                {
                    DrawAxis(axis);
                }

                if (isDirty || areGraphsDirty)
                {
                    DrawGrid(axis);
                    DrawMarkers(axis);
                    DrawTextMarkers(axis);
                    DrawAxisLabel(axis);
                }
            }

            isDirty = false;
            view.graphs.ForEach(g => g.isDirty = false);
        }

        private static void DestroyUneededLines(List<VectorLine> list, int nLinesToDestroy)
        {
            if (nLinesToDestroy > list.Count)
            {
                throw new ArgumentOutOfRangeException("Trying to destroy a higher count of lines " +
                    "than the total count of lines in this list");
            }

            for (int i = list.Count - nLinesToDestroy; i < list.Count; i++)
            {
                var currentObj = list[i];
                VectorLine.Destroy(ref currentObj, currentObj.rectTransform.gameObject);
            }

            list.RemoveRange(list.Count - nLinesToDestroy, nLinesToDestroy);
        }

        private static void DestroyUneededTexts(List<TMP_Text> list, int nTextsToDestroy)
        {
            if (nTextsToDestroy > list.Count)
            {
                throw new ArgumentOutOfRangeException("Trying to destroy a higher count of texts " +
                    "than the total count of texts in this list");
            }

            for (int i = list.Count - nTextsToDestroy; i < list.Count; i++)
            {
                Destroy(list[i].gameObject);
            }

            list.RemoveRange(list.Count - nTextsToDestroy, nTextsToDestroy);
        }

        private void DrawZeroLine()
        {
            if (rawMin.y >= 0.0f) { return; }

            var yZero = MathHelper.NormalizeToRange(0.0f, rawMin.y, rawMax.y, 0.0f, AxisLengthPx.y);

            var coords = new List<Vector2> { new Vector2(0.0f , yZero),
                                             new Vector2(AxisLengthPx.x, yZero) };

            zeroLine = Factory.Line(zeroLine, coords, canvas, lineContainer, view.style.zeroLine);
        }

        private void DrawGraphLines()
        {
            var lineStyles = view.style.lineStyleDefaults;

            if (lineStyles.Count < view.graphs.Count)
            {
                for (int i = lineStyles.Count; i < view.graphs.Count; i++)
                {
                    lineStyles.Add(new LineStyle(lineStyles[0]));
                }
            }

            for (int i = 0; i < view.graphs.Count; i++)
            {
                view.graphs[i].DrawInit(canvas, lineContainer, lineStyles[i]);
            }
        }

        private void DrawTextMarkers(Axis axis)
        {
            var style = view.style;
            if ((AxisLength.Get(axis) <= 0) ||
                (style.labelMult.x <= 0) ||
                (style.labelMult.y <= 0))
            {
                return;
            }

            string namePrefix;
            string digitFormat;
            int rounding;
            List<TMP_Text> labels;
            Func<float, Vector2> GetCoord;
            RectTransform lContainer;

            TMP_Text labelPrototype;
            float distTotalPx;
            float incrementPx;
            int count;

            switch (axis)
            {
                case Axis.x:
                    labelPrototype = labelPrototypeX;
                    lContainer = labelXSurface;

                    namePrefix = "textX_";
                    digitFormat = Factory.GetRoundingFormatBuffer(view.style.markerLabelRoundingX);
                    labels = markerTextsX;
                    rounding = view.style.markerValueRoundingX;

                    distTotalPx = AxisLengthPx.x;
                    incrementPx = style.markersDist.x > 0
                        ? RawToPx(style.markersDist).x
                        : IncrementPx.x;
                    count = Mathf.FloorToInt(distTotalPx / incrementPx) + 1;

                    var spc = drawSurface.rect.width / count;

                    layoutX.childControlHeight = true;
                    layoutX.childControlWidth = true;
                    layoutX.childForceExpandHeight = false;
                    layoutX.childForceExpandWidth = true;
                    layoutX.padding.left = -Mathf.RoundToInt(spc) / 2;
                    layoutX.padding.right = -Mathf.RoundToInt(spc) / 2;

                    GetCoord = (x) =>
                    {
                        return new Vector2(x, view.style.markerOffset.x);
                    };
                    break;

                case Axis.y:
                    labelPrototype = labelPrototypeY;
                    lContainer = labelYSurface;

                    namePrefix = "textY_";
                    digitFormat = Factory.GetRoundingFormatBuffer(view.style.markerLabelRoundingY);
                    labels = markerTextsY;
                    rounding = view.style.markerValueRoundingY;

                    distTotalPx = AxisLengthPx.y;
                    incrementPx = style.markersDist.y > 0
                        ? RawToPx(style.markersDist).y
                        : IncrementPx.y;
                    count = Mathf.FloorToInt(distTotalPx / incrementPx) + 1;

                    layoutY.childControlHeight = true;
                    layoutY.childControlWidth = true;
                    layoutY.childForceExpandHeight = true;
                    layoutY.childForceExpandWidth = false;
                    layoutY.padding.bottom = Mathf.RoundToInt(labelXSurface.rect.height);

                    GetCoord = (y) =>
                    {
                        return new Vector2(view.style.markerOffset.y, y);
                    };
                    break;

                default:
                    return;
            }

            float distCurrPx = 0.0f;
            int nNewTextsNeeded = count - labels.Count;

            if (labels.Count + nNewTextsNeeded > ObjLimit)
            {
                return;
            }

            //Destroy unnecessary marker labels if there are more than we need
            if (nNewTextsNeeded < 0)
            {
                DestroyUneededTexts(labels, Mathf.Abs(nNewTextsNeeded));
                nNewTextsNeeded = 0;
            }

            //Refresh existing marker labels with new position and text
            for (int i = 0; i < labels.Count; i++)
            {
                var pos = GetCoord(distCurrPx);
                var name = namePrefix + i.ToString();
                var num = Math.Round(PxToRaw(distCurrPx, axis) * style.labelMult.Get(axis), rounding);
                var text = StringBuffer.Format(digitFormat, num);
                labels[i] = Factory.Label(labels[i], labelPrototype, name, text, lContainer, pos, style.markerText);
                distCurrPx += incrementPx;
            }

            //Add new marker labels needed to cover the whole axis
            for (int i = 0; i < nNewTextsNeeded; i++)
            {
                var pos = GetCoord(distCurrPx);
                var name = namePrefix + labels.Count.ToString();
                var num = Math.Round(PxToRaw(distCurrPx, axis) * style.labelMult.Get(axis), rounding);
                var text = StringBuffer.Format(digitFormat, num);
                var textObj = Factory.Label(null, labelPrototype, name, text, lContainer, pos, style.markerText);
                labels.Add(textObj);
                distCurrPx += incrementPx;
            }

            if (axis == Axis.y)
            {
                for (int i = 0; i < labels.Count; i++)
                {
                    labels[i].rectTransform.SetAsFirstSibling();
                }
            }
        }

        private void DrawGrid(Axis axis)
        {
            var style = view.style;

            string namePrefix;
            Func<float, List<Vector2>> GetCoords;
            List<VectorLine> grid;

            //Select variables conditionally based on if this is the x or y axis
            switch (axis)
            {
                case Axis.x:
                    namePrefix = "GridX_";
                    grid = gridX;
                    GetCoords = (x) => new List<Vector2> { new Vector2(x, -1.0f),
                                                           new Vector2(x, AxisLengthPx.y) };
                    break;

                case Axis.y:
                    namePrefix = "GridY_";
                    grid = gridY;
                    GetCoords = (y) => new List<Vector2> { new Vector2(-1.0f, y),
                                                           new Vector2(AxisLengthPx.x, y) };
                    break;

                default:
                    throw new InvalidOperationException();
            }

            float distTotalPx = AxisLengthPx.Get(axis);
            float incrementPx = style.markersDist.Get(axis) > 0
                ? RawToPx(style.markersDist).Get(axis)
                : IncrementPx.Get(axis);
            float distCurrPx  = incrementPx;
            int count = Mathf.FloorToInt(distTotalPx / incrementPx);
            int nNewLinesNeeded = count - grid.Count;

            if (grid.Count + nNewLinesNeeded > ObjLimit)
            {
                return;
            }

            //Destroy unnecessary grid lines if there are more than we need
            if (nNewLinesNeeded < 0)
            {
                DestroyUneededLines(grid, Mathf.Abs(nNewLinesNeeded));
                nNewLinesNeeded = 0;
            }

            //Refresh existing grid lines with new position
            for (int i = 0; i < grid.Count; i++)
            {
                var coords = GetCoords(distCurrPx);
                grid[i] = Factory.Line(grid[i], coords, canvas, gridContainer, style.grid);
                distCurrPx += incrementPx;
            }

            //Add new grid lines needed to cover the whole axis
            for (int i = 0; i < nNewLinesNeeded; i++)
            {
                var coords = GetCoords(distCurrPx);
                var name = namePrefix + grid.Count.ToString();
                grid.Add(Factory.Line(null, coords, canvas, gridContainer, style.grid, name));
                distCurrPx += incrementPx;
            }
        }

        private void DrawMarkers(Axis axis)
        {
            var style = view.style;
            if ((style.markersDist.x <= 0) || (style.markersDist.y <= 0)) { return; }

            string namePrefix; Func<float, List<Vector2>> GetCoords; List<VectorLine> markers;

            //Select variables conditionally based on if this is the x or y axis
            switch (axis)
            {
                case Axis.x:
                    namePrefix = "MarkerX_";
                    markers = markersX;
                    GetCoords = (x) => new List<Vector2> { new Vector2(x, -1.0f),
                                                           new Vector2(x, style.markersHeight) };
                    break;

                case Axis.y:
                    namePrefix = "MarkerY_";
                    markers = markersY;
                    GetCoords = (y) => new List<Vector2> { new Vector2(-1.0f, y),
                                                           new Vector2(style.markersHeight, y) };
                    break;

                default:
                    throw new InvalidOperationException();
            }

            float distTotalPx = AxisLengthPx.Get(axis);
            float incrementPx = style.markersDist.Get(axis) > 0
                ? RawToPx(style.markersDist).Get(axis)
                : IncrementPx.Get(axis);
            float distCurrPx  = incrementPx;
            int count = Mathf.FloorToInt(distTotalPx / incrementPx);
            int nNewMarkersNeeded = count - markers.Count;
            if (markers.Count + nNewMarkersNeeded > ObjLimit)
            {
                return;
            }

            //Destroy unnecessary markers if there are more than we need
            if (nNewMarkersNeeded < 0)
            {
                DestroyUneededLines(markers, Mathf.Abs(nNewMarkersNeeded));
                nNewMarkersNeeded = 0;
            }

            //Refresh existing markers with new position
            for (int i = 0; i < markers.Count; i++)
            {
                var coords = GetCoords(distCurrPx);
                markers[i] = Factory.Line(markers[i], coords, canvas, gridContainer, style.marker);
                distCurrPx += incrementPx;
            }

            //Add new markers needed to cover the whole axis
            for (int i = 0; i < nNewMarkersNeeded; i++)
            {
                var coords = GetCoords(distCurrPx);
                var name = namePrefix + markers.Count.ToString();
                markers.Add(Factory.Line(null, coords, canvas, gridContainer, style.marker, name));
                distCurrPx += incrementPx;
            }
        }

        private void DrawAxisLabel(Axis axis)
        {
            var style = view.style;

            switch (axis)
            {
                case Axis.x:
                {
                    var name = "LabelX";
                    var text = strings.axisX;
                    var pos = new Vector2(AxisLengthPx.x, 0.0f);
                    labelX = Factory.Label(labelX, labelAxisPrototype, name, text, axisLabelContainer, pos);
                    labelX.alignment = TextAlignmentOptions.BottomRight;
                    labelX.margin = new Vector4(0.0f, 0.0f, 0.0f, 3.0f);
                    var rt = labelX.GetComponent<RectTransform>();
                    return;
                }

                case Axis.y:
                {
                    var name = "LabelY";
                    var text = GetLabelYString();
                    var pos = new Vector2(0.0f, AxisLengthPx.y);
                    labelY = Factory.Label(labelY, labelAxisPrototype, name, text, axisLabelContainer, pos);
                    labelY.alignment = TextAlignmentOptions.Left;
                    labelY.margin = new Vector4(6.0f, 0.0f, 0.0f, 0.0f);
                    var rt = labelY.GetComponent<RectTransform>();
                    return;
                }

                default:
                    throw new InvalidOperationException();
            }
        }

        private void DrawAxis(Axis axis)
        {
            var style = view.style;

            var name = "Axis";
            var p0 = new Vector2(AxisLengthPx.x, 0.0f);
            var p1 = Vector2.zero;
            var p2 = new Vector2(0.0f, AxisLengthPx.y);
            var coords = new List<Vector2> { p0, p1, p2 };
            axisLine = Factory.Line(axisLine, coords, canvas, axisContainer, style.axis, name);
        }

        private string GetLabelYString()
        {
            if (view.graphs.Count > 0 
                && (view.graphs[0].valueMetadata.name == null 
                || view.graphs[0].valueMetadata.name == ""))
            {
                return strings.axisY;
            }

            var builder = new System.Text.StringBuilder();
            var list = new List<string>();
            for (int i = 0; i < view.graphs.Count; i++)
            {
                string name = view.graphs[i].valueMetadata.name;
                string unit = view.graphs[i].valueMetadata.unit;

                if (list.Contains(name)) { continue; }
                unit = string.IsNullOrEmpty(unit) ? "-" : unit;
                builder.Append(name).Append("[").Append(unit).Append("]");
                if (i < view.graphs.Count - 1)
                { builder.AppendLine(); }
                list.Add(name);
            }
            return builder.ToString();
        }

        private void SetAnchors()
        {
            Vector2 pos;

            pos               = new Vector2(AxisLengthPx.x, 0.0f);
            anchorRightBottom = Factory.Anchor(anchorRightBottom, canvas.transform,
                                                "AnchorRightBottom", pos, Vector2.zero);

            pos               = new Vector2(0.0f, AxisLengthPx.y);
            anchorLeftTop     = Factory.Anchor(anchorLeftTop, canvas.transform,
                                                "AnchorLeftTop", pos, Vector2.zero);

            pos               = new Vector2(AxisLengthPx.x / 2.0f, AxisLengthPx.y);
            anchorMiddleTop   = Factory.Anchor(anchorMiddleTop, canvas.transform,
                                                "AnchorMiddleTop", pos, Vector2.zero);
        }

        private void DrawTitle()
        {
            var style = view.style;
            var pos = new Vector2(0.0f, 14.0f);
            var name = "Title";
            var text = strings.title;
            title = Factory.Label(title, titlePrototype, name, text, titleContainer, pos, view.style.titleText);
        }
    }
}