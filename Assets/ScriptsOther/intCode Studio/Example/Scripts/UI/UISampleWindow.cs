using UnityEngine;
using System.Collections;

namespace WindowManager
{
    public class UISampleWindow : UIWindow
    {
        protected override void Setup()
        {
            Debug.Log("Updated window content.");
        }

        public void ShowConfirmPopupWithoutCloseWindows()
        {
            WindowsController.Instance.PopupController.Show(PopupDefinitions.ConfirmExample, PrintConfirmationMessage, null);
        }

        public void ShowConfirmPopupCloseWindows()
        {
            WindowsController.Instance.PopupController.Show(PopupDefinitions.ConfirmExample, PrintConfirmationMessage, null, true);
        }

        public void ShowConfirmCancelPopupWithoutCloseWindows()
        {
            WindowsController.Instance.PopupController.Show(PopupDefinitions.ConfirmCancelExample, PrintBuyMessage, PrintCancelMessage);
        }

        public void ShowConfirmCancelPopupCloseWindows()
        {
            WindowsController.Instance.PopupController.Show(PopupDefinitions.ConfirmCancelExample, PrintBuyMessage, PrintCancelMessage, true);
        }

        private void PrintConfirmationMessage()
        {
            Debug.Log("Confirmed!");
        }

        private void PrintBuyMessage()
        {
            Debug.Log("Bought it!");
        }

        private void PrintCancelMessage()
        {
            Debug.Log("Cancelled!");
        }
    }
}