using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WindowManager;

namespace SSM.UI
{
    public class ThermalGeneratorPickerWindow : UIWindow
    {
        [SerializeField] private RectTransform generatorContainer;

        [SerializeField] private Button submitButton;
        [SerializeField] private Toggle generatorTogglePrototype;

        private Dictionary<Toggle, Tuple<string, ThermalGenerator>> generatorToggles
            = new Dictionary<Toggle, Tuple<string, ThermalGenerator>>();
        private List<Action<string, ThermalGenerator>> onCloseCallbacks
            = new List<Action<string, ThermalGenerator>>();

        private ThermalGenerator selectedGenerator;
        private string selectedGeneratorName;

        public void Show(Action<string, ThermalGenerator> generatorPickCallback)
        {
            onCloseCallbacks.Add(generatorPickCallback);
            Show();
        }

        protected override void Setup()
        {
            submitButton.onClick.AddListener(OnSubmit);
            PopulateGeneratorList();
        }

        private void OnSubmit()
        {
            onCloseCallbacks.ForEach(x => x.Invoke(selectedGeneratorName, selectedGenerator));
            Close();
        }

        private void PopulateGeneratorList()
        {
            foreach (var gen in ThermalGeneratorPresets.EnumeratePresets())
            {
                var toggle = Instantiate(generatorTogglePrototype);
                toggle.transform.SetParent(generatorContainer);
                toggle.GetComponentInChildren<TMP_Text>().text = gen.Key;
                toggle.onValueChanged.AddListener((bool isOn) => OnGeneratorToggle(toggle, isOn));
                generatorToggles.Add(toggle, Tuple.Create(gen.Key, gen.Value));
            }
        }

        private void OnGeneratorToggle(Toggle t, bool isOn)
        {
            if (isOn)
            {
                foreach (var kvp in generatorToggles)
                {
                    if (kvp.Key != t)
                    {
                        kvp.Key.isOn = false;
                    }
                }

                selectedGeneratorName = generatorToggles[t].Item1;
                selectedGenerator = generatorToggles[t].Item2;
            }
        }
    }

}