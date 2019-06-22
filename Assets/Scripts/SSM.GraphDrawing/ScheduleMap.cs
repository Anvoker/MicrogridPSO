using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SSM.Grid;
using TMPro;

namespace SSM.GraphDrawing
{
    public class ScheduleMap : MonoBehaviour
    {
        public int CountRow => rows.Count;
        public int CountCol => countCol;

        public RectTransform headRowPrototype;
        public ScheduleMapRow rowPrototype;
        public Image cellPrototype;
        public TMP_Text rowLabelPrototype;
        public TMP_Text colLabelPrototype;
        public RectTransform rowContainer;

        public Func<int, string> rowLabelTextGenerator = (i) => i.ToString();
        public Func<int, string> colLabelTextGenerator = (i) => "Gen. " + i.ToString();

        public Color colorOn;
        public Color colorOff;

        private int countCol;
        private RectTransform headRow;
        private List<ScheduleMapRow> rows = new List<ScheduleMapRow>();
        private List<TMP_Text> rowLabels = new List<TMP_Text>();
        private List<TMP_Text> colLabels = new List<TMP_Text>();

        public void SetData(int rowIndex, int[,] binaryStates) 
            => rows[rowIndex].SetDataToCells(cellPrototype, binaryStates, colorOn, colorOff);

        public void EnsureCount(int desiredRowCount, int desiredColCount)
        {
            if (rows.Count > desiredRowCount)
            {
                var delta = rows.Count - desiredRowCount;

                for (int i = rows.Count - 1; i > desiredRowCount; i--)
                {
                    Destroy(rows[i].gameObject);
                }

                rows.RemoveRange(desiredRowCount, delta);
                rowLabels.RemoveRange(desiredRowCount, delta);
            }
            else if (rows.Count < desiredRowCount)
            {
                var delta = desiredRowCount - rows.Count;

                for (int i = 0; i < delta; i++)
                {
                    var rowGO = Instantiate(rowPrototype);
                    rowGO.transform.SetParent(rowContainer, false);
                    var row = rowGO.GetComponent<ScheduleMapRow>();
                    rows.Add(row);

                    var rowLabel = Instantiate(rowLabelPrototype);
                    rowLabel.transform.SetParent(rowGO.transform, false);
                    rowLabel.text = rowLabelTextGenerator(i);
                    rowLabels.Add(rowLabel);
                }
            }

            if (rows.Count > desiredRowCount)
            {
                var delta = rows.Count - desiredRowCount;

                for (int i = rows.Count - 1; i > desiredRowCount; i--)
                {
                    Destroy(rows[i].gameObject);
                    Destroy(rowLabels[i].gameObject);
                }

                rows.RemoveRange(desiredRowCount, delta);
                rowLabels.RemoveRange(desiredRowCount, delta);
            }
            else if (rows.Count < desiredRowCount)
            {
                var delta = desiredRowCount - rows.Count;

                for (int i = 0; i < delta; i++)
                {
                    var rowGO = Instantiate(rowPrototype);
                    rowGO.transform.SetParent(rowContainer, false);
                    var row = rowGO.GetComponent<ScheduleMapRow>();
                    rows.Add(row);

                    var rowLabel = Instantiate(rowLabelPrototype);
                    rowLabel.transform.SetParent(rowGO.transform, false);
                    rowLabel.text = rowLabelTextGenerator(i);
                }
            }

            if (countCol > desiredColCount)
            {
                var delta = countCol - desiredColCount;

                for (int i = countCol - 1; i > desiredColCount; i--)
                {
                    Destroy(colLabels[i].gameObject);
                }

                colLabels.RemoveRange(desiredColCount, delta);
            }
            else if (countCol < desiredColCount)
            {
                var delta = desiredColCount - countCol;

                for (int i = 0; i < delta; i++)
                {
                    var colLabel = Instantiate(colLabelPrototype);
                    colLabel.transform.SetParent(headRow, false);
                    colLabel.text = colLabelTextGenerator(i);
                    colLabels.Add(colLabel);
                }
            }

            countCol = desiredColCount;
        }

        protected void Awake()
        {
            rowContainer = rowContainer ?? GetComponent<RectTransform>();
            headRow = Instantiate(headRowPrototype);
            headRow.SetParent(rowContainer, false);
            var rowLabel = Instantiate(rowLabelPrototype);
            rowLabel.transform.SetParent(headRow, false);
            rowLabel.text = "";
            rowLabel.color = Color.clear;
        }
    }
}
