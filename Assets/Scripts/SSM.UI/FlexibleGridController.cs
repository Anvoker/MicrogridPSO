using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SSM.GridUI
{
    [ExecuteInEditMode]
    public class FlexibleGridController : MonoBehaviour
    {
        public Canvas canvas;
        public int columns;
        public int rows;
        public float nudge = 0.0f;
        public GridLayoutGroup gridLayoutGroup;
        public RectTransform rt;
        private Vector2 cachedSizeDelta;

        private int cachedColumns;
        private int cachedRows;

        private void Awake()
        {
            gridLayoutGroup = gridLayoutGroup
                ?? GetComponent<GridLayoutGroup>();
        }

        private void LateUpdate()
        {
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            Canvas.ForceUpdateCanvases();
            if (cachedSizeDelta != rt.sizeDelta)
            {
                UpdateStep();
            }
            else if (cachedRows != rows || cachedColumns != columns)
            {
                cachedRows = rows;
                cachedColumns = columns;
                UpdateStep();
            }
        }

        private void UpdateStep()
        {
            float totalXSpacing = gridLayoutGroup.padding.left
                + gridLayoutGroup.padding.right
                + gridLayoutGroup.spacing.x
                * (rows - 1)
                + nudge;
            float totalYSpacing = gridLayoutGroup.padding.bottom
                + gridLayoutGroup.padding.top
                + gridLayoutGroup.spacing.y
                * (columns - 1)
                + nudge;

            var newSize = new Vector2((rt.rect.size.x - totalXSpacing) / rows,
                                      (rt.rect.size.y - totalYSpacing) / columns);

            gridLayoutGroup.cellSize = newSize;
            cachedSizeDelta = rt.rect.size;
        }
    }
}
