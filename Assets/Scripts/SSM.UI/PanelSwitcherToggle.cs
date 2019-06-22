using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SSM.GridUI
{
    public class PanelSwitcherToggle : MonoBehaviour
    {
        public List<Toggle> toggles;
        public List<RectTransform> rects;
        public Toggle defaultToggle;

        [SerializeField]
        private Toggle currentToggle;

        public void SetCurrentPanel(Toggle toggle)
        {
            if (toggles.Contains(toggle) && toggle.isOn)
            {
                DisableAllExcept(toggle);
                currentToggle = toggle;
                int i = toggles.IndexOf(toggle);
                rects[i].gameObject.SetActive(true);
            }
        }

        public Toggle GetCurrentPanel()
        {
            return currentToggle;
        }

        private void Start()
        {
            defaultToggle.isOn = true;
            if (currentToggle == null) { SetCurrentPanel(defaultToggle); }
        }

        private void OnEnable()
        {
            if (currentToggle != null && currentToggle.isOn)
            {
                int i = toggles.IndexOf(currentToggle);
                rects[i].gameObject.SetActive(true);
            }
        }

        private void DisableAllExcept(Toggle exception)
        {
            foreach (Toggle t in toggles)
            {
                if (t != exception)
                {
                    t.isOn = false;
                    int i = toggles.IndexOf(t);
                    rects[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
