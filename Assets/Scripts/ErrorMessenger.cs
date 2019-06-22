using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindowManager;

namespace SSM
{
    public class ErrorMessenger : MonoBehaviour
    {
        public void SendErrorMessage(string message) 
            => ShowPopUp(message, "Error");

        public void SendInfoMessage(string message)
            => ShowPopUp(message, "Info");

        private void ShowPopUp(string message, string titleText)
        {
            if (WindowsController.Instance.PopupController == null)
            {
                Debug.LogError("Needs an instance of PopupController to work.");
                return;
            }

            var newPopUp = new PopupDefinition();
            newPopUp.hasCancelButton = false;
            newPopUp.cancelText = "Cancel";
            newPopUp.confirmText = "OK";
            newPopUp.descriptionText = message;
            newPopUp.titleText = "Error";

            WindowsController.Instance.PopupController.Show(newPopUp, null, null, true);
        }
    }
}
