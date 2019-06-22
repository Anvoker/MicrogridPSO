using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using TMPro;
using System.Linq;

namespace SSM.GraphDrawing
{
    public class RadarCanvas : MonoBehaviour
    {
        public Radar radar;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public RectTransform surface;
        public RadarCanvasOptions options;
        public bool isDirty;

        [System.Serializable]
        public class RadarCanvasOptions
        {
            public float thicknessAxis = 2.0f;
            public float thicknessGrid = 2.0f;

            public GameObject axisLabelPrefab;
            public List<float> gridPointsNormalized = new List<float>();

            public RadarCanvasOptions() { }
        }

        [SerializeField] private float scaleX;
        [SerializeField] private float scaleY;
        [SerializeField] private Vector3 origin;

        private List<string> axisLabelsContent = new List<string>();
        private List<TMP_Text> axisLabels = new List<TMP_Text>();
        private List<VectorLine> axes = new List<VectorLine>();
        private List<VectorLine> grid = new List<VectorLine>();
            
        public void DrawRadar()
        {
            UpdateWorldCoords();

            var mesh = MakeRadarMesh(meshFilter.mesh, radar.GetLayerCoords(0, 0.0f), scaleX, scaleY);
            meshFilter.mesh = mesh;

            DrawAxes();
            DrawGrid();
            DrawAxisLabels();
        }

        public void DrawGrid()
        {
            EnsureGridCount();

            for (int iGrid = 0; iGrid < grid.Count; iGrid++)
            {
                float angle = 0.0f;
                float inc = 360.0f / radar.Dimensions;
                var norm = options.gridPointsNormalized[iGrid];
                Vector2 extent2D;
                Vector3 extent;

                grid[iGrid].points3 = new List<Vector3>(radar.Dimensions + 1);
                for (int iPoint = 0; iPoint < radar.Dimensions; iPoint++, angle += inc)
                {
                    extent2D = MathHelper.PolarToCartesian(angle, norm);
                    extent = new Vector3(
                        extent2D.x * scaleX / 2.0f,
                        extent2D.y * scaleY / 2.0f,
                        0.0f);
                    grid[iGrid].points3.Add(extent);
                }

                extent2D = MathHelper.PolarToCartesian(angle, norm);
                extent = new Vector3(
                    extent2D.x * scaleX / 2.0f,
                    extent2D.y * scaleY / 2.0f,
                    0.0f);
                grid[iGrid].points3.Add(extent);
                grid[iGrid].SetWidth(options.thicknessGrid);
                grid[iGrid].name = $"RadarGrid_{iGrid}";
                grid[iGrid].lineType = LineType.Continuous;
                grid[iGrid].color = Color.black;
                grid[iGrid].Draw3DAuto();
                grid[iGrid].material.renderQueue = 4100;
                grid[iGrid].rectTransform.SetParent(transform, false);
                grid[iGrid].rectTransform.localPosition = Vector3.zero;
            }
        }

        public void DrawAxes()
        {
            EnsureAxesCount();

            float angle = 0.0f;
            float inc = 360.0f / radar.Dimensions;

            for (int i = 0; i < radar.Dimensions; i++, angle += inc)
            {
                var extent2D = MathHelper.PolarToCartesian(angle, 1.0f);
                var extent = new Vector3(
                    extent2D.x * scaleX / 2.0f,
                    extent2D.y * scaleY / 2.0f, 
                    0.0f);

                axes[i].points3[0] = Vector3.zero;
                axes[i].points3[1] = extent;
                axes[i].SetWidth(options.thicknessAxis);
                axes[i].name = $"RadarAxis_{i}";
                axes[i].lineType = LineType.Continuous;
                axes[i].color = Color.black;
                axes[i].Draw3DAuto();
                axes[i].material.renderQueue = 4100;
                axes[i].rectTransform.SetParent(transform, false);
                axes[i].rectTransform.localPosition = Vector3.zero;
            }
        }

        public void DrawAxisLabels()
        {
            EnsureAxisLabelsCount();

            for (int iAxis = 0; iAxis < radar.Dimensions; iAxis++)
            {
                Vector3 pos = GetUIPosition(iAxis, 1.165f);
                axisLabels[iAxis].rectTransform.SetAnchor(AnchorPresets.MiddleCenter);
                axisLabels[iAxis].rectTransform.SetPivot(PivotPresets.MiddleCenter);
                axisLabels[iAxis].rectTransform.anchoredPosition3D = pos;
                axisLabels[iAxis].rectTransform.localScale = Vector3.one;
                axisLabels[iAxis].text = axisLabelsContent[iAxis];
            }
        }

        protected void Update()
        {
            if (isDirty || transform.hasChanged)
            {
                DrawRadar();
                isDirty = false;
                transform.hasChanged = false;
            }
        }

        protected void Start()
        {
            meshFilter = meshFilter ?? GetComponent<MeshFilter>();
            meshRenderer = meshRenderer ?? GetComponent<MeshRenderer>();
            options = options ?? new RadarCanvasOptions();

            axes = axes ?? new List<VectorLine>(radar?.Dimensions ?? 0);
            grid = grid ?? new List<VectorLine>(options.gridPointsNormalized.Count);
        }

        protected void OnDestroy()
        {
            foreach (var line in axes)
            {
                line.StopDrawing3DAuto();
            }

            foreach (var line in grid)
            {
                line.StopDrawing3DAuto();
            }

            VectorLine.Destroy(axes);
            VectorLine.Destroy(grid);
        }

        private Vector3 GetWorldPosition(int axisIndex, float norm)
        {
            var angle = axisIndex * 360.0f / radar.Dimensions;
            return GetWorldPosition(angle, norm);
        }

        private Vector3 GetWorldPosition(float angle, float norm)
        {
            var extent2D = MathHelper.PolarToCartesian(angle, norm);
            return new Vector3(
                extent2D.x * scaleX / 2.0f,
                extent2D.y * scaleY / 2.0f,
                0.0f);
        }

        private Vector3 GetUIPosition(int axisIndex, float norm)
        {
            var angle = axisIndex * 360.0f / radar.Dimensions;
            return GetUIPosition(angle, norm);
        }

        private Vector3 GetUIPosition(float angle, float norm)
        {
            var extent2D = MathHelper.PolarToCartesian(angle, norm);
            return new Vector3(
                extent2D.x * surface.rect.width  / 2.0f,
                extent2D.y * surface.rect.height / 2.0f,
                0.0f);
        }

        private void UpdateWorldCoords()
        {
            var fourCorners = new Vector3[4];
            surface.GetWorldCorners(fourCorners);

            var widthVector  = fourCorners[3] - fourCorners[0];
            var heightVector = fourCorners[1] - fourCorners[0];

            scaleX = widthVector.magnitude;
            scaleY = heightVector.magnitude;
            origin = fourCorners[0] + ((fourCorners[2] - fourCorners[0]) / 2.0f);
            origin = new Vector3(origin.x, origin.y, 0.0f);

            transform.position = origin;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                transform.localPosition.y, 
                0.0f);
        }

        private void EnsureGridCount()
        {
            var desiredCount = options.gridPointsNormalized.Count;

            if (grid.Count > desiredCount)
            {
                var delta = grid.Count - desiredCount;

                for (int i = grid.Count - 1; i >= desiredCount; i--)
                {
                    var line = grid[i];
                    VectorLine.Destroy(ref line);
                }

                grid.RemoveRange(desiredCount, delta);
            }
            else if (grid.Count < desiredCount)
            {
                var delta = desiredCount - grid.Count;

                for (int i = 0; i < delta; i++)
                {
                    var line = new VectorLine(name, new List<Vector3>(radar.Dimensions + 1), 1.0f);
                    grid.Add(line);
                }
            }
        }

        private void EnsureAxesCount()
        {
            var desiredCount = radar.Dimensions;

            if (axes.Count > desiredCount)
            {
                var delta = axes.Count - desiredCount;
                for (int i = axes.Count - 1; i >= desiredCount; i--)
                {
                    var line = axes[i];
                    VectorLine.Destroy(ref line);
                }

                axes.RemoveRange(desiredCount, delta);
            }
            else if (axes.Count < desiredCount)
            {
                var delta = desiredCount - axes.Count;

                for (int i = 0; i < delta; i++)
                {
                    var line = new VectorLine(name, new List<Vector3>(2), 1.0f);
                    axes.Add(line);
                }
            }
        }

        private void EnsureAxisLabelsCount()
        {
            var desiredCount = radar.Dimensions;

            if (axisLabels.Count > desiredCount)
            {
                var delta = axisLabels.Count - desiredCount;
                for (int i = axisLabels.Count - 1; i >= desiredCount; i--)
                {
                    Destroy(axisLabels[i].gameObject);
                }

                axisLabels.RemoveRange(desiredCount, delta);
            }
            else if (axisLabels.Count < desiredCount)
            {
                var delta = desiredCount - axisLabels.Count;

                for (int i = 0; i < delta; i++)
                {
                    var label = Instantiate(options.axisLabelPrefab);
                    axisLabels.Add(label.GetComponent<TMP_Text>());
                    label.transform.SetParent(surface);
                }
            }

            if (axisLabelsContent.Count > desiredCount)
            {
                var delta = axisLabelsContent.Count - desiredCount;
                axisLabelsContent.RemoveRange(desiredCount, delta);
            }
            else if (axisLabelsContent.Count < desiredCount)
            {
                axisLabelsContent = Enumerable.Range(1, desiredCount + 1)
                    .Select(x => x.ToString())
                    .ToList();
            }
        }

        private Mesh MakeRadarMesh(Mesh mesh, IList<Vector2> vertices, float scaleX, float scaleY)
        {
            ScaleVertices(vertices, scaleX, scaleY);
            AddCenterVertex(vertices);

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.MarkDynamic();
            }

            mesh.Clear();
            mesh.name = $"Radar_{gameObject.GetInstanceID()}";
            mesh.vertices = Vector2DTo3D(vertices);
            mesh.triangles = MakeTriArray_CenterFan(vertices.Count);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshRenderer.sharedMaterial.renderQueue = 3500;
            mesh.UploadMeshData(false);

            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(
                1.0f / transform.lossyScale.x, 
                1.0f / transform.lossyScale.y, 
                1.0f / transform.lossyScale.z);

            return mesh;
        }

        private int[] MakeTriArray_CenterFan(int vertexCount)
        {
            var tris = new int[3 * (vertexCount - 1)];
            int iTri = 0;

            for (int i = 2; i < tris.Length; i += 3, iTri++)
            {
                tris[i - 0] = vertexCount - 1;
                tris[i - 1] = iTri;
                tris[i - 2] = (iTri + 1) % (vertexCount - 1);
            }

            return tris;
        }

        private Vector3[] Vector2DTo3D(IList<Vector2> vertices)
        {
            Vector3[] vertices3D = new Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices3D[i] = new Vector3(
                    vertices[i].x, 
                    vertices[i].y, 
                    0.0f);
            }

            return vertices3D;
        }

        private IList<Vector2> AddCenterVertex(IList<Vector2> vertices)
        {
            vertices.Add(new Vector2(0.0f, 0.0f));
            return vertices;
        }

        private IList<Vector2> ScaleVertices(IList<Vector2> vertices, float scaleX, float scaleY)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new Vector2(
                    vertices[i].x * scaleX / 2.0f, 
                    vertices[i].y * scaleY / 2.0f);
            }

            return vertices;
        }
    }
}
