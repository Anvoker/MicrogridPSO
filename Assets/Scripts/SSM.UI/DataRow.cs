using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace SSM.UI
{
    public class DataRow : MonoBehaviour
    {
        private List<InputCell> inputCells = new List<InputCell>();
        private List<DataRowDecoration> decorationCells = new List<DataRowDecoration>();

        public void AddDecorationCell(
            DataRowDecoration decoration, 
            DataTable table, 
            int rowIndex)
        {
            var decorationClone = Instantiate(decoration, transform);
            decorationCells.Add(decorationClone);
            decorationClone.Init(table, rowIndex);
        }

        public void RefreshDecorationCells(
            DataTable table,
            int rowIndex)
        {
            foreach (var decorationCell in decorationCells)
            {
                decorationCell.Init(table, rowIndex);
            }
        }

        public IEnumerable<string> GetInputText()
        {
            foreach (var cell in inputCells)
            {
                yield return cell.inputField.text;
            }
        }

        public string GetInputText(int i)
        {
            return inputCells[i].inputField.text;
        }

        public void InitCount(
            InputCell inputCellPrototype, 
            int count)
        {
            _ = inputCellPrototype ?? throw new ArgumentNullException(nameof(inputCellPrototype));
            _ = count < 0 ? throw new ArgumentException(nameof(count)) : 0;

            DestroyCells();

            for (int i = 0; i < count; i++)
            {
                var cell = Instantiate(inputCellPrototype, transform);
                inputCells.Add(cell);
            }
        }

        public void SetData(IEnumerable<string> labelsText)
        {
            _ = labelsText ?? throw new ArgumentNullException(nameof(labelsText));

            int i = 0;

            foreach (var labelText in labelsText)
            {
                inputCells[i].inputField.text = labelText;
                i++;
            }
        }

        public void SetInputEvents(Action<int, string> callback)
        {
            for (int i = 0; i < inputCells.Count; i++)
            {
                var e = inputCells[i].inputField.onEndEdit;
                e.AddListener(PartialApply(callback, i));
            }
        }

        public void RemoveInputEvents()
        {
            for (int i = 0; i < inputCells.Count; i++)
            {
                var e = inputCells[i].inputField.onEndEdit;
                e.RemoveAllListeners();
            }
        }

        private void OnDestroy()
        {
            RemoveInputEvents();
        }

        private UnityAction<T> PartialApply<T>(Action<int, T> action, int i) 
            => new UnityAction<T>((T s) => action?.Invoke(i, s));

        private void DestroyCells()
        {
            inputCells.ForEach(cell => Destroy(cell));
            inputCells.Clear();
        }
    }
}
