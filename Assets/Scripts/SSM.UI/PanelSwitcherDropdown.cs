using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace SSM.GridUI
{
    public class PanelSwitcherDropdown : MonoBehaviour
    {
        public Dropdown dropdown;
        public ProviderRectString provider;
        private Dictionary<int, RectTransform> dict;

        public void PopulateOptions()
        {
            int count = provider.dict.Values.Count;
            var strings = provider.dict.Values.ToArray();
            var rts = provider.dict.Keys.ToArray();

            dropdown.ClearOptions();
            dropdown.AddOptions(strings.ToList());
            dict = new Dictionary<int, RectTransform>();
            for (int i = 0; i < strings.Length; i++)
            {
                dict.Add(i, rts[i]);
            }
        }

        public void SubscribeToDropdown()
        {
            dropdown.onValueChanged.AddListener(delegate { OnValueChanged(dropdown); });
        }

        public void OnValueChanged(Dropdown dropdown)
        {
            foreach(RectTransform rt in dict.Values)
            {
                rt.gameObject.SetActive(false);
            }
            dict[dropdown.value].gameObject.SetActive(true);
        }
    }

    public interface IDropDownOptionsProvider<T>
    {
        Dictionary<T, string> dict { get; set; }
    }

    public class ProviderRectString : MonoBehaviour, IDropDownOptionsProvider<RectTransform>
    {
        public Dictionary<RectTransform, string> dict
        {
            get { return _dict; }
            set { _dict = value; }
        }

        [SerializeField]
        private Dictionary<RectTransform, string> _dict;
    }
}
