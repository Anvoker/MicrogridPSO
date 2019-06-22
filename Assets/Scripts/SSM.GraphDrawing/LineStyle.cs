using UnityEngine;
using Vectrosity;

namespace SSM.GraphDrawing
{
    [System.Serializable]
    public class LineStyle
    {
        public Color lineColor = Color.black;
        public float lineThickness = 1.0f;
        public LineType lineType;
        public Joins joinType;

        public LineStyle() { }

        public LineStyle(LineStyle original)
        {
            lineColor = original.lineColor;
            lineThickness = original.lineThickness;
            lineType = original.lineType;
            joinType = original.joinType;
        }

        public LineStyle(Color lineColor, float lineThickness)
        {
            this.lineColor     = lineColor;
            this.lineThickness = lineThickness;
            this.lineType      = LineType.Continuous;
            this.joinType      = Joins.Weld;
        }

        public LineStyle(Color lineColor, float lineThickness, LineType lineType, Joins joinType)
        {
            this.lineColor     = lineColor;
            this.lineThickness = lineThickness;
            this.lineType      = lineType;
            this.joinType      = joinType;
        }
    }
}