using UnityEngine;

namespace SSM.UI
{
    public class DataRowDecoration : MonoBehaviour
    {
        public DecorationPosition Position
        {
            get => position;
            set
            {
                position = value;
                MoveToPosition();
            }
        }

        public DataTable Parent => parentTable;
        public int RowIndex => rowIndex;

        [SerializeField]
        protected DecorationPosition position;
        protected DataTable parentTable;
        protected int rowIndex;

        public virtual void Init(DataTable parentTable, int rowIndex)
        {
            this.parentTable = parentTable;
            this.rowIndex = rowIndex;

            MoveToPosition();
        }

        private void MoveToPosition()
        {
            switch (position)
            {
                case DecorationPosition.After:
                    transform.SetAsLastSibling();
                    break;

                case DecorationPosition.Before:
                    transform.SetAsFirstSibling();
                    break;
            }
        }
    }

    public enum DecorationPosition
    {
        Before,
        After
    }
}
