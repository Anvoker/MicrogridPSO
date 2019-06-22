using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WindowManager
{
    [Serializable]
    public class PopupController
    {
        private Action confirm;
        private Action cancel;

        [SerializeField]
        private List<PopupDefinition> definitions;

        [SerializeField]
        private UIPopup popup;
        private bool closeWindowsOnConfirm;

        private WindowsController windowController;

        public void Setup(UIPopup popup, WindowsController windowController)
        {
            if (popup != null)
                this.popup = popup;
            else
                Debug.LogWarning("There's no UIPopup in the scene.");

            this.windowController = windowController;
        }

        public void Show(PopupDefinitions definition, Action confirm, Action cancel, bool closeWindowsOnConfirm = false)
        {
            PopupDefinition def = definitions[(int)definition];
            this.closeWindowsOnConfirm = closeWindowsOnConfirm;
            this.confirm = confirm;
            this.cancel = cancel;

            if (popup != null)
                popup.Setup(def.titleText, def.descriptionText, def.confirmText, def.cancelText, Confirm, Cancel, def.hasCancelButton);
            else
                Debug.LogWarning("There's no UIPopup in the scene.");

            windowController.HideLastWindow();
        }

        public void Show(PopupDefinition def, Action confirm, Action cancel, bool closeWindowOnConfirm = false)
        {
            Debug.Log("show");
            this.closeWindowsOnConfirm = closeWindowOnConfirm;
            this.confirm = confirm;
            this.cancel = cancel;

            if (popup != null)
                popup.Setup(def.titleText, def.descriptionText, def.confirmText, def.cancelText, Confirm, Cancel, def.hasCancelButton);
            else
                Debug.LogWarning("There's no UIPopup in the scene.");

            windowController.HideLastWindow();
        }

        private void Confirm()
        {
            if (confirm != null)
                confirm();

            if (closeWindowsOnConfirm)
                windowController.DeleteNavigationHistory();
            else
                windowController.UnhideLastWindow();
        }

        public void Cancel()
        {
            if (cancel != null)
                cancel();

            windowController.UnhideLastWindow();
        }
    }
}