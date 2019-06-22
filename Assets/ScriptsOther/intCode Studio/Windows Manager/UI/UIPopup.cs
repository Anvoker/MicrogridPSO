using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;

namespace WindowManager
{
    public class UIPopup : MonoBehaviour
    {
        private Action confirm;
        private Action cancel;

        [SerializeField]
        private UIWindowBackground background;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private string showTriggerName = "show";

        [SerializeField]
        private string closeTriggerName = "close";

        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private TMP_Text description;

        [SerializeField]
        private TMP_Text confirmButtonText;

        [SerializeField]
        private TMP_Text cancelButtonText;

        private void Awake()
        {
            SetPopupActive(false);
        }

        public void Setup(string titleText, string descriptionText, string confirmText, string cancelText, Action confirm, Action cancel, bool hasCancelButton)
        {
            Debug.Log("setup");
            Show();

            title.text = titleText;
            description.text = descriptionText;
            this.confirm = confirm;
            this.cancel = cancel;
            this.confirmButtonText.text = confirmText;
            this.cancelButtonText.text = cancelText;

            if (!hasCancelButton)
            {
                cancelButtonText.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                cancelButtonText.transform.parent.gameObject.SetActive(true);
            }
        }

        private void Show()
        {
            SetPopupActive(true);
            animator.SetTrigger(showTriggerName);
            background.FadeIn();
        }

        public void Confirm()
        {
            confirm?.Invoke();

            Close();
        }

        public void Cancel()
        {
            cancel?.Invoke();

            Close();
        }

        private void Close()
        {
            animator.SetTrigger(closeTriggerName);
            background.FadeOut();
            StartCoroutine(Tools.GetMethodName(DeactivateWindow));
        }

        private IEnumerator DeactivateWindow()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            SetPopupActive(false);
        }

        private void SetPopupActive(bool active)
        {
            gameObject.SetActive(active);
            background.gameObject.SetActive(active);
        }
    }
}