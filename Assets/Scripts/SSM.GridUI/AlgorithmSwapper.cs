using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using SSM.Grid;

namespace SSM.GridUI
{
    public class AlgorithmSwapper : MonoBehaviour
    {
        public Microgrid microgrid;
        public MicrogridAlgorithm algorithm;
        public List<Toggle> togglesUC;
        public List<Toggle> togglesED;
        public List<RectTransform> panels;

        public void SwitchUCPanels(Toggle toggle)
        {
            foreach (RectTransform rt in panels)
            {
                rt.gameObject.SetActive(false);
            }

            panels[togglesUC.IndexOf(toggle)].gameObject.SetActive(true);
        }

        public void SwitchUCAlgorithm(Toggle toggle)
        {
            if (toggle.isOn)
            {
                microgrid.calculator.UCAlgorithm
                    = (UCAlgorithms)togglesUC.IndexOf(toggle);
                SwitchUCPanels(toggle);

                foreach (var t in togglesUC)
                {
                    if (toggle != t)
                    {
                        var e = t.onValueChanged;
                        var eCount = e.GetPersistentEventCount();
                        for (int i = 0; i < eCount; i++)
                        {
                            e.SetPersistentListenerState(i, 
                                UnityEventCallState.Off);
                        }

                        t.isOn = false;

                        for (int i = 0; i < eCount; i++)
                        {
                            e.SetPersistentListenerState(i,
                                UnityEventCallState.RuntimeOnly);
                        }
                    }
                }
            }
        }

        public void SwitchEDAlgorithm(Toggle toggle)
        {
            if (toggle.isOn)
            {
                microgrid.calculator.EDAlgorithm
                    = (EDAlgorithms)togglesED.IndexOf(toggle);

                foreach (var t in togglesED)
                {
                    if (toggle != t)
                    {
                        var e = t.onValueChanged;
                        t.onValueChanged = null;
                        t.isOn = false;
                        t.onValueChanged = e;
                    }
                }
            }
        }

        protected void OnEnable()
        {
            ReadUC();
        }

        protected void Awake()
        {
            microgrid = microgrid ?? FindObjectOfType<Microgrid>();
            algorithm = algorithm ?? FindObjectOfType<MicrogridAlgorithm>();
        }

        private void ReadUC()
        {
            var uc = algorithm.UCAlgorithm;
            var toggle = togglesUC[(int)uc];

            foreach (var t in togglesUC)
            {
                var e = t.onValueChanged;
                var eCount = e.GetPersistentEventCount();
                for (int i = 0; i < eCount; i++)
                {
                    e.SetPersistentListenerState(i,
                        UnityEventCallState.Off);
                }

                t.isOn = (toggle == t);

                for (int i = 0; i < eCount; i++)
                {
                    e.SetPersistentListenerState(i,
                        UnityEventCallState.RuntimeOnly);
                }
            }
        }
    }
}
