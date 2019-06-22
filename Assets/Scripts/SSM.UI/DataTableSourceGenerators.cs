using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using SSM.Grid;

namespace SSM.UI
{
    [RequireComponent(typeof(DataTable))]
    public class DataTableSourceGenerators : DataTableSource
    {
        private Microgrid microgrid;
        private MGInput Model => microgrid.Input;

        protected new void Awake()
        {
            base.Awake();
            microgrid = microgrid ?? FindObjectOfType<Microgrid>();
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            microgrid.OnLoaded += OnMGLoaded;
            ReadFromModel();
        }

        protected new void OnDisable()
        {
            base.OnDisable();
            microgrid.OnLoaded -= OnMGLoaded;
        }

        private void OnMGLoaded(object sender, Microgrid.LoadedEventArgs e) => ReadFromModel();

        public void ReadFromModel()
        {
            if ((Model.p_thr_max     == null || Model.p_thr_max.Length <= 0)     ||
                (Model.thr_min_dtime == null || Model.thr_min_dtime.Length <= 0) ||
                (Model.thr_min_utime == null || Model.thr_min_utime.Length <= 0) ||
                (Model.thr_c_a       == null || Model.thr_c_a.Length <= 0)       ||
                (Model.thr_c_b       == null || Model.thr_c_b.Length <= 0)       ||
                (Model.thr_c_c       == null || Model.thr_c_c.Length <= 0))
            {
                return;
            }

            dataTable.isSubmitDisabled = true;
            dataTable.SetColCount(6);
            dataTable.SetRowCount(Model.genCount);

            for (int i = 0; i < Model.genCount; i++)
            {
                dataTable.SetRow(i, ReadRowFromModel(i));
            }

            dataTable.isSubmitDisabled = false;
        }

        private IEnumerable<string> ReadRowFromModel(int i)
        {
            yield return Model.p_thr_max[i].ToString();
            yield return Model.thr_min_dtime[i].ToString();
            yield return Model.thr_min_utime[i].ToString();
            yield return Model.thr_c_a[i].ToString();
            yield return Model.thr_c_b[i].ToString();
            yield return Model.thr_c_c[i].ToString();
        }

        protected override void WriteToModel(
            IEnumerable<Tuple<int, IEnumerable<string>>> rows)
        {
            var input = microgrid.Input;

            input.genCount      = dataTable.CountRows;
            input.p_thr_max     = new float[input.genCount];
            input.thr_min_dtime = new float[input.genCount];
            input.thr_min_utime = new float[input.genCount];
            input.thr_c_a       = new float[input.genCount];
            input.thr_c_b       = new float[input.genCount];
            input.thr_c_c       = new float[input.genCount];

            foreach (var row in rows)
            {
                int i     = row.Item1;
                var cells = row.Item2.ToList();

                input.p_thr_max[i]     = float.Parse(cells[0]);
                input.thr_min_dtime[i] = float.Parse(cells[1]);
                input.thr_min_utime[i] = float.Parse(cells[2]);
                input.thr_c_a[i]       = float.Parse(cells[3]);
                input.thr_c_b[i]       = float.Parse(cells[4]);
                input.thr_c_c[i]       = float.Parse(cells[5]);
            }

            input.dirty = true;
        }
    }
}