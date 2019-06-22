using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SSM.Grid;

namespace SSM.GraphDrawing
{
    [RequireComponent(typeof(RectTransform))]
    public class ScheduleMapRow : MonoBehaviour
    {
        private List<Image> cells = new List<Image>();

        public void SetDataToCells(
            Image cellPrototype,
            int[,] binaryStates, 
            Color colorOn, 
            Color colorOff)
        {
            var rowCount = binaryStates.GetLength(1);
            EnsureImageCount(cellPrototype, rowCount);

            for (int iRow = 0; iRow < rowCount; iRow++)
            {
                cells[iRow].sprite = ScheduleTextureHelper.GetSprite(
                    MGHelper.GetCol(binaryStates, iRow), 
                    colorOn, 
                    colorOff);
            }
        }

        private void EnsureImageCount(Image cellPrototype, int desiredCount)
        {
            if (cells.Count > desiredCount)
            {
                var delta = cells.Count - desiredCount;

                for (int i = cells.Count - 1; i > desiredCount; i--)
                {
                    Destroy(cells[i].gameObject);
                }

                cells.RemoveRange(desiredCount, delta);
            }
            else if (cells.Count < desiredCount)
            {
                var delta = desiredCount - cells.Count;

                for (int i = 0; i < delta; i++)
                {
                    var img = Instantiate(cellPrototype);
                    img.transform.SetParent(transform, false);
                    cells.Add(img);
                }
            }
        }
    }
}
