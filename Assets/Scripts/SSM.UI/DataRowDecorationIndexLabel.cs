using UnityEngine;
using TMPro;

namespace SSM.UI
{
    public class DataRowDecorationIndexLabel : DataRowDecoration
    {
        [SerializeField]
        protected TMP_Text label;

        public override void Init(DataTable parent, int rowIndex)
        {
            base.Init(parent, rowIndex);
            label.text = rowIndex.ToString();
        }
    }
}