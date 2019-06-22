using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace SSM.UI
{
    public class DataTable : MonoBehaviour
    {
        public int       CountRows     => countRows;
        public int       CountCols     => countCols;
        public InputCell PrototypeCell => prototypeCell;
        public DataRow   PrototypeRow  => prototypeRow;

        public string defaultValue;
        public RectTransform rowContainer;
        public bool autoSubmit;
        public bool isSubmitDisabled;
        public event Action<IEnumerable<Tuple<int, IEnumerable<string>>>> OnTableChanged;

        [SerializeField] private InputCell prototypeCell;
        [SerializeField] private List<DataRowDecoration> prototypeDecorations;

        [SerializeField] private DataRow prototypeRow;
        private List<DataRow> rows = new List<DataRow>();

        private int countRows;
        private int countCols;

        public void SetRow(int i, IEnumerable<string> cellData)
        {
            _ = cellData ?? throw new ArgumentNullException(nameof(cellData));

            rows[i].SetData(cellData);
        }

        public void SetColCount(int countCols)
        {
            _ = countCols < 0 ? throw new ArgumentException(nameof(countCols)) : 0;

            if (countCols == this.countCols) { return; }

            var countRows = this.countRows;

            DestroyRows();

            this.countCols = countCols;

            SetRowCount(countRows);
        }

        public void SetRowCount(int desiredCount)
        {
            if (desiredCount < CountRows)
            {
                DestroyRows();
            }

            if (desiredCount > CountRows)
            {
                AddRows(desiredCount - CountRows);
            }
        }

        public void Submit()
        {
            if (!isSubmitDisabled)
            {
                OnTableChanged?.Invoke(EnumerateCells());
            }
        }

        public void AddRows(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddRow();
            }
        }

        public void AddRow()
        {
            var newRow = Instantiate(PrototypeRow, rowContainer);
            rows.Add(newRow);
            newRow.InitCount(prototypeCell, CountCols);
            newRow.SetData(Enumerable.Repeat(defaultValue, countCols));
            newRow.SetInputEvents(OnCellEdit);

            foreach (var decoration in prototypeDecorations)
            {
                newRow.AddDecorationCell(decoration, this, countRows);
            }

            countRows++;
        }

        public void DeleteRow(int iRow)
        {
            Destroy(rows[iRow].gameObject);
            rows.RemoveAt(iRow);
            countRows--;

            for (int i = iRow; i < countRows; i++)
            {
                rows[i].RemoveInputEvents();
                rows[i].SetInputEvents(OnCellEdit);
                rows[i].RefreshDecorationCells(this, i);
            }
        }

        private void DestroyRows()
        {
            countRows = 0;
            rows.ForEach(row => Destroy(row.gameObject));
            rows.Clear();
        }

        private void OnCellEdit(int i, string labelText)
        {
            if (autoSubmit) { Submit(); }
        }

        private IEnumerable<Tuple<int, IEnumerable<string>>> EnumerateCells()
        {
            for (int iRow = 0; iRow < CountRows; iRow++)
            {
                var text = rows[iRow].GetInputText();
                yield return Tuple.Create(iRow, text);
            }
        }
    }
}