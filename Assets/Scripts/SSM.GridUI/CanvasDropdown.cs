using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SSM.GraphDrawing;

namespace SSM.GridUI
{
    public class CanvasDropdown : MonoBehaviour
    {
        public CanvasSwitcher canvasSwitcher;
        public Dropdown dropdown;
        public Dictionary<int, GraphSubscriber> dict;

        public void OnValueChanged(Dropdown dropdown)
        {
            canvasSwitcher.gSubscriber = dict[dropdown.value];
            canvasSwitcher.Refresh();
        }

        private void SubscribeToDropdown()
        {
            dropdown.onValueChanged.AddListener(delegate { OnValueChanged(dropdown); });
        }

        private void Start()
        {
            SubscribeToDropdown();
        }

        private void OnEnable()
        {
            OnValueChanged(dropdown);
        }
    }
}
