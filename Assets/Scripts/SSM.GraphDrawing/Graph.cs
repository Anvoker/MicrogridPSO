using UnityEngine;
using System.Collections.Generic;
using Vectrosity;
using System.Linq;

namespace SSM.GraphDrawing
{
    [System.Serializable]
    public class Graph
    {
        public IReadOnlyList<Vector2> Coords    => coordsRO;
        public IReadOnlyList<Vector2> RawCoords => rawCoordsRO;

        public Vector2 Min => new Vector2(
            (rawMin.x - offsetX) * ratio.x,
            (rawMin.y - offsetY) * ratio.y);

        public Vector2 Max => new Vector2(
            (rawMax.x - offsetX) * ratio.x,
            (rawMax.y - offsetY) * ratio.y);

        public Vector2 RawMin => rawMin;
        public Vector2 RawMax => rawMax;
        public Vector2 Ratio => ratio;

        public int id;
        public string label;
        public bool isDirty;
        public ValueMetadata valueMetadata;
        public bool editable;
        public Color? colorOverride;

        private IReadOnlyList<Vector2> coordsRO;
        private IReadOnlyList<Vector2> rawCoordsRO;
        private List<Vector2> coords    = new List<Vector2>();
        private List<Vector2> rawCoords = new List<Vector2>();
        private VectorLine line;

        private Vector2 rawMin;
        private Vector2 rawMax;
        private Vector2 ratio;
        private float offsetX;
        private float offsetY;

        public Graph() { }

        public Graph(
            string label,
            List<Vector2> rawCoords,
            bool editable = false,
            Color? colorOverride = null)
        {
            this.label = label;
            SetRawCoords(rawCoords);
            this.editable = editable;
            this.colorOverride = colorOverride;
        }

        public void ScaleCoordinates(Vector2 rawMin, Vector2 rawMax, Vector2 axisLengthPx)
        {
            MathHelper.NormalizeToRange(
                rawCoords, 
                coords,
                rawMin, 
                rawMax, 
                Vector2.zero, 
                axisLengthPx);
        }

        public void InitCoordCount(int count)
        {
            rawCoords = Enumerable.Repeat(Vector2.zero, count).ToList();
            coords    = Enumerable.Repeat(Vector2.zero, count).ToList();
        }

        public void SetRawCoord(Vector2 coord, int i)
        {
            rawCoords[i] = coord;
            coords[i] = new Vector2(
                (rawCoords[i].x + offsetX) * ratio.x,
                (rawCoords[i].y + offsetY) * ratio.y);

            if (rawMin.x > coord.x)
            {
                rawMin = new Vector2(coord.x, rawMin.y);
            }
            else if (rawMax.x < coord.x)
            {
                rawMax = new Vector2(coord.x, rawMax.y);
            }

            if (rawMin.y > coord.y)
            {
                rawMin = new Vector2(rawMin.x, coord.y);
            }
            else if (rawMax.y < coord.y)
            {
                rawMax = new Vector2(rawMax.x, coord.y);
            }
        }

        public void SetRawCoords(IEnumerable<Vector2> coords)
        {
            rawCoords   = new List<Vector2>(coords);
            this.coords = Enumerable.Repeat(Vector2.zero, rawCoords.Count).ToList();
            GetMinMax();

            rawCoordsRO = rawCoords;
            coordsRO = this.coords;
        }

        public void DrawInit(Canvas canvas, Transform parent, LineStyle style) 
            => line = Factory.Line(line, coords, canvas, parent, style, $"{label}_line", colorOverride);

        public void DestroyLine() 
            => VectorLine.Destroy(ref line);

        private void GetMinMax()
        {
            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float maxX = Mathf.NegativeInfinity;
            float maxY = Mathf.NegativeInfinity;

            for (int iCoord = 0; iCoord < rawCoords.Count; iCoord++)
            {
                float currX = rawCoords[iCoord].x;
                float currY = rawCoords[iCoord].y;

                minX = minX > currX ? currX : minX;
                minY = minY > currY ? currY : minY;
                maxX = maxX < currX ? currX : maxX;
                maxY = maxY < currY ? currY : maxY;
            }

            rawMin = new Vector2(minX, minY);
            rawMax = new Vector2(maxX, maxY);
        }
    }
}