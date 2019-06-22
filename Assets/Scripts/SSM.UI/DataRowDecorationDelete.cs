using UnityEngine;
using UnityEngine.UI;

namespace SSM.UI
{
    public class DataRowDecorationDelete : DataRowDecoration
    {
        [SerializeField]
        protected Button deleteButton;

        private bool isSubscribedToButton;

        public override void Init(DataTable parent, int rowIndex)
        {
            base.Init(parent, rowIndex);
            if (gameObject.activeInHierarchy)
            {
                SetSubscriptionStatus(true);
            }
        }

        private void Delete()
        {
            parentTable.DeleteRow(rowIndex);
            parentTable.Submit();
        }

        private void SetSubscriptionStatus(bool isSubscribed)
        {
            if (isSubscribedToButton != isSubscribed)
            {
                if (isSubscribed)
                {
                    deleteButton.onClick.AddListener(Delete);
                    isSubscribedToButton = true;
                }
                else
                {
                    deleteButton.onClick.RemoveListener(Delete);
                    isSubscribedToButton = false;
                }
            }
        }

        private void OnEnable()
        {
            SetSubscriptionStatus(true);
        }

        private void OnDisable()
        {
            SetSubscriptionStatus(false);
        }
    }
}
