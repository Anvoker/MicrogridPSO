using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

namespace SSM.UI
{
    [RequireComponent(typeof(DataTable))]
    public abstract class DataTableSource : MonoBehaviour
    {
        protected DataTable dataTable;

        protected void Awake()
        {
            dataTable = GetComponent<DataTable>();
        }

        protected void OnEnable()
        {
            dataTable.OnTableChanged += WriteToModel;
        }

        protected void OnDisable()
        {
            dataTable.OnTableChanged -= WriteToModel;
        }

        protected abstract void WriteToModel(IEnumerable<Tuple<int, IEnumerable<string>>> obj);
    }
}