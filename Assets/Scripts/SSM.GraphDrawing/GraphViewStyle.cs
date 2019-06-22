using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSM.GraphDrawing
{
    [System.Serializable]
    public class GraphViewStyle : MonoBehaviour
    {
        public List<LineStyle> lineStyleDefaults;
        public LineStyle zeroLine;
        public LineStyle axis;
        public LineStyle grid;
        public LineStyle marker;

        public TextStyle labelTextX;
        public TextStyle labelTextY;
        public TextStyle titleText;
        public TextStyle markerText;

        public Vector2 minAxis;
        public Vector2 maxAxis;
        public bool minAxisXOverride;
        public bool minAxisYOverride;
        public bool maxAxisXOverride;
        public bool maxAxisYOverride;

        public AnchorPresets anchorTitle;
        public AnchorPresets anchorLabelX;
        public AnchorPresets anchorLabelY;
        public PivotPresets pivotTitle;
        public PivotPresets pivotLabelX;
        public PivotPresets pivotLabelY;

        [Delayed]
        public float markersHeight = 5.0f;

        public Vector2 labelMult = new Vector2(1440.0f, 1.0f);
        public Vector2 gridDist = new Vector2(100.0f, 100.0f);
        public Vector2 markersDist = new Vector2 (100.0f, 100.0f);
        public Vector2 markerOffset = new Vector2(4.0f, 4.0f);
        public Vector2 labelXOffset = new Vector2(4.0f, 4.0f);
        public Vector2 labelYOffset = new Vector2(4.0f, 4.0f);
        public Vector2 titleOffset = new Vector2(0.0f, 0.0f);

        [Delayed]
        public int markerLabelRoundingX = 1;
        [Delayed]
        public int markerLabelRoundingY = 1;

        public int markerValueRoundingX = 0;
        public int markerValueRoundingY = 0;
    }
}